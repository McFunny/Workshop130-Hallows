using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TicketBox : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    bool hasTicket = false;
    public AudioSource source;
    public AudioClip grabTicket;

    public InventoryItemData ticket;

    public Animator anim;

    public SpriteRenderer r;

    public WagonMerchantNPC merchant;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;


    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;

        if(hasTicket)
        {
            bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(ticket, GameSaveData.Instance.tTicketsAvailable);
            if (addedSuccessfully)
            {
                hasTicket = false;
                GameSaveData.Instance.tTicketsHeld += GameSaveData.Instance.tTicketsAvailable;
                GameSaveData.Instance.tTicketsAvailable = 0;
                anim.SetBool("IsOpened", false);
                r.enabled = false;
            }
        }

        merchant.InteractWithTicketBox();
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

    public void Update()
    {
        if(!hasTicket && highlightEnabled) ToggleHighlight(false);

        if(GameSaveData.Instance.tTicketsAvailable > 0 && !hasTicket)
        {
            hasTicket = true;
            anim.SetBool("IsOpened", true);
            r.enabled = true;
        }
    }


    public void ToggleHighlight(bool enable)
    {
        if(highlight.Count == 0) return;
        if(highlightMaterial.Count == 0)
        {
            foreach(GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if(enable && !highlightEnabled && hasTicket)
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
