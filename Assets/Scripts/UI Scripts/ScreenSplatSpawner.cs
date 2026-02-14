using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSplatSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform canvasRect;
    //[SerializeField] ScreenSplat splatPrefab;

    [Header("Spawn Settings")]
    [SerializeField] Vector2 sizeRange = new Vector2(80, 180);
    //[SerializeField] int splatsPerHit = 2;

    [Header("Edge Spawn")]
    [SerializeField] float edgeThickness = 120f;   // distance from edge inward
    [SerializeField] float inwardBias = 40f;       // how far splats can drift inward

    [Header("Vertical Bias")]
    [SerializeField, Range(0f, 1f)]
    float lowerScreenBias = 0.75f; // 0.5 = neutral, 1 = very bottom-heavy

    [SerializeField] Sprite bloodSprite;
    [SerializeField] Sprite waterSprite;
    [SerializeField] Sprite dirtSprite;

    public static ScreenSplatSpawner Instance;

    public ScreenSplatPool splatPool;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    public void SpawnSplats(SplatType type, Color color, float amount)
    {
        StartCoroutine(SpawnSplatsRoutine(type, color, amount));
    }

    IEnumerator SpawnSplatsRoutine(SplatType type, Color color, float amount)
    {
        for (int i = 0; i < amount; i++)
        {
            ScreenSplat splat = splatPool.Get();

            RectTransform rt = splat.GetComponent<RectTransform>();
            Image img = splat.GetComponent<Image>();

            switch(type)
            {
                case SplatType.Blood:
                img.sprite = bloodSprite;
                break;
                case SplatType.Water:
                img.sprite = waterSprite;
                break;
                case SplatType.Dirt:
                img.sprite = dirtSprite;
                break;
            }
            img.color = color;

            rt.anchoredPosition = RandomScreenPosition();
            rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));

            float size = Random.Range(sizeRange.x, sizeRange.y);
            rt.sizeDelta = Vector2.one * size;

            yield return new WaitForSeconds(Random.Range(0.01f, 0.1f));
        }
    }

    Vector2 RandomScreenPosition()
    {
        Rect r = canvasRect.rect;

        float x = 0f;
        float y = 0f;

        int edge = Random.Range(0, 4);

        switch (edge)
        {
            case 0: // Top
                x = Random.Range(0, r.width);
                y = r.height - Random.Range(0, edgeThickness);
                break;

            case 1: // Bottom
                x = Random.Range(0, r.width);
                y = Random.Range(0, edgeThickness);
                break;

            case 2: // Left
                x = Random.Range(0, edgeThickness);
                y = BiasedHeight(r);
                break;

            case 3: // Right
                x = r.width - Random.Range(0, edgeThickness);
                y = BiasedHeight(r);
                break;
        }

        x = Mathf.Clamp(x + Random.Range(-inwardBias, inwardBias), 0, r.width);
        y = Mathf.Clamp(y + Random.Range(-inwardBias, inwardBias), 0, r.height);

        return new Vector2(x, y);
    }

    float BiasedHeight(Rect r)
    {
        float t = Random.value;

        // Bias toward bottom
        t = Mathf.Pow(t, Mathf.Lerp(1f, 3f, lowerScreenBias));

        return t * r.height;
    }
}

public enum SplatType
{
    Blood,
    Water,
    Dirt
}
