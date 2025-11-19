using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

public class VertexRetopoShrinkwrapTool : EditorWindow
{
    [Header("Base (Reference) Mesh")]
    [SerializeField] private GameObject baseObject;

    [Header("Modified (ProBuilder) Mesh")]
    [SerializeField] private ProBuilderMesh modifiedMesh;

    [Header("Settings")]
    [Tooltip("Optional max distance for snapping. 0 = no limit.")]
    [SerializeField] private float maxSnapDistance = 0f;

    [SerializeField] private bool verboseLogging = true;

    [MenuItem("Tools/Vertex Retopo (Shrinkwrap)")]
    public static void ShowWindow()
    {
        GetWindow<VertexRetopoShrinkwrapTool>("Vertex Retopo (Shrinkwrap)");
    }

    private void OnGUI()
    {
        GUILayout.Label("Vertex Retopo Shrinkwrap", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        baseObject = (GameObject)EditorGUILayout.ObjectField(
            "Base Object (Any Mesh)",
            baseObject,
            typeof(GameObject),
            true);

        modifiedMesh = (ProBuilderMesh)EditorGUILayout.ObjectField(
            "Modified (ProBuilderMesh)",
            modifiedMesh,
            typeof(ProBuilderMesh),
            true);

        EditorGUILayout.Space();
        maxSnapDistance = EditorGUILayout.FloatField("Max Snap Distance", maxSnapDistance);
        verboseLogging = EditorGUILayout.Toggle("Verbose Logging", verboseLogging);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(baseObject == null || modifiedMesh == null))
        {
            if (GUILayout.Button("Shrinkwrap To Base Surface"))
            {
                Shrinkwrap();
            }
        }

        EditorGUILayout.HelpBox(
            "Base object can be any object with a MeshFilter, SkinnedMeshRenderer, or ProBuilderMesh.\n" +
            "Modified must be a ProBuilderMesh. Each vertex is snapped to the closest point on the base mesh surface.",
            MessageType.Info);
    }

    private void Shrinkwrap()
    {
        if (baseObject == null || modifiedMesh == null)
        {
            Debug.LogError("VertexRetopoShrinkwrapTool: Base object or modified ProBuilderMesh is missing.");
            return;
        }

        Mesh baseMesh = GetMeshFromGameObject(baseObject);
        if (baseMesh == null)
        {
            Debug.LogError("VertexRetopoShrinkwrapTool: Could not find a mesh on the base object.");
            return;
        }

        Transform baseTransform = baseObject.transform;
        Transform modifiedTransform = modifiedMesh.transform;

        Vector3[] baseVerts = baseMesh.vertices;
        int[] baseTris = baseMesh.triangles;

        if (baseVerts == null || baseVerts.Length == 0 || baseTris == null || baseTris.Length == 0)
        {
            Debug.LogError("VertexRetopoShrinkwrapTool: Base mesh is missing vertices or triangles.");
            return;
        }

        // Convert base vertices to world space
        Vector3[] baseWorldVerts = new Vector3[baseVerts.Length];
        for (int i = 0; i < baseVerts.Length; i++)
        {
            baseWorldVerts[i] = baseTransform.TransformPoint(baseVerts[i]);
        }

        // Copy modified positions
        List<Vector3> modifiedLocalPositions = new List<Vector3>(modifiedMesh.positions);
        if (modifiedLocalPositions.Count == 0)
        {
            Debug.LogError("VertexRetopoShrinkwrapTool: Modified ProBuilderMesh has no vertices.");
            return;
        }

        Vector3[] modifiedWorldVerts = new Vector3[modifiedLocalPositions.Count];
        for (int i = 0; i < modifiedLocalPositions.Count; i++)
        {
            modifiedWorldVerts[i] = modifiedTransform.TransformPoint(modifiedLocalPositions[i]);
        }

        Undo.RegisterCompleteObjectUndo(modifiedMesh, "Shrinkwrap ProBuilder Vertices To Base Mesh");

        int movedCount = 0;
        float maxDistSqr = (maxSnapDistance > 0f) ? maxSnapDistance * maxSnapDistance : float.MaxValue;

        // For each modified vertex, find closest point on ANY triangle of the base mesh
        for (int v = 0; v < modifiedWorldVerts.Length; v++)
        {
            Vector3 p = modifiedWorldVerts[v];

            float bestSqrDist = float.MaxValue;
            Vector3 bestPoint = p;

            for (int t = 0; t < baseTris.Length; t += 3)
            {
                Vector3 a = baseWorldVerts[baseTris[t]];
                Vector3 b = baseWorldVerts[baseTris[t + 1]];
                Vector3 c = baseWorldVerts[baseTris[t + 2]];

                Vector3 closest = ClosestPointOnTriangle(p, a, b, c);
                float sqrDist = (p - closest).sqrMagnitude;

                if (sqrDist < bestSqrDist)
                {
                    bestSqrDist = sqrDist;
                    bestPoint = closest;
                }
            }

            if (bestSqrDist <= maxDistSqr)
            {
                modifiedWorldVerts[v] = bestPoint;
                movedCount++;
            }
        }

        // Convert back to local space & apply
        for (int i = 0; i < modifiedLocalPositions.Count; i++)
        {
            modifiedLocalPositions[i] = modifiedTransform.InverseTransformPoint(modifiedWorldVerts[i]);
        }

        modifiedMesh.positions = modifiedLocalPositions;

        // Rebuild and refresh mesh & normals
        modifiedMesh.ToMesh();
        modifiedMesh.Refresh(RefreshMask.All);

        if (verboseLogging)
        {
            Debug.Log($"VertexRetopoShrinkwrapTool: Moved {movedCount} / {modifiedLocalPositions.Count} vertices.");
        }
    }

    /// <summary>
    /// Returns closest point on a triangle ABC to point P.
    /// Standard geometric projection/clamping.
    /// </summary>
    private static Vector3 ClosestPointOnTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // From "Real-Time Collision Detection" (Christer Ericson)
        Vector3 ab = b - a;
        Vector3 ac = c - a;
        Vector3 ap = p - a;

        float d1 = Vector3.Dot(ab, ap);
        float d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) return a; // barycentric (1,0,0)

        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp);
        float d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) return b; // (0,1,0)

        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f)
        {
            float v = d1 / (d1 - d3);
            return a + v * ab; // between A and B
        }

        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp);
        float d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) return c; // (0,0,1)

        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f)
        {
            float w = d2 / (d2 - d6);
            return a + w * ac; // between A and C
        }

        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f)
        {
            float w = (d4 - d3) / ((d4 - d3) + (d5 - d6));
            return b + w * (c - b); // between B and C
        }

        // Inside face region
        float denom = 1f / (va + vb + vc);
        float v2 = vb * denom;
        float w2 = vc * denom;
        return a + ab * v2 + ac * w2;
    }

    /// <summary>
    /// Get mesh from MeshFilter, SkinnedMeshRenderer, or ProBuilderMesh.
    /// </summary>
    private Mesh GetMeshFromGameObject(GameObject go)
    {
        if (go == null) return null;

        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
            return mf.sharedMesh;

        SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMesh != null)
            return smr.sharedMesh;

        ProBuilderMesh pb = go.GetComponent<ProBuilderMesh>();
        if (pb != null)
        {
            MeshFilter pbFilter = pb.GetComponent<MeshFilter>();
            if (pbFilter != null && pbFilter.sharedMesh != null)
                return pbFilter.sharedMesh;
        }

        return null;
    }
}
