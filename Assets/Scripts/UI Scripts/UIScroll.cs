using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIScroll : MonoBehaviour
{
    [SerializeField] ScrollRect scrollRect;
    private ControlManager controlManager;
    // Start is called before the first frame update
    void Start()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
    }

    void OnEnable()
    {
        controlManager.uiScroll.action.started += HandleScroll;
    }

    void OnDisable()
    {
        controlManager.uiScroll.action.started -= HandleScroll;
    }

    private void HandleScroll(InputAction.CallbackContext context)
    {
        if(ControlManager.isGamepad) return;
        float scrollValue = context.ReadValue<float>();
        Debug.Log("Scroll Value:" + scrollValue);

        if(scrollValue > 0) scrollValue = 1;
        else if (scrollValue < 0) scrollValue = -1;
        else scrollValue = 0;

        scrollRect.verticalScrollbar.value += scrollValue * scrollRect.scrollSensitivity;
    }
}
