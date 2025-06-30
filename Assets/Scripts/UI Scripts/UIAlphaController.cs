using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAlphaController : MonoBehaviour
{
    [SerializeField] float startValue = 0f; // Initial alpha value
    [SerializeField] private float fadeInSpeed = 5f; // Speed of the fade in effect
    [SerializeField] private float fadeOutSpeed = 2f; // Speed of the fade out effect
    [SerializeField] private CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component is missing on " + gameObject.name);
            return;
        }

        canvasGroup.alpha = startValue; // Set the initial alpha value
    }

    private void Update()
    {
        if (PlayerMovement.isCodexOpen)
        {
            if (canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += fadeInSpeed * Time.fixedDeltaTime;
            }
        }
        else
        {
            if (canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= fadeOutSpeed * Time.fixedDeltaTime;
            }
        }   
    }
}
