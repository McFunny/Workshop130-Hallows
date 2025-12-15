using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class SeedPodInteractable : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public InventoryItemData nutItem;

    public Rigidbody rb;

    public PopupScript emptyhandP;

    public AudioClip damageSFX, pickupSFX, hitGroundSFX;

    bool hitGround;

    void Awake()
    {
        StartCoroutine(DistanceCheck());
    }


    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != null) 
        {
            PopupHandler.Instance.AddToQueue(emptyhandP);
            return;
        }
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(nutItem, 1);
        interactSuccessful = addedSuccessfully;
        if (addedSuccessfully)
        {
            HotbarDisplay display = FindObjectOfType<HotbarDisplay>();
            int i = display.FindItemInHotbar(nutItem);
            if(i != -1 && i != HotbarDisplay.Instance.GetCurrentSlotIndex())
            {
                display.SelectHotbarSlot(i);
            }

            AudioPoolManager.Instance.PlayClipAtPosition(pickupSFX, transform.position, 0.8f, 20);

            Destroy(this.gameObject);
        }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        return;
        
    }
    
    public void EndInteraction()
    {
       
    }

    IEnumerator DistanceCheck()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(10);
            if(Vector3.Distance(transform.position, PlayerInteraction.Instance.playerFeet.position) > 300) Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 7 && rb.velocity.magnitude > 5 && !hitGround)
        {
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            AudioPoolManager.Instance.PlayClipAtPosition(hitGroundSFX, transform.position, 0.8f, 20);
            return;
        }
        if(other.gameObject.layer == 9 && rb.velocity.magnitude > 10);
        {
            var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(5);
                creature.PlayHitParticle(creature.corpseParticleTransform.position);
                AudioPoolManager.Instance.PlayClipAtPosition(damageSFX, transform.position, 0.8f, 20);
            }
        }
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
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
            foreach(GameObject thing in highlight) if(thing) thing.SetActive(false);
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
