using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using System.Linq;

public class WallMultiScatterTool : EditorWindow
{
    public enum ScatterMode { WholeMesh, SelectedProBuilderFaces }
    public enum PlacementMode { Vertices, Faces }

    [System.Serializable]
    public class PrefabEntry
    {
        public GameObject prefab;

        public int minAmount = 1;
        public int maxAmount = 5;

        public float minScale = 1f;
        public float maxScale = 1f;

        public int GetRandomAmount()
        {
            if (maxAmount < minAmount)
                maxAmount = minAmount;
            return Random.Range(minAmount, maxAmount + 1);
        }
    }

    public GameObject baseObject;
    public ScatterMode scatterMode = ScatterMode.WholeMesh;
    public PlacementMode placementMode = PlacementMode.Vertices;

    public List<PrefabEntry> prefabs = new List<PrefabEntry>();

    public float normalOffset = 0f;
    public float randomRotation = 30f;

    public bool evenDistribution = false;
    public float minSpacing = 0.5f;
    public int maxAttemptsPerObject = 25;

    private List<Vector3> placedPositions = new List<Vector3>();

    [MenuItem("Tools/Wall Multi Scatter Tool")]
    public static void Open()
    {
        GetWindow<WallMultiScatterTool>("Wall Multi Scatter Tool");
    }

    // =====================================================================
    // GUI — unchanged from your version
    // =====================================================================
    private void OnGUI()
    {
        GUILayout.Label("Scatter Objects Across Mesh", EditorStyles.boldLabel);

        scatterMode = (ScatterMode)EditorGUILayout.EnumPopup("Scatter Mode", scatterMode);

        if (scatterMode == ScatterMode.WholeMesh)
            baseObject = (GameObject)EditorGUILayout.ObjectField("Base Object", baseObject, typeof(GameObject), true);
        else
            EditorGUILayout.HelpBox("Using Selected ProBuilder Faces. BaseObject ignored.", MessageType.Info);

        placementMode = (PlacementMode)EditorGUILayout.EnumPopup("Placement Mode", placementMode);

        EditorGUILayout.Space(10);

        GUILayout.Label("Prefabs", EditorStyles.boldLabel);
        if (GUILayout.Button("Add Prefab"))
            prefabs.Add(new PrefabEntry());

        for (int i = 0; i < prefabs.Count; i++)
        {
            var p = prefabs[i];

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label($"Prefab #{i + 1}", EditorStyles.boldLabel);
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                prefabs.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break;
            }

            EditorGUILayout.EndHorizontal();

            p.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", p.prefab, typeof(GameObject), false);
            p.minAmount = EditorGUILayout.IntField("Min Amount", p.minAmount);
            p.maxAmount = EditorGUILayout.IntField("Max Amount", p.maxAmount);
            if (p.maxAmount < p.minAmount)
                p.maxAmount = p.minAmount;

            p.minScale = EditorGUILayout.FloatField("Min Scale", p.minScale);
            p.maxScale = EditorGUILayout.FloatField("Max Scale", p.maxScale);
            if (p.maxScale < p.minScale)
                p.maxScale = p.minScale;

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space(10);

        normalOffset = EditorGUILayout.FloatField("Normal Offset", normalOffset);
        randomRotation = EditorGUILayout.FloatField("Random Rotation", randomRotation);

        EditorGUILayout.Space(10);

        evenDistribution = EditorGUILayout.Toggle("Even Distribution (Faces)", evenDistribution);
        if (evenDistribution)
        {
            minSpacing = EditorGUILayout.FloatField("Min Spacing", minSpacing);
            maxAttemptsPerObject = EditorGUILayout.IntField("Max Attempts Per Object", maxAttemptsPerObject);
        }

        EditorGUILayout.Space(20);

        bool canScatter =
            scatterMode == ScatterMode.SelectedProBuilderFaces ||
            (scatterMode == ScatterMode.WholeMesh && baseObject != null);

        using (new EditorGUI.DisabledScope(!canScatter))
        {
            if (GUILayout.Button("Scatter Objects"))
                Scatter();
        }
    }

    // =====================================================================
    // DISPATCH
    // =====================================================================
    void Scatter()
    {
        placedPositions.Clear();

        if (placementMode == PlacementMode.Vertices)
            ScatterVertices();
        else
            ScatterFacesSelectedPB();
    }

    // =====================================================================
    // VERTEX SCATTER — RANDOM PREFAB SELECTION
    // =====================================================================
    void ScatterVertices()
    {
        Undo.IncrementCurrentGroup();

        // ---------------------------------------------------------
        // 1. Compute total spawn list: (PrefabEntry entry, count)
        // ---------------------------------------------------------
        List<PrefabEntry> spawnPool = new List<PrefabEntry>();

        foreach (var entry in prefabs)
        {
            if (!entry.prefab) continue;

            int spawnCount = entry.GetRandomAmount();
            for (int i = 0; i < spawnCount; i++)
                spawnPool.Add(entry);
        }

        // Shuffle so the order is random
        Shuffle(spawnPool);

        // ---------------------------------------------------------
        // WHOLE MESH
        // ---------------------------------------------------------
        if (scatterMode == ScatterMode.WholeMesh)
        {
            Mesh mesh = GetMesh(baseObject);
            if (!mesh)
            {
                Debug.LogError("No mesh on baseObject.");
                return;
            }

            Transform t = baseObject.transform;
            Vector3[] v = mesh.vertices;
            Vector3[] n = mesh.normals;

            if (n.Length != v.Length)
            {
                mesh.RecalculateNormals();
                n = mesh.normals;
            }

            List<int> vertexOrder = Enumerable.Range(0, v.Length).ToList();
            Shuffle(vertexOrder);

            int cursor = 0;

            foreach (var chosen in spawnPool)
            {
                if (cursor >= vertexOrder.Count) break;

                int idx = vertexOrder[cursor++];
                PlaceObject(chosen, v[idx], n[idx], t);
            }

            Debug.Log("Vertex scatter complete (whole mesh).");
            return;
        }

        // ---------------------------------------------------------
        // PB SELECTED VERTICES
        // ---------------------------------------------------------
        GameObject[] selected = Selection.gameObjects;
        List<(ProBuilderMesh pb, int vIndex)> verts = new();

        foreach (var go in selected)
        {
            var pb = go.GetComponent<ProBuilderMesh>();
            if (!pb) continue;

            var faces = pb.GetSelectedFaces();
            if (faces == null || faces.Length == 0) continue;

            List<int> allVerts = new();
            pb.GetCoincidentVertices(faces, allVerts);

            foreach (int vIdx in new HashSet<int>(allVerts))
                verts.Add((pb, vIdx));
        }

        if (verts.Count == 0)
        {
            Debug.LogError("No ProBuilder vertices found.");
            return;
        }

        Shuffle(verts);

        int pointer = 0;
        foreach (var chosen in spawnPool)
        {
            if (pointer >= verts.Count) break;

            var (pb, vIdx) = verts[pointer++];

            pb.ToMesh();
            pb.Refresh();

            var mf = pb.GetComponent<MeshFilter>();
            Mesh mesh = mf.sharedMesh;

            Vector3[] v = mesh.vertices;
            Vector3[] n = mesh.normals;

            PlaceObject(chosen, v[vIdx], n[vIdx], pb.transform);
        }

        Debug.Log("Vertex scatter complete (PB multi).");
    }

    // =====================================================================
    // FACE SCATTER, WHOLE MESH — RANDOM PREFAB SELECTION
    // =====================================================================
    void ScatterFacesWholeMesh()
    {
        Mesh mesh = GetMesh(baseObject);
        if (!mesh)
        {
            Debug.LogError("No mesh found.");
            return;
        }

        Transform t = baseObject.transform;
        Vector3[] v = mesh.vertices;
        Vector3[] n = mesh.normals;

        if (n.Length != v.Length)
        {
            mesh.RecalculateNormals();
            n = mesh.normals;
        }

        int[] tri = mesh.triangles;
        int triCount = tri.Length / 3;

        float[] cumulative = new float[triCount];
        float totalArea = 0f;

        for (int i = 0; i < triCount; i++)
        {
            Vector3 v0 = v[tri[i * 3]];
            Vector3 v1 = v[tri[i * 3 + 1]];
            Vector3 v2 = v[tri[i * 3 + 2]];

            float a = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
            if (a < 0.0001f) a = 0.0001f;

            totalArea += a;
            cumulative[i] = totalArea;
        }

        Undo.IncrementCurrentGroup();

        // ---------------------------------------------
        // 1. Build randomly shuffled spawn pool
        // ---------------------------------------------
        List<PrefabEntry> spawnPool = new List<PrefabEntry>();

        foreach (var entry in prefabs)
        {
            if (!entry.prefab) continue;

            int spawnCount = entry.GetRandomAmount();
            for (int i = 0; i < spawnCount; i++)
                spawnPool.Add(entry);
        }

        Shuffle(spawnPool);

        // ---------------------------------------------
        // 2. Scatter using pool order
        // ---------------------------------------------
        foreach (var chosen in spawnPool)
        {
            Vector3 p, nrm, wp;
            int attempts = 0;

            while (true)
            {
                float s = Random.value * totalArea;
                int triIndex = FindTri(cumulative, s);

                int i0 = tri[triIndex * 3];
                int i1 = tri[triIndex * 3 + 1];
                int i2 = tri[triIndex * 3 + 2];

                RandomTriangleSmooth(v[i0], v[i1], v[i2], n[i0], n[i1], n[i2], out p, out nrm);

                wp = t.TransformPoint(p);

                if (!evenDistribution)
                    break;

                if (!IsTooClose(wp) || attempts >= maxAttemptsPerObject)
                    break;

                attempts++;
            }

            if (evenDistribution && IsTooClose(wp))
                continue;

            placedPositions.Add(wp);
            PlaceObject(chosen, p, nrm, t);
        }

        Debug.Log("Face scatter complete (whole mesh).");
    }

    // =====================================================================
    // PB FACE SCATTER — RANDOM PREFAB SELECTION
    // =====================================================================
    struct PBTri
    {
        public ProBuilderMesh pb;
        public int i0, i1, i2;
        public Vector3 faceNormal;
    }

    void ScatterFacesSelectedPB()
    {
        GameObject[] selected = Selection.gameObjects;

        List<PBTri> tris = new List<PBTri>();
        List<float> cumulative = new List<float>();
        float totalArea = 0f;

        foreach (var go in selected)
        {
            var pb = go.GetComponent<ProBuilderMesh>();
            if (!pb) continue;

            var faces = pb.GetSelectedFaces();
            if (faces == null || faces.Length == 0) continue;

            var pos = pb.positions;

            foreach (var face in faces)
            {
                var idx = face.indexes;
                if (idx.Count < 3) continue;

                Vector3 fv0 = pos[idx[0]];
                Vector3 fv1 = pos[idx[1]];
                Vector3 fv2 = pos[idx[2]];
                Vector3 faceN = Vector3.Cross(fv1 - fv0, fv2 - fv0).normalized;

                int baseIdx = idx[0];
                for (int i = 1; i < idx.Count - 1; i++)
                {
                    int i0 = baseIdx;
                    int i1 = idx[i];
                    int i2 = idx[i + 1];

                    float a = Vector3.Cross(pos[i1] - pos[i0], pos[i2] - pos[i0]).magnitude * 0.5f;
                    if (a < 0.0001f) a = 0.0001f;

                    totalArea += a;
                    cumulative.Add(totalArea);

                    tris.Add(new PBTri
                    {
                        pb = pb,
                        i0 = i0,
                        i1 = i1,
                        i2 = i2,
                        faceNormal = faceN
                    });
                }
            }
        }

        if (tris.Count == 0)
        {
            Debug.LogError("No ProBuilder faces found.");
            return;
        }

        Undo.IncrementCurrentGroup();

        // ---------------------------------------------
        // 1. Build randomly shuffled spawn pool
        // ---------------------------------------------
        List<PrefabEntry> spawnPool = new List<PrefabEntry>();
        foreach (var entry in prefabs)
        {
            if (!entry.prefab) continue;

            int spawnCount = entry.GetRandomAmount();
            for (int i = 0; i < spawnCount; i++)
                spawnPool.Add(entry);
        }
        Shuffle(spawnPool);

        // ---------------------------------------------
        // 2. Scatter using pool order
        // ---------------------------------------------
        foreach (var chosen in spawnPool)
        {
            Vector3 p, nrm, wp;
            int attempts = 0;

            while (true)
            {
                float s = Random.value * totalArea;
                int index = FindTri(cumulative, s);

                var tri = tris[index];
                var pb = tri.pb;
                var pos = pb.positions;

                RandomTriangleTrueNormal(
                    pos[tri.i0],
                    pos[tri.i1],
                    pos[tri.i2],
                    tri.faceNormal,
                    out p,
                    out nrm);

                wp = pb.transform.TransformPoint(p);

                if (!evenDistribution)
                    break;

                if (!IsTooClose(wp) || attempts >= maxAttemptsPerObject)
                    break;

                attempts++;
            }

            if (evenDistribution && IsTooClose(wp))
                continue;

            placedPositions.Add(wp);
            PlaceObject(chosen, p, nrm, tris[0].pb.transform);
        }

        Debug.Log("Face scatter complete (PB multi).");
    }

    // =====================================================================
    // UTILITY
    // =====================================================================
    bool IsTooClose(Vector3 newPos)
    {
        float minSqr = minSpacing * minSpacing;

        foreach (var p in placedPositions)
            if ((p - newPos).sqrMagnitude < minSqr)
                return true;

        return false;
    }

    Mesh GetMesh(GameObject go)
    {
        if (!go) return null;

        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf && mf.sharedMesh)
            return mf.sharedMesh;

        ProBuilderMesh pb = go.GetComponent<ProBuilderMesh>();
        if (pb)
        {
            var mfpb = pb.GetComponent<MeshFilter>();
            if (mfpb && mfpb.sharedMesh)
                return mfpb.sharedMesh;
        }

        return null;
    }

    void PlaceObject(PrefabEntry entry, Vector3 localPos, Vector3 localNormal, Transform t)
    {
        Vector3 wp = t.TransformPoint(localPos);
        Vector3 wn = t.TransformDirection(localNormal).normalized;

        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(entry.prefab);
        Undo.RegisterCreatedObjectUndo(obj, "Scatter Object");

        obj.transform.position = wp + wn * normalOffset;
        obj.transform.rotation = Quaternion.FromToRotation(Vector3.up, wn);

        obj.transform.Rotate(wn, Random.Range(-randomRotation, randomRotation), Space.World);

        float mul = Random.Range(entry.minScale, entry.maxScale);

        // Multiply original prefab scale instead of overriding it
        obj.transform.localScale = Vector3.Scale(obj.transform.localScale, new Vector3(mul, mul, mul));

    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int k = Random.Range(i, list.Count);
            (list[i], list[k]) = (list[k], list[i]);
        }
    }

    int FindTri(float[] cum, float sample)
    {
        for (int i = 0; i < cum.Length; i++)
            if (sample <= cum[i])
                return i;
        return cum.Length - 1;
    }

    int FindTri(List<float> cum, float sample)
    {
        for (int i = 0; i < cum.Count; i++)
            if (sample <= cum[i])
                return i;
        return cum.Count - 1;
    }

    void RandomTriangleSmooth(Vector3 v0, Vector3 v1, Vector3 v2,
                              Vector3 n0, Vector3 n1, Vector3 n2,
                              out Vector3 p, out Vector3 n)
    {
        float r1 = Random.value;
        float r2 = Random.value;
        float s = Mathf.Sqrt(r1);

        float u = 1 - s;
        float v = s * (1 - r2);
        float w = s * r2;

        p = u * v0 + v * v1 + w * v2;
        n = (u * n0 + v * n1 + w * n2).normalized;
    }

    void RandomTriangleTrueNormal(Vector3 v0, Vector3 v1, Vector3 v2,
                                  Vector3 faceNormal,
                                  out Vector3 p, out Vector3 n)
    {
        float r1 = Random.value;
        float r2 = Random.value;
        float s = Mathf.Sqrt(r1);

        float u = 1 - s;
        float v = s * (1 - r2);
        float w = s * r2;

        p = u * v0 + v * v1 + w * v2;

        Vector3 triN = Vector3.Cross(v1 - v0, v2 - v0).normalized;
        if (Vector3.Dot(triN, faceNormal) < 0)
            triN = -triN;

        n = triN;
    }
}
