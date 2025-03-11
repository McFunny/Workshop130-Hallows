using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class OpenWebsite : MonoBehaviour
{
    public bool canOpen;
    public InputActionReference openInput, moreDetails;
    public string feedbackLink;

    void Update()
    {
        if(canOpen && ControlManager.isController && (openInput.action.WasPressedThisFrame() || moreDetails.action.WasPressedThisFrame()))
        {
            OpenFeedbackForm();
        }
    }
    public void OpenFeedbackForm()
    {
        Application.OpenURL(feedbackLink);
    }
}
