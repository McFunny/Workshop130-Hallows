using UnityEngine;
using UnityEditor;
using UnityEngine.ProBuilder;
using System.Collections.Generic;

public class RandomizeVertices : EditorWindow
{
    private float randomOffset = 0.1f;

    [MenuItem("Tools/ProBuilder/Randomize Vertices")]
    public static void ShowWindow()
    {
        GetWindow<RandomizeVertices>("Randomize Vertices");
    }

    private void OnGUI()
    {
        GUILayout.Label("Randomize ProBuilder Vertices", EditorStyles.boldLabel);
        randomOffset = EditorGUILayout.FloatField("Random Offset", randomOffset);

        if (GUILayout.Button("Randomize Selected Object"))
        {
            if (Selection.activeGameObject == null)
            {
                Debug.LogError("No object selected!");
                return;
            }

            var mesh = Selection.activeGameObject.GetComponent<ProBuilderMesh>();
            if (mesh == null)
            {
                Debug.LogError("Selected object is not a ProBuilder mesh!");
                return;
            }

            RandomizeSharedVertices(mesh, randomOffset);
        }
    }

    private void RandomizeSharedVertices(ProBuilderMesh mesh, float offset)
    {
        Undo.RegisterCompleteObjectUndo(mesh, "Randomize Vertices");

        // Copy positions to a mutable list
        List<Vector3> positions = new List<Vector3>(mesh.positions);

        // Get shared vertex groups (each represents a connected vertex cluster)
        var sharedVertices = mesh.sharedVertices;

        foreach (var sharedGroup in sharedVertices)
        {
            // Generate one random offset per shared vertex cluster
            Vector3 randomOffsetVec = new Vector3(
                Random.Range(-offset, offset),
                Random.Range(-offset, offset),
                Random.Range(-offset, offset)
            );

            // Apply that offset to all vertex indices in this group
            foreach (int index in sharedGroup)
            {
                positions[index] += randomOffsetVec;
            }
        }

        mesh.positions = positions;
        mesh.ToMesh();
        mesh.Refresh();

        EditorUtility.SetDirty(mesh);
        Debug.Log($"Randomized shared vertices on {mesh.name} with offset ±{offset}.");
    }
}
