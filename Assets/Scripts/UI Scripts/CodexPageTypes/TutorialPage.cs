using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialPage : CodexPage
{
    public TextMeshProUGUI descriptionLeft, descriptionRight;

    public override void UpdatePage(CodexEntries entry, GameObject catContainer, GameObject containerToOpen)
    {
        title.text = entry.entryName;
        image.sprite = entry.mainImage;
        descriptionLeft.text = entry.leftText;
        descriptionRight.text = entry.rightText;
    }
}
