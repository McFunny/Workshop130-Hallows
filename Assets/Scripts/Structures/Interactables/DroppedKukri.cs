using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DroppedKukri : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public InventoryItemData kukriItem;

    public Transform parentObject;
    public bool stuck = false;
    public Rigidbody rb;

    public static DroppedKukri Instance;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            print("Destroyed Copy");
            return;
        }
        else
        {
            Instance = this;
        }

        StartCoroutine(DistanceCheck());
    }


    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(kukriItem, 1);
        interactSuccessful = addedSuccessfully;
        if (addedSuccessfully)
        {
            HotbarDisplay display = FindObjectOfType<HotbarDisplay>();
            int i = display.FindItemInHotbar(kukriItem);
            if(i != -1)
            {
                display.SelectHotbarSlot(i);
            }

            Destroy(this.gameObject);
            //PlayerInteraction.Instance.droppedKukri = false;
            PlayerInteraction.Instance.lostKukri = false;
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

    public void StuckInObject(Transform obj)
    {
        rb.isKinematic = true;
        stuck = true;
        parentObject = obj;
    }

    void Update()
    {
        if(parentObject)
        {
            transform.position = parentObject.position;
            transform.rotation = parentObject.rotation;
        }
        else if(stuck)
        {
            rb.isKinematic = false;
            stuck = false;
        }
    }

    IEnumerator DistanceCheck()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(10);
            if(!stuck && Vector3.Distance(transform.position, PlayerInteraction.Instance.playerFeet.position) > 60) Destroy(gameObject);
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
