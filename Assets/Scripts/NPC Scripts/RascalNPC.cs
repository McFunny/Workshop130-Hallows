using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RascalNPC : NPC, ITalkable
{
    //special interactions for: Showing the key
    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
        {
            currentPath = -1;
            dialogueController.SetInterruptable(false);
        }
        Talk();
        interactSuccessful = true;
    }

    public void Talk()
    {
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(dialogueController.IsInterruptable() == false)
        {
            interactSuccessful = true;
            return;
        } 

        interactSuccessful = true;
    }

    public override void PlayerLeftRadius()
    {

    }

}
