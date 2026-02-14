using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class AnimatedText : MonoBehaviour
{
    public bool enableAnimation = true;
    public float amplitude = 0.2f;
    public float frequency = 1.5f;
    public float randomness = 0.4f;
    public float driftSpeed = 0.1f;

    TMP_Text text;
    float[] seeds;
    int lastCharCount = -1;

    void Awake()
    {
        text = GetComponent<TMP_Text>();
        RegenerateSeeds();
    }

    void OnEnable()
    {
        enableAnimation = PlayerPrefs.GetInt("DialogueAnimation", 1) == 1;
        Debug.Log("Dialogue Animation Enabled: " + enableAnimation);
        if(enableAnimation == false) return;
        RegenerateSeeds();
    }

    void Update()
    {
        text.ForceMeshUpdate();
        if(enableAnimation == false) return;

        // Check if character count changed (text updated)
        int currentCount = text.textInfo.characterCount;

        if (currentCount != lastCharCount)
        {
            RegenerateSeeds();
            lastCharCount = currentCount;
        }

        var mesh = text.mesh;
        var vertices = mesh.vertices;
        float time = Time.time;

        for (int i = 0; i < currentCount; i++)
        {
            var charInfo = text.textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int v = charInfo.vertexIndex;

            float seed = seeds[i];

            float wave =
                Mathf.Sin(time * frequency + seed) * amplitude * 0.6f +
                Mathf.Cos(time * driftSpeed + seed * 2f) * amplitude * 0.4f;

            float x = wave * (0.3f + randomness * (Mathf.PerlinNoise(seed, time) - 0.5f));
            float y = wave * (1.0f + randomness * (Mathf.PerlinNoise(seed + 50, time) - 0.5f));

            Vector3 offset = new Vector3(x, y, 0);

            for (int j = 0; j < 4; j++)
                vertices[v + j] += offset;
        }

        mesh.vertices = vertices;
        text.canvasRenderer.SetMesh(mesh);
    }

    void RegenerateSeeds()
    {
        text.ForceMeshUpdate();
        int count = text.textInfo.characterCount;

        seeds = new float[count];
        for (int i = 0; i < count; i++)
            seeds[i] = Random.Range(0f, 100f);

        lastCharCount = count;
    }
}