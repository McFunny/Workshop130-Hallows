using UnityEngine;
using UnityEditor;

public class MultiAxisRotationTool : EditorWindow
{
    private bool rotateX = false;
    private bool rotateY = true;
    private bool rotateZ = false;

    private float minRotation = -180f;
    private float maxRotation = 180f;

    [MenuItem("Tools/Multi-Axis Rotation Tool")]
    public static void ShowWindow()
    {
        GetWindow<MultiAxisRotationTool>("Multi-Axis Rotation Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Multi-Axis Rotation Offset", EditorStyles.boldLabel);

        GUILayout.Label("Select Axes to Rotate:", EditorStyles.label);
        rotateX = EditorGUILayout.ToggleLeft("X Axis", rotateX);
        rotateY = EditorGUILayout.ToggleLeft("Y Axis", rotateY);
        rotateZ = EditorGUILayout.ToggleLeft("Z Axis", rotateZ);

        GUILayout.Space(5);

        minRotation = EditorGUILayout.FloatField("Min Rotation (°)", minRotation);
        maxRotation = EditorGUILayout.FloatField("Max Rotation (°)", maxRotation);

        if (maxRotation < minRotation)
            maxRotation = minRotation;

        GUILayout.Space(10);

        if (GUILayout.Button("Apply Random Rotation"))
        {
            ApplyRotation();
        }
    }

    private void ApplyRotation()
    {
        Transform[] selectedObjects = Selection.transforms;

        if (selectedObjects.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more GameObjects.", "OK");
            return;
        }

        if (!rotateX && !rotateY && !rotateZ)
        {
            EditorUtility.DisplayDialog("No Axis Selected", "Please enable at least one axis to rotate.", "OK");
            return;
        }

        Undo.RecordObjects(selectedObjects, "Apply Multi-Axis Rotation");

        foreach (Transform obj in selectedObjects)
        {
            Vector3 euler = obj.eulerAngles;

            if (rotateX)
                euler.x += Random.Range(minRotation, maxRotation);
            if (rotateY)
                euler.y += Random.Range(minRotation, maxRotation);
            if (rotateZ)
                euler.z += Random.Range(minRotation, maxRotation);

            obj.eulerAngles = euler;
        }

        //EditorUtility.DisplayDialog("Rotation Applied", "Applied random rotations to selected objects.", "OK");
    }
}
