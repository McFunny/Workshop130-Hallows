using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StructurePage : CodexPage
{
    [SerializeField] private TextMeshProUGUI description;
    [SerializeField] private Sprite mintSprite;
    [SerializeField] private List<GameObject> repairObjects;
    [SerializeField] private List<Image> resourceIcons;
    [SerializeField] private List<TextMeshProUGUI> resourceText;

    public override void UpdatePage(CodexEntries entry, Quest quest)
    {
        GetReferences();

        title.text = entry.entryName;
        image.sprite = entry.mainImage;
        description.text = entry.rightText;

        for (int i = 0; i < repairObjects.Count; i++)
        {
            repairObjects[i].SetActive(false);
        }

        print("HELLO?????");
        for (int i = 0; i <= entry.structureData.repairItems.Count; i++)
        {
            if (i < entry.structureData.repairItems.Count)
            {
                repairObjects[i].SetActive(true);
                resourceIcons[i].gameObject.SetActive(true);
                resourceIcons[i].sprite = entry.structureData.repairItems[i].item.icon;
                resourceText[i].text = entry.structureData.repairItems[i].amount.ToString() + "x";
            }
            else
            {
                if (entry.structureData.mintRepairCost <= 0) break;
                //Show mint cost
                repairObjects[i].SetActive(true);
                resourceIcons[i].gameObject.SetActive(true);
                resourceIcons[i].sprite = mintSprite;
                resourceText[i].text = entry.structureData.mintRepairCost.ToString() + "x";
            }
        }
    }

    private void GetReferences()
    {
        if (resourceIcons.Count != 0 && resourceText.Count != 0) return;

        print("Getting references for " + this.gameObject + ".");
        resourceIcons = new List<Image>();
        resourceText = new List<TextMeshProUGUI>();

        for (int i = 0; i < repairObjects.Count; i++)
        {
            var icon = repairObjects[i].gameObject.transform.GetChild(1).GetComponent<Image>();
            var text = repairObjects[i].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

            resourceIcons.Add(icon);
            resourceText.Add(text);
        }
    }

    private IEnumerator DelayedUpdatePage(CodexEntries entry)
    {
        

        yield break;
    }
}
