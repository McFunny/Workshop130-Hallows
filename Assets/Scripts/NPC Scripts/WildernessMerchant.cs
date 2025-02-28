using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessMerchant : NPC, ITalkable
{
    //private InventoryItemData lastSeenItem;
    [HideInInspector] public bool interactedWithRecently;
    public Transform merchantWagonPos;
    public Transform merchant;

    void Start()
    {
        WildernessManager.Instance.wagon = this;
    }

    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(dialogueController.IsTalking() == false)
        {
            if(interactedWithRecently) //Leave
            {
                //leave
                WildernessManager.Instance.ClearCreatures();
                StartCoroutine(TakeToTown());
                interactSuccessful = true;
                return;
            }
            else //Ask to leave
            {
                currentPath = 0;
                currentType = PathType.Default;
                interactedWithRecently = true;
            }
            dialogueController.SetInterruptable(false);

            anim.SetTrigger("IsTalking");
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
        
        interactSuccessful = true;
    }

    public override void PlayerLeftRadius()
    {
        interactedWithRecently = false;
    }

    public IEnumerator PlayerTooLate()
    {
        TimeManager.Instance.stopTime = true;
        yield return new WaitUntil(() => PlayerMovement.accessingInventory == false);
        TimeManager.Instance.stopTime = false;

        Vector3 pos = PlayerInteraction.Instance.transform.position + PlayerInteraction.Instance.mainCam.transform.forward * -5;
        pos.y = PlayerInteraction.Instance.transform.position.y;
        //pos = new Vector3(pos.x, pos.y - 1, pos.z);
        merchant.position = pos;
        currentPath = 0;
        currentType = PathType.Misc;
        dialogueController.SetInterruptable(false);
        Talk();
        StartCoroutine(WaitUntilDoneTalking());
    }

    IEnumerator TakeToTown()
    {
        interactedWithRecently = false;

        //restrict movement and darken screen
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        yield return new WaitForSeconds(2);
        WildernessManager.Instance.ExitWilderness();
        FadeScreen.coverScreen = false;
        PlayerMovement.restrictMovementTokens--;
        merchant.position = merchantWagonPos.position;
    }

    IEnumerator WaitUntilDoneTalking()
    {
        PlayerCam.Instance.NewObjectOfInterest(eyeLine.position);
        WildernessManager.Instance.ClearCreatures();
        yield return new WaitUntil(() => DialogueController.Instance.IsTalking() == false);
        PlayerCam.Instance.ClearObjectOfInterest();
        PlayerInteraction.Instance.currentMoney -= 250;
        if(PlayerInteraction.Instance.currentMoney < 0) PlayerInteraction.Instance.currentMoney = 0;
        StartCoroutine(TakeToTown());
    }
}
