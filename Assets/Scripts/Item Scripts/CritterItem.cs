using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Critter Item")]
public class CritterItem : InventoryItemData
{
    //public CritterType critterType;
    public CritterTypeDataPair critterRef;
    public PetType petType;
    public bool petOverride = false; //if true, use pettype. otherwise, use crittertype
}
[System.Serializable]
public class CritterTypeDataPair
{
    public CritterType critterType;
    public CreatureObject critterData;
}
