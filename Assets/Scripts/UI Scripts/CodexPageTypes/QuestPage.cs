using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestPage : CodexPage
{
    [SerializeField] TextMeshProUGUI description;
    public override void UpdatePage(CodexEntries entry, Quest quest)
    {
        title.text = quest.assignee.ToString() + ": " + quest.name;
        description.text = quest.description;

    }
}
