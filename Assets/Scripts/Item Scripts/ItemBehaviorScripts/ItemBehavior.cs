using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemBehavior : ScriptableObject
{
    public virtual void UseItem(out bool consumeItem)
    {
        consumeItem = false;
    }

    public virtual void OnRecieve(InventoryItemData recievedItem)
    {
        //
    }
}
