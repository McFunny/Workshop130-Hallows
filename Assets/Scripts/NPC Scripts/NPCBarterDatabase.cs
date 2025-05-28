using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Barter Object", menuName = "NPC Barter")]
public class NPCBarterDatabase : ScriptableObject
{
    public List<Barter> transactions = new List<Barter>();

    [ContextMenu("Name Entries")]
    void RefreshEntries()
    {
        for(int i = 0; i < transactions.Count; i++)
        {
            if(transactions[i].itemForSale)
            {
                transactions[i].name = transactions[i].itemForSale.name;
                if(transactions[i].useItemPrice) transactions[i].mintCost = (int)transactions[i].itemForSale.value;
            }
        }
    }
}
[System.Serializable]
public class Barter
{
    [HideInInspector] public string name;
    //public int townLevelReq = 0; //The level the town or shop should be at to sell
    public InventoryItemData itemForSale; //If there is more than 1, then give a pouch item filled with this reference instead
    public bool useItemPrice = false; //If true, mintCost is = to the item's default price
    public int mintCost;
    public int amountForSale = 1;
    public float barterChance = 100; //Chance of this barter being available in store
    public List<ItemWithAmount> itemsRequired = new List<ItemWithAmount>();
}
