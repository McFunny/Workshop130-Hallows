using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CatacombDoor : MonoBehaviour, IInteractable
{
    public InventoryItemData key;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public Transform interior, exterior;

    public bool debugMode = false;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(GameSaveData.Instance.catacombUnlocked || debugMode)
        {
            if(TownGate.Instance.location == PlayerLocation.InTown)
            {
                StartCoroutine(Transition(true));
            }
            else
            {
                StartCoroutine(Transition(false));
            }
            interactSuccessful = true;
            return;
        }
        interactSuccessful = false;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        ToolItem t_item = item as ToolItem;
        if (t_item)
        {
            interactSuccessful = false;
            return;
        }
        if(GameSaveData.Instance.catacombUnlocked || debugMode)
        {
            if(TownGate.Instance.location == PlayerLocation.InTown)
            {
                StartCoroutine(Transition(true));
            }
            else
            {
                StartCoroutine(Transition(false));
            }
            interactSuccessful = true;
            return;
        }
        if((item == key))
        {
            GameSaveData.Instance.catacombUnlocked = true;
            interactSuccessful = true;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            return;
        }
        interactSuccessful = false;
        
    }
    
    public void EndInteraction()
    {
       
    }

    IEnumerator Transition(bool goingToCrypt)
    {
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        AmbientAudioManager.Instance.ChangeMusic();
        yield return new WaitForSeconds(3);
        if(goingToCrypt)
        {
            PlayerInteraction.Instance.transform.position = interior.position;
            TownGate.Instance.Transition(PlayerLocation.InCrypt);
        }
        else
        {
            PlayerInteraction.Instance.transform.position = exterior.position;
            TownGate.Instance.Transition(PlayerLocation.InTown);
        }
        TimeManager.Instance.ToggleSkyLights();
        PlayerMovement.restrictMovementTokens--;
        FadeScreen.coverScreen = false;
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
