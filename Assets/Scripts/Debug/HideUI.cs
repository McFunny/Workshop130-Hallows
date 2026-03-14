using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUI : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup, tooltipCanvasGroup;
    private CanvasGroup versionCanvas;
    [SerializeField] KeyCode mainKey, tooltipKey, versionKey;
    public static bool hideUI = false;
    public System.Action onUIHidden, onUIShown;

    void Start()
    {
        GameObject versionObject = GameObject.Find("BuildCanvas");

        if(PlayerPrefs.GetInt("HideUI", 0) == 1)
        {
            if(canvasGroup != null) canvasGroup.alpha = 0;
        }
        else
        {
            if(canvasGroup != null) canvasGroup.alpha = 1;
        }

        if(PlayerPrefs.GetInt("HideTooltips", 0) == 1)
        {
            if(tooltipCanvasGroup != null) tooltipCanvasGroup.alpha = 0;
        }
        else
        {
            if(tooltipCanvasGroup != null) tooltipCanvasGroup.alpha = 1;
        }

        if(versionObject == null) return;
        versionCanvas = versionObject.GetComponent<CanvasGroup>();
        
        if(PlayerPrefs.GetInt("HideVersion", 0) == 1)
        {
            if(versionCanvas != null) versionCanvas.alpha = 0;
        }
        else
        {
            if(versionCanvas != null) versionCanvas.alpha = 1;
        }

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
                PlayerPrefs.SetInt("HideUI", 1);
            }
            else
            {
                onUIShown?.Invoke(); // Invoke the action when UI is shown
                canvasGroup.alpha = 1;
                PlayerPrefs.SetInt("HideUI", 0);
            }
            PlayerPrefs.Save();
        }

        if (tooltipKey != KeyCode.None && Input.GetKeyDown(tooltipKey) && tooltipCanvasGroup != null)
        {
            if (tooltipCanvasGroup.alpha == 1)
            {
                tooltipCanvasGroup.alpha = 0;
                PlayerPrefs.SetInt("HideTooltips", 1);
            }
            
            else 
            {
                tooltipCanvasGroup.alpha = 1;
                PlayerPrefs.SetInt("HideTooltips", 0);
            }
        }

        if (versionKey != KeyCode.None && Input.GetKeyDown(versionKey) && versionCanvas != null)
        {
            if (versionCanvas.alpha == 1)
            {
                if(versionCanvas == null) return;
                versionCanvas.alpha = 0;
                PlayerPrefs.SetInt("HideVersion", 1);

            }
            
            else 
            {
                if(versionCanvas == null) return;
                versionCanvas.alpha = 1;
                PlayerPrefs.SetInt("HideVersion", 0);
            }
        }

        if(canvasGroup.alpha == 0) hideUI = true;
        else hideUI = false;
        
    }
}
