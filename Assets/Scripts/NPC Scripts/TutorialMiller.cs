using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialMiller : NPC, ITalkable
{
    bool introducedTutorial, finished, justGavePen;

    public InventoryItemData penKit;

    bool spawnedHog, petHog;
    public GameObject hogPrefab;
    TruffleHog newHog;

    public PopupScript petP, renameP, placePenP;

    public static TutorialMiller Instance;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        base.Awake();
    }

    void Start()
    {
        TimeManager.Instance.stopTime = true;
        StartCoroutine(TutorialStart());
    }

    //void Update

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(finished)
        {
            interactSuccessful = true;
            return;
        }
        if (dialogueController.IsTalking() == false && dialogueController.FreeToSpeak(this))
        {
            if(petHog)
            {
                currentPath = 2;
                currentType = PathType.Misc;
                finished = true;
            }
            else if(introducedTutorial)
            {
                currentPath = 0;
                currentType = PathType.Misc;
            }
            else if(!PlayerInventoryHolder.Instance.IsInventoryFull())
            {
                currentPath = -1;
                currentType = PathType.Default;
                dialogueController.SetItem(penKit, 1);
                introducedTutorial = true;
            }
            else //Inventory is full
            {
                currentPath = 1;
                currentType = PathType.Misc;
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
        interactSuccessful = false;
        return;
    }

    public override void OnConvoEnd()
    {
        if(finished) StartCoroutine(Despawn());

        if(!justGavePen)
        {
            PopupHandler.Instance.AddToQueue(placePenP);
            justGavePen = true;
        }
    }

    IEnumerator TutorialStart()
    {
        FadeScreen.coverScreen = true;
        PlayerMovement.restrictMovementTokens++;
        yield return new WaitForSeconds(1.5f);
        PlayerMovement.restrictMovementTokens--;
        FadeScreen.coverScreen = false;

        PlayerCam.Instance.NewObjectOfInterest(eyeLine.position);
        Interact(PlayerInteraction.Instance, out bool success);
    }

    IEnumerator Despawn()
    {
        FadeScreen.coverScreen = true;
        PlayerMovement.restrictMovementTokens++;
        TimeManager.Instance.stopTime = false;
        yield return new WaitForSeconds(1.5f);
        PlayerMovement.restrictMovementTokens--;
        FadeScreen.coverScreen = false;
        PlayerInteraction.Instance.invincible = false;

        PopupHandler.Instance.ClearQueue();
        newHog.burrowsToDig = 3;

        Destroy(this.gameObject);
    }

    public void HogPetted()
    {
        if(petHog) return;
        petHog = true;
        PopupHandler.Instance.AddToQueue(renameP);
    }

    public void SpawnHog()
    {
        if(spawnedHog) return;
        spawnedHog = true;
        newHog = Instantiate(hogPrefab, WagonManager.Instance.farmWagon.critterPos.position, Quaternion.identity).GetComponent<TruffleHog>();
        newHog.burrowsToDig = 0;

        PopupHandler.Instance.AddToQueue(petP);
    }
}
