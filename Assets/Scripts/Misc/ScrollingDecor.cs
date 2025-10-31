using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingDecor : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float scrollSpeed = 5f;     // Speed at which the segment scrolls
    public float resetX = -50f;        // Local X position where it resets
    public float startX = 50f;         // Local X position where it reappears

    void Update()
    {
        // Move environment left (negative X) relative to its parent
        transform.localPosition += Vector3.left * scrollSpeed * Time.deltaTime;

        // When segment goes past the reset point, wrap it back
        if (transform.localPosition.x <= resetX)
        {
            Vector3 pos = transform.localPosition;
            pos.x = startX;
            transform.localPosition = pos;
        }
    }
}
