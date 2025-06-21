using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BugPage : CodexPage
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    public override void UpdatePage(CodexEntries entry, Quest quest)
    {
        title.text = entry.entryName;
        image.sprite = entry.mainImage;
    }
}
