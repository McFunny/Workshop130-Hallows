using UnityEngine;
using UnityEditor;

public class TorchIdAssignerEditor : EditorWindow
{
    [MenuItem("Tools/Torch Utilities/Assign Torch IDs")]
    public static void AssignTorchIDs()
    {
        var torches = GameObject.FindObjectsOfType<CatacombsTorch>(true);

        // Start from 0, but skip over torches that already have valid IDs
        int nextId = 0;

        foreach (var torch in torches)
        {
            if (torch.ID >= 0)
            {
                // Already has a valid ID -> make sure we don't collide
                if (torch.ID >= nextId)
                    nextId = torch.ID + 1;
                continue;
            }

            Undo.RecordObject(torch, "Assign Torch ID");
            torch.ID = nextId;
            EditorUtility.SetDirty(torch);
            nextId++;
        }

        Debug.Log($"Torch ID assignment complete. Highest ID: {nextId - 1}");
    }
}
