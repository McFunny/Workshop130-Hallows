using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spa : MonoBehaviour, IInteractable
{
    bool playerInSpa;

    public InventoryItemData waterCan, waterGun;

    public InventoryItemData bathBomb;

    public ParticleSystem splashVFX;

    public Transform focalPoint;

    void Start()
    {
        StartCoroutine(Heal());
    }

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if((item != waterCan && item != waterGun) || PlayerInteraction.Instance.waterHeld == PlayerInteraction.Instance.maxWaterHeld)
        {
            interactSuccessful = false;
            return;
        }
        interactSuccessful = true;
        PlayerInteraction.Instance.waterHeld = PlayerInteraction.Instance.maxWaterHeld;
        splashVFX.Play();
        
    }
    
    public void EndInteraction()
    {
       
    }

    public void ReturnFocalPoint(out Transform _focalPoint)
    {
        _focalPoint = focalPoint;
    }

    public void ToggleHighlight(bool enable)
    {
        //
    }

    IEnumerator HightlightFlash()
    {
        yield break;
    }

    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            playerInSpa = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            playerInSpa = false;
        }
    }

    IEnumerator Heal()
    {
        do
        {
            yield return new WaitForSeconds(0.5f);
            if(playerInSpa && PlayerInteraction.Instance.stamina < PlayerInteraction.Instance.maxStamina * 0.75f) PlayerInteraction.Instance.stamina += 5;
        }
        while(gameObject.activeSelf);
    }
}
