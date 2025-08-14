using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshCombiner : MonoBehaviour
{
    // Source Meshes you want to combine
    public List<MeshFilter> listMeshFilter;

    // Make a new mesh to be the target of the combine operation
    public MeshFilter TargetMesh;

    [ContextMenu("Combine Meshes")]
    public void CombineMesh()
    {
        //Make an array of CombineInstance.
        var combine = new CombineInstance[listMeshFilter.Count];

        //Set Mesh And their Transform to the CombineInstance
        for (int i = 0; i < listMeshFilter.Count; i++)
        {
            combine[i].mesh = listMeshFilter[i].sharedMesh;
            combine[i].transform = listMeshFilter[i].transform.localToWorldMatrix;
        }

        // Create a Empty Mesh
        var mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Use 32-bit indices for larger meshes

        //Call targetMesh.CombineMeshes and pass in the array of CombineInstances.
        mesh.CombineMeshes(combine);

        //Assign the target mesh to the mesh filter of the combination game object.
        TargetMesh.mesh = mesh;

        // Save The Mesh To Location
        SaveMesh(TargetMesh.sharedMesh, gameObject.name, false, true);

        // Print Results
        print($"<color=#20E7B0>Combine Meshes was Successful!</color>");
    }


    public static void SaveMesh(Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
    {
        string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
        if (string.IsNullOrEmpty(path)) return;

        path = FileUtil.GetProjectRelativePath(path);

        Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;

        if (optimizeMesh)
            MeshUtility.Optimize(meshToSave);

        AssetDatabase.CreateAsset(meshToSave, path);
        AssetDatabase.SaveAssets();
    }
}

#if UNITY_EDITOR
// Editor buttons :)
[CustomEditor(typeof(MeshCombiner), true)]
public class MeshCombinerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshCombiner meshCombiner = (MeshCombiner)target;
        if(meshCombiner.TargetMesh == null)
        {
            meshCombiner.TargetMesh = meshCombiner.GetComponent<MeshFilter>();
        }
        DrawDefaultInspector();

        if (GUILayout.Button("Add Children to Mesh Filter List"))
        {
            meshCombiner.listMeshFilter.Clear();
            MeshFilter[] meshFilters = meshCombiner.GetComponentsInChildren<MeshFilter>();
            foreach (var mf in meshFilters)
            {
                if (mf != null && !meshCombiner.listMeshFilter.Contains(mf))
                {
                    meshCombiner.listMeshFilter.Add(mf);
                }
            }
            Debug.Log("Added all child MeshFilters to the list.");
        }

        if (GUILayout.Button("Combine Meshes"))
        {
            meshCombiner.CombineMesh();
        }

        if (GUILayout.Button("Enable Mesh Filter"))
        {
            foreach (var mf in meshCombiner.listMeshFilter)
            {
                if (mf != null && mf.GetComponent<MeshRenderer>() != null)
                {
                    mf.GetComponent<MeshRenderer>().enabled = true;
                }
            }
            Debug.Log("Enabled all Mesh Filters in the list.");
        }
        
        if (GUILayout.Button("Disable Mesh Filter"))
        {
            foreach (var mf in meshCombiner.listMeshFilter)
            {
                if (mf != null && mf.GetComponent<MeshRenderer>() != null)
                {
                    mf.GetComponent<MeshRenderer>().enabled = false;
                }
            }
            Debug.Log("Disabled all Mesh Filters in the list.");
        }
    }
}
#endif
