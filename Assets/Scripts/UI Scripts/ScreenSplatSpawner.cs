using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenSplatSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform canvasRect;
    [SerializeField] ScreenSplat splatPrefab;

    [Header("Spawn Settings")]
    [SerializeField] Vector2 sizeRange = new Vector2(80, 180);
    //[SerializeField] int splatsPerHit = 2;

    [Header("Edge Spawn")]
    [SerializeField] float edgeThickness = 120f;   // distance from edge inward
    [SerializeField] float inwardBias = 40f;       // how far splats can drift inward

    [SerializeField] Sprite bloodSprite;
    [SerializeField] Sprite waterSprite;
    [SerializeField] Sprite dirtSprite;

    public static ScreenSplatSpawner Instance;

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
        for (int i = 0; i < amount; i++)
        {
            ScreenSplat splat = Instantiate(splatPrefab, canvasRect);

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
        }
    }

    Vector2 RandomScreenPosition()
    {
        Rect r = canvasRect.rect;

        float x = 0f;
        float y = 0f;

        int edge = Random.Range(0, 4); // 0=Top, 1=Bottom, 2=Left, 3=Right

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
                y = Random.Range(0, r.height);
                break;

            case 3: // Right
                x = r.width - Random.Range(0, edgeThickness);
                y = Random.Range(0, r.height);
                break;
        }

        // Optional inward drift so they don’t hug the edge too tightly
        x = Mathf.Clamp(x + Random.Range(-inwardBias, inwardBias), 0, r.width);
        y = Mathf.Clamp(y + Random.Range(-inwardBias, inwardBias), 0, r.height);

        return new Vector2(x, y);
        }
}

public enum SplatType
{
    Blood,
    Water,
    Dirt
}
