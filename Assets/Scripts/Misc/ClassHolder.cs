using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassHolder : MonoBehaviour
{
    //
}
[System.Serializable]
public class ObjectWithProbability
{
    public GameObject _object;
    public float _probability = 0;
}
[System.Serializable]
public class IntWithProbability
{
    public int _int;
    public float _probability = 0;
}
[System.Serializable]
public class ItemWithAmount
{
    public InventoryItemData item;
    public int amount = 1;

    public ItemWithAmount(InventoryItemData _item, int _amount)
    {
        amount = _amount;
        item = _item;
    }
}
