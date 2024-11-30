using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RascalNPC : NPC, ITalkable
{
    public InventoryItemData key, paleCarrot;

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
        {
            if(!GameSaveData.Instance.rascalWantsFood)
            {
                currentPath = 1;
                currentType = PathType.Quest;
                GameSaveData.Instance.rascalWantsFood = true; 
            }
            else
            {
                currentPath = -1;
                currentType = PathType.Default;
            }
        }
        Talk();
        interactSuccessful = true;
    }

    public void Talk()
    {
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(dialogueController.IsInterruptable() == false)
        {
            interactSuccessful = true;
            return;
        } 

        if(item == paleCarrot && GameSaveData.Instance.rascalWantsFood == true && GameSaveData.Instance.rascalMentionedKey == false)
        {
            currentPath = 2;
            currentType = PathType.Quest;
            GameSaveData.Instance.rascalMentionedKey = true;
        }

        else if(item == key)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }
        else
        {
            currentPath = 0;
            currentType = PathType.ItemSpecific;
        }

        //code for the item being edible
        Talk();

        interactSuccessful = true;
    }

    public override void PlayerLeftRadius()
    {

    }

}
