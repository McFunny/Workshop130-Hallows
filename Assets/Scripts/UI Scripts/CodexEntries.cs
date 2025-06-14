using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class CodexEntries : ScriptableObject
{
    //public GameObject entryButton;
    public Sprite buttonIcon;

    [Tooltip("Unlocked??? Yes or... no....? (True is yes, false is no)")]
    public bool unlocked = false;

    [Tooltip("The assigned image. Will convert first page to a large page if left empty.")]
    public Sprite mainImage;

    [Tooltip("Name of the entry personally I thought this was pretty self explanatory tho")]
    public string entryName;

    public enum EntryType
    {
        Tutorial,
        Tool,
        Structure,
        Plant,
        Creature,
    }
    public EntryType entryType;

    public CropData cropData;
    public CreatureObject creatureData;
    public StructureObject structureData;

    [Tooltip("Unused in new Codex, but used in old Codex.")]
    [TextArea(4, 10)]
    public string[] description;

    [TextArea(4, 10)]
    public string leftText, rightText;

    public void CopyOldDescription()
    {
        if (description.Length == 1)
        {
            rightText = description[0];
        }
        else
        {
          for (int i = 0; i < description.Length; i++)
            {
                if (i == 0)
                {
                    leftText = description[i];
                }
                else if (i == 1)
                {
                    rightText = description[i];
                }
                else
                {
                    Debug.LogWarning("CodexEntries: More than 2 description lines found, only the first two will be used.");
                    break;
                }
            }  
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(CodexEntries), true)]
[CanEditMultipleObjects]
public class CodexEntriesEditor : Editor
{
    public override void OnInspectorGUI()
    {
        CodexEntries codexEntries = (CodexEntries)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Copy Old Description to new format"))
        {
            codexEntries.CopyOldDescription();
        }
        
    }
}
#endif
