using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigameFunctionality : MonoBehaviour
{
    [Header("Percent of bar this segment occupies")]
    public float size;
    private RectTransform maskTransform;

    [Header("Actual variables that matter")]
    public int hitCount = 1; // Number of "hits" this segment counts as
    public bool isHit; // Determines if hitting this segment counts as a hit or a miss

    public virtual void Start()
    {
        // Initialize size based on the local scale of the GameObject
        maskTransform = transform.parent.GetComponent<RectTransform>();
        maskTransform.sizeDelta = new Vector2(519.75f, maskTransform.sizeDelta.y); //519.75 is the max width. This resets the size to things stay consistent

        maskTransform.sizeDelta = new Vector2(size * maskTransform.sizeDelta.x, maskTransform.sizeDelta.y);
    }

    public virtual void MinigameFunction()
    {
        // This function is meant to be overridden by subclasses
        Debug.Log("Minigame: Miss!!!");
    }

    public void SetSize()
    {
        Start();
    }
    
}
