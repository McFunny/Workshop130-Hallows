using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisUI : MonoBehaviour
{
    [SerializeField] private bool forceHideUI = false;
    [SerializeField] private GameObject uiContainer;

    private DebrisPile debrisPile;

    private void Start()
    {
        debrisPile = GetComponent<DebrisPile>();
        uiContainer.SetActive(false);

        ShowDebrisUI();
    }

    private void Update()
    {
        if (forceHideUI) HideDebrisUI();
        else ShowDebrisUI();
    }

    private void ShowDebrisUI()
    {
        if (!debrisPile.repairedStruct) return;
        if(forceHideUI) return;
        uiContainer.SetActive(true);
    }

    private void HideDebrisUI()
    {
        uiContainer.SetActive(false);
    }


}
