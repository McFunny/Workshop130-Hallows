using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialNPC : NPC, ITalkable
{
    bool goneAtStart = true;
    bool finishedTalking = false;
    bool shotAt;

    public InventoryItemData seeds;

    Quest mainQuest;

    public GameObject tutorial;
    void Start()
    {
        mainQuest = QuestDatabase.Instance.GetMainQuest(0);
        if(MainMenuScript.loadingData) StartCoroutine(Despawn());
        else 
        {
            goneAtStart = false;
            TimeManager.Instance.stopTime = true;
            CabinFog f = FindObjectOfType<CabinFog>();
            if(f) Destroy(f.gameObject);
            AmbientAudioManager.Instance.playMusicAtStart = false;
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
        //QuestManager.Instance.AddQuest(mainQuest);

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
        shotAt = true;
        QuestManager.Instance.AddQuest(mainQuest);
    }

    IEnumerator Despawn()
    {
        if(goneAtStart)
        {
            Destroy(this.gameObject);
        }
        else
        {
            //QuestManager.Instance.AddQuest(mainQuest);
            FadeScreen.coverScreen = true;
            PlayerMovement.restrictMovementTokens++;
            TimeManager.Instance.stopTime = false;
            AmbientAudioManager.Instance.BeginPlayingMusic();
            yield return new WaitForSeconds(1.5f);
            PlayerMovement.restrictMovementTokens--;
            FadeScreen.coverScreen = false;

            if(!shotAt) tutorial.SetActive(true);

            Destroy(this.gameObject);
        }
    }
}
