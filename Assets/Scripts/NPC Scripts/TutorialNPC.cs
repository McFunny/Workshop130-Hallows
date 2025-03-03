using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialNPC : NPC, ITalkable
{
    bool goneAtStart = true;
    bool finishedTalking = false;

    public InventoryItemData seeds;

    public Quest mainQuest;
    void Start()
    {
        if(MainMenuScript.loadingData) StartCoroutine(Despawn());
        else 
        {
            goneAtStart = false;
            TimeManager.Instance.stopTime = true;
        }
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
        interactSuccessful = false;
        return;
    }

    public override void OnConvoEnd()
    {
        StartCoroutine(Despawn());
    }

    public override void ShotAt()
    {
        if(dialogueController.IsTalking()) return;
        currentPath = 0;
        currentType = PathType.Misc;
        Talk();
        finishedTalking = true;
    }

    IEnumerator Despawn()
    {
        if(goneAtStart) Destroy(this.gameObject);
        else
        {
            FadeScreen.coverScreen = true;
            PlayerMovement.restrictMovementTokens++;
            yield return new WaitForSeconds(1.5f);
            PlayerMovement.restrictMovementTokens--;
            FadeScreen.coverScreen = false;
            TimeManager.Instance.stopTime = false;

            QuestManager.Instance.AddQuest(mainQuest);

            Destroy(this.gameObject);
        }
    }
}
