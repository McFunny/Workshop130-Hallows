using UnityEngine;
using UnityEditor;

public class AlignAxisToGroundTool : EditorWindow
{
    public enum AxisChoice
    {
        X_Axis_Red,
        Y_Axis_Green,
        Z_Axis_Blue
    }

    public AxisChoice axisToPointDown = AxisChoice.Y_Axis_Green;
    public float raycastDistance = 10f;
    public LayerMask groundLayer = ~0; // everything

    [MenuItem("Tools/Align Axis To Ground")]
    public static void Open()
    {
        GetWindow<AlignAxisToGroundTool>("Align Axis To Ground");
    }

    private void OnGUI()
    {
        GUILayout.Label("Align Chosen Axis Toward Ground", EditorStyles.boldLabel);
        axisToPointDown = (AxisChoice)EditorGUILayout.EnumPopup("Axis To Aim Down", axisToPointDown);

        raycastDistance = EditorGUILayout.FloatField("Raycast Distance", raycastDistance);
        groundLayer = EditorGUILayout.LayerField("Ground Layer", groundLayer);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Align Selected"))
            AlignSelected();
    }

    private void AlignSelected()
    {
        var objects = Selection.gameObjects;
        if (objects.Length == 0)
        {
            Debug.LogWarning("Select at least one object.");
            return;
        }

        Undo.IncrementCurrentGroup();

        foreach (GameObject obj in objects)
        {
            AlignObjectAxisToGround(obj);
        }

        Debug.Log("Alignment complete.");
    }

    private void AlignObjectAxisToGround(GameObject obj)
    {
        Vector3 origin = obj.transform.position + Vector3.up * 0.1f; // avoid self-collisions
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, groundLayer))
        {
            Undo.RecordObject(obj.transform, "Align Axis To Ground");

            Vector3 targetNormal = hit.normal;

            // Determine local axis
            Vector3 localAxis = Vector3.zero;
            switch (axisToPointDown)
            {
                case AxisChoice.X_Axis_Red: localAxis = obj.transform.right; break;
                case AxisChoice.Y_Axis_Green: localAxis = obj.transform.up; break;
                case AxisChoice.Z_Axis_Blue: localAxis = obj.transform.forward; break;
            }

            // Rotate so that axis points opposite the normal (DOWN)
            Quaternion targetRotation =
                Quaternion.FromToRotation(localAxis, -targetNormal) * obj.transform.rotation;

            obj.transform.rotation = targetRotation;

            // Snap position onto surface
            obj.transform.position = hit.point;
        }
        else
        {
            Debug.LogWarning($"{obj.name} did not hit ground.");
        }
    }
}
