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

    public bool IsCritterHomeless()
    {
        return true;
    }

    public CritterData GetCritterData()
    {
        return new CritterData();
    }

    public CritterBehaviorScript GetCritterScript()
    {
        return null;
    }

    public void LoadData(CritterData c){}

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful);

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item);
    
}

[System.Serializable]
public class CritterData
{
    public int id;
    public int friendshipLevel;
    public float friendPoints;
    public float health;
    public float hunger;
    public float thirst;
    public string name;
    public float extraVar;

    public CritterData()
    {
        id = -1;
        friendshipLevel = 0;
        friendPoints = 0;
        health = 0;
        hunger = 100;
        thirst = 100;
        name = "";
        extraVar = 0;
    }

    public CritterData(int _id, int _friendLevel, float _friendPoints, float _health, float _hunger, float _thirst, string _name, float _extraVar)
    {
        id = _id;
        friendshipLevel = _friendLevel;
        friendPoints = _friendPoints;
        health = _health;
        hunger = _hunger;
        thirst = _thirst;
        name = _name;
        extraVar = _extraVar;
    }
}

