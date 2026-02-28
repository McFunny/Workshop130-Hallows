using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class EndCutsceneObjectToggler : MonoBehaviour
{
    public static EndCutsceneObjectToggler Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject postFinaleObj;

    [Tooltip("Objects to disable during the end cutscene.")]
    [SerializeField] private List<GameObject> disableForCutscene = new List<GameObject>();

    [Header("Auto-Populate")]
    [SerializeField] private string disableTag = "DisableForEndCutscene";

    private void Awake()
    {
        // Global singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        postFinaleObj?.SetActive(false);
    }

    /// <summary>
    /// Called when the end cutscene finishes.
    /// Disables everything in DisableForCutscene and enables PostFinaleObj.
    /// </summary>
    public void OnEndCutscene()
    {
        // If empty, try to populate at runtime (in case designer forgot).
        if (disableForCutscene == null || disableForCutscene.Count == 0)
        {
            TryPopulateDisableList();
        }

        ApplyFinaleState(isPostFinale: true);
    }

    /// <summary>
    /// Resets back to pre-finale state.
    /// Re-enables DisableForCutscene objects and disables PostFinaleObj.
    /// </summary>
    public void Reset()
    {
        ApplyFinaleState(isPostFinale: false);
    }

    private void ApplyFinaleState(bool isPostFinale)
    {

        if (disableForCutscene != null)
        {
            for (int i = disableForCutscene.Count - 1; i >= 0; i--)
            {
                GameObject obj = disableForCutscene[i];
                if (obj == null)
                {
                    disableForCutscene.RemoveAt(i);
                    continue;
                }

                obj.SetActive(!isPostFinale);
            }
        }

        if (postFinaleObj != null)
        {
            postFinaleObj.SetActive(isPostFinale);
        }
    }

    private void TryPopulateDisableList()
    {
        if (disableForCutscene == null)
            disableForCutscene = new List<GameObject>();
        else
            disableForCutscene.Clear();

        if (string.IsNullOrWhiteSpace(disableTag))
        {
            return;
        }

        GameObject[] found = GameObject.FindGameObjectsWithTag(disableTag);

        for (int i = 0; i < found.Length; i++)
        {
            GameObject go = found[i];
            if (go != null && go != gameObject)
                disableForCutscene.Add(go);
        }

        if (disableForCutscene.Count == 0)
        {
            Debug.LogWarning(
                $"{nameof(EndCutsceneObjectToggler)}: No active objects found with tag '{disableTag}'. " +
                $"(Note: inactive objects won't be found by FindGameObjectsWithTag.)");
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(EndCutsceneObjectToggler))]
    private class EndCutsceneObjectTogglerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EndCutsceneObjectToggler t = (EndCutsceneObjectToggler)target;

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Populate DisableForCutscene From Tag"))
            {
                Undo.RecordObject(t, "Populate DisableForCutscene");
                t.TryPopulateDisableList();
                EditorUtility.SetDirty(t);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test: OnEndCutscene()"))
            {
                t.OnEndCutscene();
            }
            if (GUILayout.Button("Test: Reset()"))
            {
                t.Reset();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}