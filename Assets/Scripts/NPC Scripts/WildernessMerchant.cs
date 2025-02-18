using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessMerchant : NPC, ITalkable
{
    //private InventoryItemData lastSeenItem;
    [HideInInspector] public bool interactedWithRecently;

    void Start()
    {
        //
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

    IEnumerator TakeToTown()
    {
        interactedWithRecently = false;

        //restrict movement and darken screen
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        yield return new WaitForSeconds(3);
        WildernessManager.Instance.ExitWilderness();
        FadeScreen.coverScreen = false;
        PlayerMovement.restrictMovementTokens--;
    }
}
