using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToolPage : CodexPage
{
    public TextMeshProUGUI descriptionLeft;
    public override void UpdatePage(CodexEntries entry, GameObject catContainer, GameObject containerToOpen)
    {
        title.text = entry.entryName;
        image.sprite = entry.mainImage;
        descriptionLeft.text = entry.rightText;
    }
}
