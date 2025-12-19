using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup, tooltipCanvasGroup;
    private CanvasGroup versionCanvas;
    [SerializeField] KeyCode mainKey, tooltipKey;
    public static bool hideUI = false;
    public System.Action onUIHidden, onUIShown;

    void Awake()
    {
        versionCanvas = GameObject.Find("BuildCanvas").GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mainKey != KeyCode.None && Input.GetKeyDown(mainKey) && canvasGroup != null)
        {
            if (canvasGroup.alpha == 1)
            {
                onUIHidden?.Invoke(); // Invoke the action when UI is hidden
                canvasGroup.alpha = 0;
            }
            else
            {
                onUIShown?.Invoke(); // Invoke the action when UI is shown
                canvasGroup.alpha = 1;
            }    
        }

        if (tooltipKey != KeyCode.None && Input.GetKeyDown(tooltipKey) && tooltipCanvasGroup != null)
        {
            if (tooltipCanvasGroup.alpha == 1)
            {
                if(versionCanvas == null) versionCanvas = GameObject.Find("BuildCanvas").GetComponent<CanvasGroup>();
                tooltipCanvasGroup.alpha = 0;
                versionCanvas.alpha = 0;
            }
            
            else 
            {
                if(versionCanvas == null) versionCanvas = GameObject.Find("BuildCanvas").GetComponent<CanvasGroup>();
                tooltipCanvasGroup.alpha = 1;
                versionCanvas.alpha = 1;
            }
        }

        if(canvasGroup.alpha == 0) hideUI = true;
        else hideUI = false;
        
    }
}
