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

    public CritterData GetCritterData()
    {
        return new CritterData();
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

    public CritterData()
    {
        id = -1;
        friendshipLevel = 0;
        friendPoints = 0;
        health = 0;
        hunger = 100;
        thirst = 100;
        name = "";
    }

    public CritterData(int _id, int _friendLevel, float _friendPoints, float _health, float _hunger, float _thirst, string _name)
    {
        id = _id;
        friendshipLevel = _friendLevel;
        friendPoints = _friendPoints;
        health = _health;
        hunger = _hunger;
        thirst = _thirst;
        name = _name;
    }
}

