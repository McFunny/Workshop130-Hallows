using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuitHandler : MonoBehaviour
{
    private OpenWebsite openWebsite;
    void Awake()
    {
        openWebsite = FindObjectOfType<OpenWebsite>();
        if (openWebsite == null)
        {
            Debug.LogWarning("No OpenWebsite script found in the scene.");
        }
    }
    
    private void OnApplicationQuit()
    {
        Application.OpenURL(openWebsite.feedbackLink);
    }
}
