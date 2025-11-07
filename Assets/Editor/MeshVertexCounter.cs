using UnityEngine;
using UnityEditor;

public class MeshVertexCounter
{
    [MenuItem("Tools/Mesh/Count Vertices")]
    public static void CountVertices()
    {
        var obj = Selection.activeGameObject;
        if (obj == null)
        {
            Debug.Log("Select a model or prefab in the Hierarchy first.");
            return;
        }

        MeshFilter mf = obj.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            Debug.Log($"{obj.name} has {mf.sharedMesh.vertexCount} vertices.");
        }
        else
        {
            SkinnedMeshRenderer smr = obj.GetComponent<SkinnedMeshRenderer>();
            if (smr != null && smr.sharedMesh != null)
                Debug.Log($"{obj.name} has {smr.sharedMesh.vertexCount} vertices (skinned mesh).");
            else
                Debug.Log("No mesh found on selected object.");
        }
    }
}
