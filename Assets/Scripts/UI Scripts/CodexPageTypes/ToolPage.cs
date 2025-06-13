using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToolPage : CodexPage
{
    public TextMeshProUGUI descriptionLeft;
    public override void UpdatePage(CodexEntries entry, GameObject catContainer, GameObject containerToOpen)
    {
        print("Tool page opened.");
        title.text = entry.entryName;
        image.sprite = entry.mainImage;
        descriptionLeft.text = entry.rightText;

        catContainer.SetActive(false);
        containerToOpen.SetActive(true);
    }
}
