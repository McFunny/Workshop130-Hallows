using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebrisUI : MonoBehaviour
{
    public bool forceHideUI = false;
    [SerializeField] private Sprite mintSprite;
    [SerializeField] private GameObject uiContainer;
    [SerializeField] private Image[] resourceIcons;
    [SerializeField] private TextMeshProUGUI[] resourceText;
    [SerializeField] private GameObject imageContainer, repairText;

    private DebrisPile debrisPile;

    private void Start()
    {
        debrisPile = GetComponent<DebrisPile>();
        uiContainer.SetActive(false);

        for (int i = 0; i < resourceIcons.Length; i++)
        {
            resourceIcons[i].gameObject.SetActive(false);
        }

        ShowDebrisUI();
    }

    private void Update()
    {

    }

    private void ShowDebrisUI()
    {
        if (!debrisPile.repairedStruct) return;
        if (forceHideUI) return;
        uiContainer.SetActive(true);
        var repairItems = debrisPile.repairedStruct.repairItems;

        for (int i = 0; i <= repairItems.Count; i++)
        {
            if (i < repairItems.Count)
            {
                resourceIcons[i].gameObject.SetActive(true);
                resourceIcons[i].sprite = repairItems[i].item.icon;
                resourceText[i].text = repairItems[i].amount.ToString();
            }
            else
            {
                if (debrisPile.repairedStruct.mintRepairCost <= 0) continue;
                //Show mint cost
                resourceIcons[i].gameObject.SetActive(true);
                resourceIcons[i].sprite = mintSprite;
                resourceText[i].text = debrisPile.repairedStruct.mintRepairCost.ToString();
            }

        }
    }

    private void HideDebrisUI()
    {
        uiContainer.SetActive(false);
    }

    public void ShowRepairUI()
    {
        if (forceHideUI) return;
        imageContainer.SetActive(false);
        repairText.SetActive(true);
    }


}
