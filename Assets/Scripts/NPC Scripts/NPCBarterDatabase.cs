using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Barter Object", menuName = "NPC Barter")]
public class NPCBarterDatabase : ScriptableObject
{
    public List<Barter> transactions = new List<Barter>();
}
[System.Serializable]
public class Barter
{
    public InventoryItemData itemForSale; //If there is more than 1, then give a pouch item filled with this reference instead
    public float barterChance = 100; //Chance of this barter being available in store
    public List<ItemWithAmount> itemsRequired = new List<ItemWithAmount>();
}
