using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICritter
{
    public float GetCritterHealth()
    {
        return -1;
    }

    public float GetCritterHunger()
    {
        return -1;
    }

    public float GetCritterThirst()
    {
        return -1;
    }

    public string GetCritterName()
    {
        return "";
    }

    public int GetCritterID()
    {
        return -1;
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful);

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item);
    
}
