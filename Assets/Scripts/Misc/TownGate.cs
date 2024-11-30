using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TownGate : MonoBehaviour, IInteractable
{
    public GameObject townMist, farmMist;
    public Transform townPos, farmPos;
    public static bool inTown = false;
    bool transitioning = false;

    public AudioSource source;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(transitioning || (!TimeManager.Instance.isDay && !inTown))
        {
            interactSuccessful = false;
            return;
        }
        StartCoroutine(Transition());
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        return;
        
    }
    
    public void EndInteraction()
    {
       
    }

    IEnumerator Transition()
    {
        transitioning = true;
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        source.Play();
        yield return new WaitForSeconds(1);
        if(inTown)
        {
            PlayerInteraction.Instance.transform.position = farmPos.position;
            townMist.gameObject.SetActive(false);
            farmPos.gameObject.SetActive(true);
            inTown = false;
        }
        else
        {
            PlayerInteraction.Instance.transform.position = townPos.position;
            townMist.gameObject.SetActive(true);
            farmPos.gameObject.SetActive(false);
            inTown = true;
        }
        PlayerMovement.restrictMovementTokens--;
        FadeScreen.coverScreen = false;
        transitioning = false;
    }

}
