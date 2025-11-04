using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MerchantLantern : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    //public Transform interior, exterior;
    [HideInInspector] public WagonMerchantNPC merchant;
    //public bool forceEnable = false; //MAKE THIS FALSE BEFORE BUILDING

    public Collider myCollider;
    public GameObject enabledObject;


    //////////////PLAYER LANTERN BOOLS/////////////////
    public bool playerOwnedLamp = false;
    bool interactedWith = false;

    public PopupScript enterPopup, exitPopup, blockedPopup;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    public void EnableSelf()
    {
        myCollider.enabled = true;
        enabledObject.SetActive(true);
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(playerOwnedLamp)
        {
            interactSuccessful = true;
            if(TownGate.Instance.location == PlayerLocation.InFarm) //In Farm
            {
                if(TravelCheck())
                {
                    if(interactedWith)
                    {
                        StartCoroutine(TakeToTransition());
                    }
                    else 
                    {
                        PopupHandler.Instance.AddToQueue(enterPopup);
                        StartCoroutine(InteractionTimer());
                    }
                }
                else PopupHandler.Instance.AddToQueue(blockedPopup);
            }
            else //In Wilderness
            {
                if(interactedWith) //Leave
                {
                    WildernessManager.Instance.ClearCreatures();
                    StartCoroutine(TakeToTown());
                }
                else //Ask to leave
                {
                    PopupHandler.Instance.AddToQueue(exitPopup);
                    StartCoroutine(InteractionTimer());
                }
            }
            return;
        }

        merchant.LanternInteraction();
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        
    }
    
    public void EndInteraction()
    {
       
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
    }

    IEnumerator TakeToTransition()
    {
        //restrict movement and darken screen
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        interactedWith = false;
        yield return new WaitForSeconds(3);
        WildernessTransitionManager.Instance.EnterTransition();
        //FadeScreen.coverScreen = false;
        //PlayerMovement.restrictMovementTokens--;
    }

    IEnumerator TakeToTown()
    {
        interactedWith = false;

        //restrict movement and darken screen
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        yield return new WaitForSeconds(2);
        WildernessManager.Instance.ExitWilderness();
        FadeScreen.coverScreen = false;
        PlayerMovement.restrictMovementTokens--;
    }

    bool TravelCheck()
    {
        if(TimeManager.Instance.currentHour >= 17 || !TimeManager.Instance.isDay || WildernessManager.Instance.visitedWilderness) return false;
        else return true;
    }

    IEnumerator InteractionTimer()
    {
        interactedWith = true;
        yield return new WaitForSeconds(3);
        interactedWith = false;
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(4);
        if(/*GameSaveData.Instance.wildernessIntroduced == true || */playerOwnedLamp) EnableSelf();
        else
        {
            myCollider.enabled = false;
            enabledObject.SetActive(false);
        }
    }

    public void ToggleHighlight(bool enable)
    {
        if(highlight.Count == 0) return;
        if(highlightMaterial.Count == 0)
        {
            foreach(GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if(enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach(GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if(!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach(GameObject thing in highlight) thing.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while(highlightEnabled)
        {
            do
            {
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power > 0.7f && highlightEnabled);
            do
            {
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power < 1.9f && highlightEnabled);
        }
    }
}
