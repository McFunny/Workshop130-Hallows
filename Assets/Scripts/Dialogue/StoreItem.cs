using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

//[RequireComponent(typeof(SphereCollider))]
public class StoreItem : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public InventoryItemData itemData;

    private Collider myCollider;

    public SpriteRenderer r;
    public Color original, highlighted;
    public GameObject costObject, arrowObject, barterObject;
    public TextMeshProUGUI costText, stockText;

    public NPC seller;

    public int cost, amountLeft, amountGiven;

    public List<ItemWithAmount> barterCost = new List<ItemWithAmount>();

    public bool clearUponPurchase = true;

    bool awakeOver = false;
    private ToolTipScript toolTipScript;

    public bool isAnimalCrate = false;

    StorePetCage cageScript;

    private void Awake()
    {
        myCollider = GetComponent<Collider>();
        cageScript = GetComponent<StorePetCage>();
        if (!itemData || !seller) Empty();
        toolTipScript = GameObject.Find("BarterCanvas").GetComponent<ToolTipScript>();
    }

    void Start()
    {
        awakeOver = true;
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if (itemData && (cost > 0 || barterCost.Count > 0))
        {
            seller.PurchaseAttempt(this);
            toolTipScript.panel.SetActive(true);
            if(itemData != null)
            {
                toolTipScript.UpdateTooltipBarter(itemData, barterCost, cost);
            }
            else toolTipScript.panel.SetActive(false);
            
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
        CritterItem c = newItem as CritterItem;
        if(c)
        {
            if(cageScript)
            {
                if(c.petOverride) cageScript.RefreshCage(c.petType);
                else cageScript.RefreshCage(c.critterRef.critterType);
            }
        }
        else 
        {
            r.sprite = newItem.icon;
            stockText.text = "";
            barterObject.SetActive(false);
        }
        itemData = newItem;
        cost = _cost;
        amountLeft = 1;
        amountGiven = 1;
        costText.text = cost.ToString();
        if(cost > 0) costObject.SetActive(true);
        myCollider.enabled = true;
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

    public void ChangeAmountGiven(int num)
    {
        amountGiven = num;
    }

    public void Empty()
    {
        if(cageScript) cageScript.ClearCage();
        else 
        {
            r.sprite = null;
            barterObject.SetActive(false);
            barterCost.Clear();
            stockText.text = "";
            myCollider.enabled = false;
        }
        itemData = null;
        cost = 0;
        costText.text = "";
        costObject.SetActive(false);
        amountLeft = 0;
        amountGiven = 1;
        if(awakeOver) ParticlePoolManager.Instance.GrabSparkParticle().transform.position = transform.position;
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
        toolTipScript.panel.SetActive(false);
        
        if (clearUponPurchase == false) return;
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
        toolTipScript.panel.SetActive(false);

        if (clearUponPurchase == false) return;
        amountLeft--;
        if(amountLeft == 1)  stockText.text = "";
        else stockText.text = "x " + amountLeft;
    }

    public void ToggleHighlight(bool enable)
    {
        if(!r) return;
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
