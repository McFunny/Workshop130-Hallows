using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Survival Merchant Barter Database", menuName = "NPC Objects/Survival Merchant Barter Database")]
public class SurvivalMechantBarterDatabase : NPCBarterDatabase
{


    public List<Barter> alwaysThere = new List<Barter>(); //To always be included in pool
    public List<Barter> seeds = new List<Barter>(); //To not be included in pool and instead specifically referenced
    public List<Barter> structures = new List<Barter>(); //To not be included in pool and instead specifically referenced
    public List<Barter> furniture = new List<Barter>(); //To not be included in pool and instead specifically referenced
    public List<Barter> specialObjs = new List<Barter>(); //To not be included in pool and instead specifically referenced
    public List<Barter> trinkets = new List<Barter>(); //To not be included in pool and instead specifically referenced



    [ContextMenu("Name all Entries")]
    void RefreshEntries()
    {
        for (int i = 0; i < transactions.Count; i++)
        {
            if (transactions[i].itemForSale)
            {
                transactions[i].name = transactions[i].itemForSale.name;
                if (transactions[i].useItemPrice) transactions[i].mintCost = (int)transactions[i].itemForSale.value;
            }
        }

        for (int i = 0; i < uniqueTransactions.Count; i++)
        {
            if (uniqueTransactions[i].itemForSale)
            {
                uniqueTransactions[i].name = uniqueTransactions[i].itemForSale.name;
                if (uniqueTransactions[i].useItemPrice) uniqueTransactions[i].mintCost = (int)uniqueTransactions[i].itemForSale.value;
            }
        }

        for (int i = 0; i < uniqueTransactions2.Count; i++)
        {
            if (uniqueTransactions2[i].itemForSale)
            {
                uniqueTransactions2[i].name = uniqueTransactions2[i].itemForSale.name;
                if (uniqueTransactions2[i].useItemPrice) uniqueTransactions2[i].mintCost = (int)uniqueTransactions2[i].itemForSale.value;
            }
        }

        for(int i = 0; i < alwaysThere.Count; i++)
        {
            if (alwaysThere[i].itemForSale)
            {
                alwaysThere[i].name = alwaysThere[i].itemForSale.name;
                if (alwaysThere[i].useItemPrice) alwaysThere[i].mintCost = (int)alwaysThere[i].itemForSale.value;
            }
        }

        for(int i = 0; i < seeds.Count; i++)
        {
            if (seeds[i].itemForSale)
            {
                seeds[i].name = seeds[i].itemForSale.name;
                if (seeds[i].useItemPrice) seeds[i].mintCost = (int)seeds[i].itemForSale.value;
            }
        }

        for(int i = 0; i < structures.Count; i++)
        {
            if (structures[i].itemForSale)
            {
                structures[i].name = structures[i].itemForSale.name;
                if (structures[i].useItemPrice) structures[i].mintCost = (int)structures[i].itemForSale.value;
            }
        }

        for(int i = 0; i < furniture.Count; i++)
        {
            if (furniture[i].itemForSale)
            {
                furniture[i].name = furniture[i].itemForSale.name;
                if (furniture[i].useItemPrice) furniture[i].mintCost = (int)furniture[i].itemForSale.value;
            }
        }

        for(int i = 0; i < specialObjs.Count; i++)
        {
            if (specialObjs[i].itemForSale)
            {
                specialObjs[i].name = specialObjs[i].itemForSale.name;
                if (specialObjs[i].useItemPrice) specialObjs[i].mintCost = (int)specialObjs[i].itemForSale.value;
            }
        }

        for (int i = 0; i< trinkets.Count; i++)
        {
            if (trinkets[i].itemForSale)
            {
                trinkets[i].name = trinkets[i].itemForSale.name;
                if (trinkets[i].useItemPrice) trinkets[i].mintCost = (int)trinkets[i].itemForSale.value;
            }
        }
    }

}
