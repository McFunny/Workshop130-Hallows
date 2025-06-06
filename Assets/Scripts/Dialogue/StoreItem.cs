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
    public GameObject costObject, arrowObject, barterObject;
    public TextMeshProUGUI costText, stockText;

    public NPC seller;

    public int cost, amountLeft;

    public List<ItemWithAmount> barterCost = new List<ItemWithAmount>();

    public bool clearUponPurchase = true;

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
        amountLeft = 1;
        stockText.text = "";
        costText.text = cost.ToString();
        if(cost > 0) costObject.SetActive(true);
        myCollider.enabled = true;
        barterObject.SetActive(false);
    }

    public void RefreshItem(InventoryItemData newItem, int _cost, List<ItemWithAmount> newCost, int _amount)
    {
        r.sprite = newItem.icon;
        itemData = newItem;
        cost = _cost;

        amountLeft = _amount;
        if(amountLeft >= 99) clearUponPurchase = false;
        if(amountLeft == 0) amountLeft = 1;
        if(amountLeft == 1 || !clearUponPurchase)  stockText.text = "";
        else stockText.text = "x " + amountLeft;

        barterCost.Clear();
        for(int i = 0; i < newCost.Count; i++)
        {
            barterCost.Add(new ItemWithAmount(newCost[i].item, newCost[i].amount));
        }
        costText.text = cost.ToString();
        if(cost > 0) costObject.SetActive(true);
        if(newCost.Count > 0) barterObject.SetActive(true);
        myCollider.enabled = true;
    }

    public void Empty()
    {
        r.sprite = null;
        itemData = null;
        cost = 0;
        costText.text = "";
        costObject.SetActive(false);
        amountLeft = 0;
        stockText.text = "";
        myCollider.enabled = false;
        if(awakeOver) ParticlePoolManager.Instance.GrabSparkParticle().transform.position = transform.position;
        barterObject.SetActive(false);
        barterCost.Clear();
        clearUponPurchase = true;
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

    public void CompleteTrade() //Completed a barter trade
    {
        QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetTutorialQuest(302)); //Completed the Barter quest if assigned

        PlayerInventoryHolder.Instance.RemoveItemsFromBothInventories(barterCost);
        PlayerInventoryHolder.Instance.UpdateInventory();
        if(clearUponPurchase && amountLeft == 1)
        {
            Empty();
            return;
        }
        ParticlePoolManager.Instance.GrabSparkParticle().transform.position = transform.position;
        
        if(clearUponPurchase == false) return;
        amountLeft--;
        if(amountLeft == 1)  stockText.text = "";
        else stockText.text = "x " + amountLeft;
    }

    public void CompletePurchase() //Completed a store purchase
    {
        QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.GetTutorialQuest(300)); //Completed the Purchase Seeds Quest if assigned

        if(clearUponPurchase && amountLeft == 1)
        {
            Empty();
            return;
        }
        ParticlePoolManager.Instance.GrabSparkParticle().transform.position = transform.position;

        if(clearUponPurchase == false) return;
        amountLeft--;
        if(amountLeft == 1)  stockText.text = "";
        else stockText.text = "x " + amountLeft;
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
