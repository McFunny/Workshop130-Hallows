using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Creature Object", menuName = "Creature")]
public class CreatureObject : ScriptableObject
{
    public GameObject objectPrefab;
    [HideInInspector] public float health;

    [HideInInspector] public float[] position = new float[3];

    public int dangerCost = 1; //how much wealth does it cost to spawn in
    public int dangerThreshold = 0; //how much wealth does the player need to have in order to spawn it
    public int spawnWeight = 10; //how likely is it to get spawned over another creature
    public int spawnCap = 5; //how many can exist on the field
    public int spawnCapPerHour = 3; //how many can spawn per hour at max

    public Creature data = new Creature();

    public Creature CreateCreature()
    {
        Creature newCreature = new Creature(this);
        return newCreature;
    }
}
[System.Serializable]
public class Creature
{
    [Header("Variables that need to be saved")]
    public string Name;
    public int Id = -1;
    public float health;
    public float[] position = new float[3];


    public Creature()
    {
        Name = "";
        Id = -1;
        position = new float[3];
    }
    public Creature(CreatureObject Creature)
    {
        Name = Creature.name;
        Id = Creature.data.Id;
    }
}
