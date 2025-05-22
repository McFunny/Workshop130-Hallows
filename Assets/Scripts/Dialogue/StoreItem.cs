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

    public List<ItemWithAmount> barterCost = new List<ItemWithAmount>();

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
        if(cost > 0 || barterCost.Count > 0)
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

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
    }

    public void RefreshItem(InventoryItemData newItem, int _cost)
    {
        r.sprite = newItem.icon;
        itemData = newItem;
        cost = _cost;
        costText.text = cost.ToString();
        if(cost > 0) costObject.SetActive(true);
        myCollider.enabled = true;
    }

    public void RefreshItem(InventoryItemData newItem, int _cost, List<ItemWithAmount> newCost)
    {
        r.sprite = newItem.icon;
        itemData = newItem;
        cost = _cost;
        barterCost.Clear();
        for(int i = 0; i < newCost.Count; i++)
        {
            barterCost.Add(new ItemWithAmount(newCost[i].item, newCost[i].amount));
        }
        costText.text = cost.ToString();
        if(cost > 0) costObject.SetActive(true);
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
        barterCost.Clear();
    }

    public bool CanAffordTrade()
    {
        if(PlayerInteraction.Instance.currentMoney < cost) return false;
        for(int i = 0; i < barterCost.Count; i++)
        {
            int amountToFind = barterCost[i].amount;
            if(PlayerInventoryHolder.Instance.ReturnItemCountInPlayerInventory(barterCost[i].item) < amountToFind) return false;
        }
        return true;
    }

    public void CompleteTrade()
    {
        PlayerInventoryHolder.Instance.RemoveItemsFromBothInventories(barterCost);
        PlayerInventoryHolder.Instance.UpdateInventory();
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
