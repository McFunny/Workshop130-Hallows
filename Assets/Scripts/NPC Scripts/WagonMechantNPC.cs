using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonMerchantNPC : NPC, ITalkable
{
    private InventoryItemData lastSeenItem;

    //Find a way to get feedback on when a dialogue tree is finished by calling an event/delegate.

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        currentPath = -1;
        Talk();
        interactSuccessful = true;
        lastSeenItem = null;
    }

    public void Talk()
    {
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
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
                currentPath = 0;
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
