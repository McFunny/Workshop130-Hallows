using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialNPC : NPC, ITalkable
{
    bool goneAtStart = true;
    bool finishedTalking = false;

    public InventoryItemData seeds;
    void Start()
    {
        if(GameSaveData.Instance.tutorialMerchantSpoke) StartCoroutine(Despawn());
        else goneAtStart = false;
    }

    //void Update

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(finishedTalking)
        {
            interactSuccessful = true;
            return;
        }
        currentPath = -1;
        currentType = PathType.Default;
        dialogueController.SetItem(seeds, 8);
        Talk();
        interactSuccessful = true;
        finishedTalking = true;
    }

    public void Talk()
    {
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText, currentPath, currentType);
    }

    public override void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = true;
        return;
    }

    public override void OnConvoEnd()
    {
        StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        if(goneAtStart) Destroy(this.gameObject);
        else
        {
            FadeScreen.coverScreen = true;
            PlayerMovement.restrictMovementTokens++;
            yield return new WaitForSeconds(1);
            PlayerMovement.restrictMovementTokens--;
            FadeScreen.coverScreen = false;
            Destroy(this.gameObject);
        }
    }
}
