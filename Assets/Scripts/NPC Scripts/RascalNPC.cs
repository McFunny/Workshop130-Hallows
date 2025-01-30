using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RascalNPC : NPC, ITalkable
{
    public InventoryItemData key, paleCarrot;

    void Start()
    {
        anim.SetBool("IsLeaning", true);
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
        {
            if(!GameSaveData.Instance.rascalMet)
            {
                currentPath = -1;
                currentType = PathType.Default;
                GameSaveData.Instance.rascalMet = true;
            }
            else if(!GameSaveData.Instance.rascalWantsFood)
            {
                currentPath = 1;
                currentType = PathType.Quest;
                GameSaveData.Instance.rascalWantsFood = true; 
            }
            else
            {
                if(NPCManager.Instance.rascalSpoke)
                {
                    interactSuccessful = false;
                    return;
                }
                if(currentPath == -1)
                {
                    int i = Random.Range(0, dialogueText.fillerPaths.Length);
                    currentPath = i;
                }
                //currentPath = -1;
                currentType = PathType.Filler;
                NPCManager.Instance.rascalSpoke = true;
            }
        }
        Talk();
        interactSuccessful = true;
    }

    public void Talk()
    {
        //anim.SetTrigger("IsTalking");
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem tItem = item as ToolItem;
        if(dialogueController.IsInterruptable() == false || tItem)
        {
            interactSuccessful = false;
            return;
        } 

        if(item == paleCarrot && GameSaveData.Instance.rascalWantsFood == true && GameSaveData.Instance.rascalMentionedKey == false)
        {
            currentPath = 2;
            currentType = PathType.Quest;
            GameSaveData.Instance.rascalMentionedKey = true;
            //anim.SetTrigger("TakeItem");
        }

        else if(item == key)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }
        else if(item.staminaValue > 0)
        {
            currentPath = 0;
            currentType = PathType.ItemRecieved;
            /*
            if(!NPCManager.Instance.rascalFed)
            {
                currentPath = 0;
                currentType = PathType.ItemRecieved;
                NPCManager.Instance.rascalFed = true;
                //anim.SetTrigger("TakeItem");
            }
            else
            {
                currentPath = 1;
                currentType = PathType.ItemRecieved;
            }
            */
            //Its consumable and giftable
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
        base.PlayerLeftRadius();
    }

    public override void OnConvoEnd()
    {
        currentPath = -1;
    }

}
