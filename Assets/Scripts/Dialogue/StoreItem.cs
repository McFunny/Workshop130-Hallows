using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public class StoreItem : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public InventoryItemData itemData;

    private SphereCollider myCollider;

    public SpriteRenderer r;
    public Color original, highlighted;
    public GameObject costObject, arrowObject;
    public TextMeshProUGUI costText;

    public NPC seller;

    public int cost;

    bool awakeOver = false;

    private void Awake()
    {
        myCollider = GetComponent<SphereCollider>();
        if(!itemData || !seller) Empty();
    }

    void Start()
    {
        awakeOver = true;
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(cost > 0)
        {
            seller.PurchaseAttempt(this);
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = true;
    }

    public void EndInteraction()
    {
        throw new System.NotImplementedException();
    }

    public void RefreshItem(InventoryItemData newItem, int _cost)
    {
        r.sprite = newItem.icon;
        itemData = newItem;
        cost = _cost;
        costText.text = cost.ToString();
        costObject.SetActive(true);
        myCollider.enabled = true;
    }

    public void Empty()
    {
        r.sprite = null;
        itemData = null;
        cost = 0;
        costText.text = "";
        costObject.SetActive(false);
        myCollider.enabled = false;
        if(awakeOver) ParticlePoolManager.Instance.GrabSparkParticle().transform.position = transform.position;
    }

    public void ToggleHighlight(bool enable)
    {
        if(enable)
        {
            r.color = highlighted;
        }

        if(!enable)
        {
            r.color = original;
        }
    }
}
