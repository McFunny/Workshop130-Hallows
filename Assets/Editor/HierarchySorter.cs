using UnityEngine;
using UnityEditor;
using System.Linq;

public class HierarchySorter : EditorWindow
{
    [MenuItem("Tools/Sort Selected Objects By Name")]
    static void SortSelectedObjects()
    {
        var selected = Selection.gameObjects;

        if (selected.Length <= 1)
        {
            Debug.LogWarning("Select 2 or more GameObjects to sort.");
            return;
        }

        // Sort alphabetically (A → Z)
        var sorted = selected.OrderBy(g => g.name).ToArray();

        // All objects must share the same parent
        Transform parent = sorted[0].transform.parent;
        foreach (var go in sorted)
        {
            if (go.transform.parent != parent)
            {
                Debug.LogError("All selected objects must share the same parent to sort correctly.");
                return;
            }
        }

        Undo.RegisterFullObjectHierarchyUndo(parent, "Sort Hierarchy");

        // Apply sibling indices in sorted order
        for (int i = 0; i < sorted.Length; i++)
            sorted[i].transform.SetSiblingIndex(i);

        Debug.Log("Sorted selected objects by name.");
    }
}
