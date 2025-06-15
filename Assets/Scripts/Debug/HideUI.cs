using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] KeyCode keyToPress;
    // Start is called before the first frame update
    void Start()
    {
        if (canvasGroup == null)
        {
            Debug.LogWarning("No canvasgroup found at " + this.gameObject + ". Hiding this UI element will not work.", this);
            this.enabled = false;
        }

        if (keyToPress == KeyCode.None)
        {
            Debug.LogWarning("No keycode is set at " + this.gameObject + ". Disabling component.", this);
            this.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            if (canvasGroup.alpha == 1) canvasGroup.alpha = 0;
            else canvasGroup.alpha = 1;
        }
    }
}
