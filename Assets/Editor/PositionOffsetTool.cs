using UnityEngine;
using UnityEditor;

public class MultiAxisOffsetTool : EditorWindow
{
    private bool offsetX = false;
    private bool offsetY = true;
    private bool offsetZ = false;

    private float minOffset = -1f;
    private float maxOffset = 1f;

    [MenuItem("Tools/Multi-Axis Offset Tool")]
    public static void ShowWindow()
    {
        GetWindow<MultiAxisOffsetTool>("Multi-Axis Offset Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Multi-Axis Position Offset", EditorStyles.boldLabel);

        GUILayout.Label("Select Axes to Offset:", EditorStyles.label);
        offsetX = EditorGUILayout.ToggleLeft("X Axis", offsetX);
        offsetY = EditorGUILayout.ToggleLeft("Y Axis", offsetY);
        offsetZ = EditorGUILayout.ToggleLeft("Z Axis", offsetZ);

        GUILayout.Space(5);

        minOffset = EditorGUILayout.FloatField("Min Offset", minOffset);
        maxOffset = EditorGUILayout.FloatField("Max Offset", maxOffset);

        if (maxOffset < minOffset)
            maxOffset = minOffset;

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Random Offset"))
        {
            ApplyOffset();
        }
    }

    private void ApplyOffset()
    {
        Transform[] selectedObjects = Selection.transforms;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
            return;
        }

        if (!offsetX && !offsetY && !offsetZ)
        {
            EditorUtility.DisplayDialog("No Axis Selected", "Please enable at least one axis to offset.", "OK");
            return;
        }

        Undo.RecordObjects(selectedObjects, "Apply Multi-Axis Offset");

        foreach (Transform obj in selectedObjects)
        {
            Vector3 pos = obj.position;

            if (offsetX)
                pos.x += Random.Range(minOffset, maxOffset);
            if (offsetY)
                pos.y += Random.Range(minOffset, maxOffset);
            if (offsetZ)
                pos.z += Random.Range(minOffset, maxOffset);

            obj.position = pos;
        }

        //EditorUtility.DisplayDialog("Offset Applied", "Applied random offsets to selected objects.", "OK");
    }
}
