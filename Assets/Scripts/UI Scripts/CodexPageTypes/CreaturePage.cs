using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CreaturePage : CodexPage
{
    [SerializeField] private TextMeshProUGUI description, timesKilled;
    public override void UpdatePage(CodexEntries entry, GameObject catContainer, GameObject containerToOpen)
    {
        title.text = entry.entryName;
        image.sprite = entry.mainImage;
        description.text = entry.rightText;
        timesKilled.text = "Times Killed: " + entry.creatureData.amountKilled;

        catContainer.SetActive(false);
        containerToOpen.SetActive(true);
    }
}
