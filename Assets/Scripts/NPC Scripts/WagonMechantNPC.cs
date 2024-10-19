using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonMerchantNPC : NPC, ITalkable
{
    private InventoryItemData lastSeenItem;

    //Find a way to get feedback on when a dialogue tree is finished by calling an event/delegate.

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
        {
            currentPath = -1;
            lastSeenItem = null;
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
        if(item.sellValueMultiplier == 0 || item.value == 0)
        {
            //Cannot Buy
            lastSeenItem = item;
            currentPath = 1;
            Talk();
        }
        else
        {
            //Can Buy
            if(lastSeenItem != item)
            {
                //Are you sure?
                lastSeenItem = item;
                if(HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize > 1) currentPath = 3;
                else currentPath = 0;
            }
            else
            {
                //Sold, remove item and gain money
                currentPath = 2;
            }
            Talk();
        }
        interactSuccessful = true;
    }
}
