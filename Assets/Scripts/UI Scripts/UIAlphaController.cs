using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAlphaController : MonoBehaviour
{
    [SerializeField] float startValue = 0f; // Initial alpha value
    [SerializeField] private float fadeSpeed = 5f; // Speed of the fade in effect
    [SerializeField] private CanvasGroup canvasGroup;
    // Start is called before the first frame update
    void Start()
    {
        if(canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if(canvasGroup == null)
        {
            Debug.LogError("CanvasGroup component is missing on " + gameObject.name);
            return;
        }

        canvasGroup.alpha = startValue; // Set the initial alpha value
    }

    private void Update()
    {
        if(PlayerMovement.isCodexOpen)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }

    // Update is called once per frame
    public void FadeIn()
    {
        canvasGroup.alpha = Mathf.Lerp(1f, 0f, Time.fixedDeltaTime * fadeSpeed);

        if (canvasGroup.alpha < 0.01f)
        {
            canvasGroup.alpha = 0f;
        }
    }

    public void FadeOut()
    {
        canvasGroup.alpha = Mathf.Lerp(0f, 1f, Time.fixedDeltaTime * fadeSpeed);
        
        if (canvasGroup.alpha > 0.99f)
        {
            canvasGroup.alpha = 1f;
        }
    }
}
