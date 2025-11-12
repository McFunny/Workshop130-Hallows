using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingTerrain : MonoBehaviour
{
    [Header("Terrain Scrolling Settings")]
    public Terrain terrain;          // Assign your Terrain object
    public float scrollSpeed = 0.05f; // How fast the texture scrolls
    public bool scrollInLocalSpace = true; // Whether to respect parent movement

    private Vector2 offset = Vector2.zero;
    private Vector3 lastPosition;

    public bool scrollTerrain = true;

    void Start()
    {
        if (terrain == null)
            terrain = GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogWarning("TerrainTextureScroller: No terrain assigned.");
            enabled = false;
            return;
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        if (terrain == null || !scrollTerrain) return;

        // Compute delta movement (in local or world space)
        Vector3 delta = scrollInLocalSpace
            ? transform.localPosition - transform.parent.localPosition
            : transform.position - lastPosition;

        // Scroll texture opposite to movement direction (to appear like ground moving)
        offset.x += scrollSpeed * Time.deltaTime;

        // Apply offset to all terrain layers
        var layers = terrain.terrainData.terrainLayers;
        for (int i = 0; i < layers.Length; i++)
        {
            Vector2 currentOffset = layers[i].tileOffset;
            currentOffset.x = offset.x;
            layers[i].tileOffset = currentOffset;
        }

        terrain.terrainData.terrainLayers = layers;

        lastPosition = transform.position;
    }
}
