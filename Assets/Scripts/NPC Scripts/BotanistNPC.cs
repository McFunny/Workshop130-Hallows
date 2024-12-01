using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotanistNPC : NPC, ITalkable
{
    public InventoryItemData fertalizerT, fertalizerG, fertalizerI;

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
            if(movementHandler.isWorking)
            {
                currentPath = 0;
                currentType = PathType.Misc;
            }
            else if(currentPath == -1)
            {
                int i = Random.Range(0, dialogueText.fillerPaths.Length);
                currentPath = i;
                currentType = PathType.Filler;
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

        if(item == fertalizerI || item == fertalizerT || item == fertalizerG)
        {
            currentPath = 1;
            currentType = PathType.ItemSpecific;
        }

        else if(item.staminaValue > 0)
        {
            if(!NPCManager.Instance.botanistFed)
            {
                currentPath = 0;
                currentType = PathType.ItemRecieved;
                NPCManager.Instance.botanistFed = true;
            }
            else
            {
                currentPath = 1;
                currentType = PathType.ItemRecieved;
            }
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

    }

    public override void OnConvoEnd()
    {
        currentPath = -1;
    }

}

