using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Creature Object", menuName = "Creature")]
public class CreatureObject : ScriptableObject
{
    public GameObject objectPrefab;//, wildernessPrefab;
    public List<CreatureVariant> creatureVariants = new List<CreatureVariant>();
    [HideInInspector] public float health;

    [HideInInspector] public float[] position = new float[3];

    public int id = -1;

    public float dangerCost = 1; //how much wealth does it cost to spawn in
    public int dangerThreshold = 0; //how much wealth does the player need to have in order to spawn it
    public int spawnWeight = 10; //how likely is it to get spawned over another creature
    public int spawnCap = 5; //how many can exist on the field
    public int spawnCapPerHour = 3; //how many can spawn per hour at max

    public int wealthPrerequisite = 0; //How much money should the player have collected prior to seeing this creature

    public bool contribuiteToCreatureCap = true; //EX Crows shouldnt contribuite to max amount of creatures loaded in. Instead use their spawn cap

    public int amountKilled = 0;

    public bool hasSpawned = false;
    public bool excludeFromNormalNights = false; //If true, the creature will not be picked to be part of the regular nighttime enemies
    public bool canSpawnBehindCabin = true; //If false, enemies cannot spawn at the mist section behind the cabin

    //public CreatureSpawnVariance spawnVariance; //Dictates how many of this unit will spawn

    //[HideInInspector] public bool forceSpawnVariant = false;

    public SpawnType spawnType;

    //////For Wilderness Spawning/////
    /// 
    public float spawnChance_w = 100;

    public int mintWorth = 1; //Used for quests

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

[System.Serializable]
public class CreatureVariant
{
    public string name;
    public GameObject prefab;
    //public float probabilityInFarm = 100; //Probability of spawning. Obsolete if using the List below
    public float probabilityInWilderness = 100;
    public bool canSpawnInWilderness;
    public int wealthPrerequisite = 0;

    public List<IntWithProbability> variantChanceInFarm = new List<IntWithProbability>(); //int is the siege num, float is the chance of spawning
}
public enum SpawnType
{
    Common, //Grunt enemies
    Rare, //More dynamic and gameplay changing
    Support //Less impactful or optional creature
}

[System.Serializable]
public class CreatureSpawnVariance
{
    public int amountToSpawn;
    public int variance;
}
