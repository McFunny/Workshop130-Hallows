using UnityEditor;
using UnityEngine;

public class ScaleMultiplierTool : EditorWindow
{
    private float minMultiplier = 0.7f;
    private float maxMultiplier = 0.8f;

    [MenuItem("Tools/Scale Multiplier Tool")]
    public static void ShowWindow()
    {
        GetWindow<ScaleMultiplierTool>("Scale Multiplier Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Uniform Scale Multiplier", EditorStyles.boldLabel);

        minMultiplier = EditorGUILayout.FloatField("Min Multiplier", minMultiplier);
        maxMultiplier = EditorGUILayout.FloatField("Max Multiplier", maxMultiplier);

        if (minMultiplier < 0f) minMultiplier = 0f;
        if (maxMultiplier < minMultiplier) maxMultiplier = minMultiplier;

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Random Scale"))
        {
            ApplyScaleMultiplier();
        }
    }

    private void ApplyScaleMultiplier()
    {
        Transform[] selectedObjects = Selection.transforms;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
            return;
        }

        Undo.RecordObjects(selectedObjects, "Apply Random Scale Multiplier");

        foreach (Transform obj in selectedObjects)
        {
            float randomMultiplier = Random.Range(minMultiplier, maxMultiplier);
            obj.localScale *= randomMultiplier; // ✅ just multiply it — simple and uniform
        }

        //EditorUtility.DisplayDialog("Scaling Complete", "Applied uniform random scaling to selected objects.", "OK");
    }
}
