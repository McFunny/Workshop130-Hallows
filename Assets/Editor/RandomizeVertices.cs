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

        if (GUILayout.Button("Randomize Selected Objects"))
        {
            var selection = Selection.gameObjects;

            if (selection.Length == 0)
            {
                Debug.LogError("No objects selected!");
                return;
            }

            foreach (var go in selection)
            {
                var mesh = go.GetComponent<ProBuilderMesh>();
                if (mesh == null)
                {
                    Debug.LogWarning($"{go.name} is not a ProBuilder mesh, skipping.");
                    continue;
                }

                RandomizeSharedVertices(mesh, randomOffset);
            }
        }
    }

    private void RandomizeSharedVertices(ProBuilderMesh mesh, float offset)
    {
        Undo.RegisterCompleteObjectUndo(mesh, "Randomize Vertices");

        List<Vector3> positions = new List<Vector3>(mesh.positions);
        var sharedVertices = mesh.sharedVertices;

        foreach (var sharedGroup in sharedVertices)
        {
            Vector3 randomOffsetVec = new Vector3(
                Random.Range(-offset, offset),
                Random.Range(-offset, offset),
                Random.Range(-offset, offset)
            );

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
