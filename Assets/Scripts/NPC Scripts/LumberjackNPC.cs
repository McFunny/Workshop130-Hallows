using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LumberjackNPC : NPC, ITalkable
{
    //public InventoryItemData key, paleCarrot;

    void Awake()
    {
        movementHandler = GetComponent<NPCMovement>();
        faceCamera = GetComponent<FaceCamera>();
        faceCamera.enabled = false;
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
        {
            if(GameSaveData.Instance.rascalMentionedKey)
            {
                if(!GameSaveData.Instance.lumber_offersDeal)
                {
                    GameSaveData.Instance.lumber_offersDeal = true;
                    currentPath = 0;
                    currentType = PathType.Quest;
                }
                else if(!GameSaveData.Instance.lumber_choppedTree)
                {
                    if(PlayerInteraction.Instance.currentMoney >= 200)
                    {
                        PlayerInteraction.Instance.currentMoney -= 200;
                        currentPath = 2;
                        currentType = PathType.Quest;
                        GameSaveData.Instance.lumber_choppedTree = true;
                    }
                    else
                    {
                        currentPath = 1;
                        currentType = PathType.Quest;
                    }
                }
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
        movementHandler.TalkToPlayer();
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

        currentPath = 0;
        currentType = PathType.ItemSpecific;

        //code for the item being edible
        Talk();

        interactSuccessful = true;
    }

    public override void PlayerLeftRadius()
    {

    }

}

