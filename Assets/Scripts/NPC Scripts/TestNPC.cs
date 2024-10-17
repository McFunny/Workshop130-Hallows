using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNPC : NPC, ITalkable
{

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        Talk();
        interactSuccessful = true;
        Debug.Log("NPC Interact Successful");
    }

    public void Talk()
    {
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        Talk();
        interactSuccessful = true;
        Debug.Log("NPC Interact Successful");
    }
}
