using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightSpawningManager : MonoBehaviour
{
    //Consider using "pools" of enemies. Example: Pool 1, hare, walker, fly. Pool 2, hare, hive, mancer

    public static NightSpawningManager Instance;

    float difficultyPoints = 0;
    float highestDifficultyPoints = 0;
    float removedDifficultyPoints = 0; //accumulates when a structure is destroyed by any means

    float difficultyMultiplier = 1; //Increases to 1.25 after 2000 mints are collected. Multiplies difficulty points of structures
    //float originalDifficultyPoints = 0;

    public CreatureObject[] creatures; //list of possible creatures to spawn
    public CreatureObject[] fillerCreatures; //list of creatures that can spawn when out of danger points
    List<int> spawnedCreaturesThisHour = new List<int>(); //tracks how many of a specific type of creature was spawned this hour //CREATURES NEED TO BE REMOVED WHEN KILLED
    Queue<CreatureObject> creatureQueue = new Queue<CreatureObject>(); //Holds the enemies that are set to spawn but have not spawned yet

    public List<CreatureBehaviorScript> allCreatures; //all creatures in the scene, have a limit to how many there can be in a scene
    //this list saves all current creatures, and all spawned creatures through this/saved by this manager should be assigned to this list

    public List<Transform> testSpawns;
    public Transform[] despawnPositions;

    List<StructureBehaviorScript> accountedStructures = new List<StructureBehaviorScript>(); //keeps track of the structures counted for wealth points. Clears at day

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

    }

    void Start()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;
        //load old danger values
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !TimeManager.Instance.isDay)
        {
            SpawnCreature(creatures[6]);
        }
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

    void HourUpdate()
    {
        if(TimeManager.Instance.timeSkipping) return;
        if(TimeManager.Instance.isDay)
        {
            if(accountedStructures.Count > 0) accountedStructures.Clear();
            removedDifficultyPoints = 0;
            difficultyPoints = 0;
            highestDifficultyPoints = 0;
            return;
        }
        CalculateDifficulty();

        //if(difficultyPoints < 20 && TimeManager.Instance.currentHour == 21) difficultyPoints = 20;
        //difficultyPoints += 1000;
        //difficultyPoints += TimeManager.dayNum;
        //originalDifficultyPoints = difficultyPoints;

        if(TownGate.Instance.location != PlayerLocation.InWilderness) HourlySpawns();
    }

    void HourlySpawns()
    {
        int totalCreatures = 0;
        for(int i = 0; i < allCreatures.Count; i++)
        {
            if(allCreatures[i].creatureData.contribuiteToCreatureCap) totalCreatures++;
        }

        int maxCreatures = CalculateMaxCreatures();

        List<int> creatureTally = new List<int>(); //this list keeps track of the amount of each specific creature
        //Each monster has their weight added to a list
        List<int> weightArray = new List<int>();
        spawnedCreaturesThisHour.Clear();
        for(int i = 0; i < creatures.Length; i++)
        {
            spawnedCreaturesThisHour.Add(0);
            creatureTally.Add(0);
        }


        int w = 0;
        foreach(CreatureObject c in creatures)
        {
            //If there is more max difficulty points than it's threshold, it has a chance to spawn
            if(c.dangerThreshold <= highestDifficultyPoints && c.wealthPrerequisite < PlayerInteraction.Instance.totalMoneyEarned);
            {
                for(int s = 0; s < c.spawnWeight; s++) weightArray.Add(w);
            }
            
            foreach(CreatureBehaviorScript cs in allCreatures)
            {
                if(cs.creatureData == c) creatureTally[w]++;
            }

            w++;
        }

        //try to spawn up to 6 things per hour, with a failed attempt counting for 0.25f tries
        float spawnAttempts = 0;
        int r;
        float threshhold = difficultyPoints * GetThreshold();
        //Each hour only a fraction of the points can be used, to prevent overwhelming spawns
        print("Difficulty points currently is: " + difficultyPoints);
        print("Threshold is: " + threshhold);
        do
        {
            r = Random.Range(0, weightArray.Count);
            CreatureObject attemptedCreature = creatures[weightArray[r]];
            //If there is enough points to afford the creature and it hasnt reached it's spawn cap, spawn it
            if(attemptedCreature.dangerCost <= difficultyPoints && spawnedCreaturesThisHour[weightArray[r]] < attemptedCreature.spawnCapPerHour && difficultyPoints > threshhold
                && attemptedCreature.spawnCap > creatureTally[weightArray[r]] && PlayerInteraction.Instance.totalMoneyEarned >= attemptedCreature.wealthPrerequisite
                && totalCreatures < maxCreatures)
            {
                spawnedCreaturesThisHour[weightArray[r]]++;
                difficultyPoints -= attemptedCreature.dangerCost;
                //SpawnCreature(attemptedCreature); /this is to spawn creatures instantly
                if(creatureQueue.Count == 0) StartCoroutine(SpawnCreatures());
                creatureQueue.Enqueue(attemptedCreature);
                spawnAttempts++;
                totalCreatures++;
                //print("Spawned Creature");
            }
            else 
            {
                spawnAttempts += 0.25f;
                //print("Unable to Spawn");
                //if(difficultyPoints <= threshhold) print("Points under threshhold");
            }
            
        }
        while(spawnAttempts < 6); //add threshhold req too

        if(allCreatures.Count <= 2 && difficultyPoints < 10)
        {
            r = Random.Range(1,4);
            for(int i = 0; i < r; i++)
            {
                r = Random.Range(0, fillerCreatures.Length);
                CreatureObject newCreature = fillerCreatures[r];
                if(newCreature.wealthPrerequisite <= PlayerInteraction.Instance.totalMoneyEarned && totalCreatures < maxCreatures) 
                {
                    totalCreatures++;
                    SpawnCreature(newCreature);
                }
            }
        }
    }

    void SpawnCreature(CreatureObject c)
    {
        //Add chance of spawning variants here
        GameObject prefab = null;
        if(c.creatureVariants.Count > 0)
        {
            int r = Random.Range(0, c.creatureVariants.Count);
            int p = Random.Range(0,100);
            if(c.creatureVariants[r].probabilityInFarm > p && c.creatureVariants[r].wealthPrerequisite <= PlayerInteraction.Instance.totalMoneyEarned) prefab = c.creatureVariants[r].prefab;
        }
        if(prefab == null) prefab = c.objectPrefab;

        GameObject newCreature = Instantiate(prefab, RandomMistPosition(), Quaternion.identity);
        if(newCreature.TryGetComponent<CreatureBehaviorScript>(out var enemy))
        {
            enemy.OnSpawn(); //Why does the mist walker not do this?
            allCreatures.Add(enemy);
        }
    }

    IEnumerator SpawnCreatures()
    {
        yield return new WaitForSeconds(0.5f);
        while(creatureQueue.Count != 0)
        {
            yield return new WaitForSeconds(Random.Range(1f, 4f));
            CreatureObject c = creatureQueue.Dequeue();
            SpawnCreature(c);
        }
    }

    float GetThreshold()
    {
        switch (TimeManager.Instance.currentHour)
            {
                case 1:
                    return 0.4f;
                case 2:
                    return 0.4f;
                case 3:
                    return 0.2f;
                case 4:
                    return 0.1f;
                case 5:
                    return 0;
                case 6:
                    return 0;
                case 20:
                    return 0.9f;
                case 21:
                    return 0.9f;
                case 22:
                    return 0.8f;
                case 23:
                    return 0.7f;
                case 0:
                    return 0.4f;
                default:
                    return 1;
            }
    }

    public Vector3 RandomMistPosition()
    {
        int r = Random.Range(0, testSpawns.Count);
        float x = Random.Range(-2, 2);
        return testSpawns[r].position + (x * testSpawns[r].transform.right); 
        //Debug.Log(testSpawns[r]);
        //return testSpawns[r].position;
    }

    public void GameOver()
    {
        ClearAllCreatures();
    }

    public void ClearAllCreatures()
    {
        CreatureBehaviorScript[] creatures = FindObjectsOfType<CreatureBehaviorScript>();

        foreach (CreatureBehaviorScript creature in creatures)
        {
            if (creature != null && creature.gameObject != null)
            {
                Destroy(creature.gameObject);
            }
        }
        allCreatures.Clear();
    }

    public void RemoveDifficultyPoints(float amount)
    {
        removedDifficultyPoints += amount;
    }

    public void RemoveFromCreatureList(CreatureBehaviorScript creature)
    {
        //
    }

    void CalculateDifficulty()
    {
        if(PlayerInteraction.Instance.totalMoneyEarned > 4000) difficultyMultiplier = 1.5f;
        else if(PlayerInteraction.Instance.totalMoneyEarned > 2000) difficultyMultiplier = 1.25f;
        else difficultyMultiplier = 1;

        foreach(StructureBehaviorScript structure in StructureManager.Instance.allStructs)
        {
            if(accountedStructures.Contains(structure) || structure.wealthValue == 0) continue;
            if(removedDifficultyPoints > 0) //To account for example, a player removing a barrel, to then replace it elsewhere.
            {
                removedDifficultyPoints -= structure.wealthValue * difficultyMultiplier;
                if(removedDifficultyPoints < 0) //removed difficulty points is a negative number
                {
                    difficultyPoints -= removedDifficultyPoints;
                    removedDifficultyPoints = 0;
                }
            }
            else
            {
                difficultyPoints += structure.wealthValue * difficultyMultiplier;
                highestDifficultyPoints += structure.wealthValue * difficultyMultiplier;
            }
            accountedStructures.Add(structure);
        }
    }

    public int ReportTotalOfCreature(CreatureObject creatureType)
    {
        int tally = 0;
        foreach (CreatureBehaviorScript creature in allCreatures)
        {
            if(creature.creatureData == creatureType) tally++;
        }
        return tally;
    }

    int CalculateMaxCreatures()
    {
        if(highestDifficultyPoints > 300) return 15;
        else if(highestDifficultyPoints > 200) return 12;
        else if(highestDifficultyPoints > 150) return 8;
        else if(highestDifficultyPoints > 80) return 6;
        else return 4;
        /*
        switch (TimeManager.Instance.dayNum)
        {
            case 1:
                return 3;
            case 2:
                return 4;
            case 3:
                return 5;
            case 4:
                return 5;
            case 5:
                return 8;
            case 6:
                return 8;
            case 7:
                return 12;
            default:
                //use greater than statements
                return 12;
        }
        */

    }


    /*void ChooseCreatureTypesToSpawn()
    {
        foreach(CreatureVarietyThreshold t in varietyThresholds)
        {
            if(t.moneyThreshold <= PlayerInteraction.Instance.totalMoneyEarned || t.dayThreshold <= TimeManager.Instance.dayNum)
            {
                creatureTypesAllowed = t.typeAmounts;
                break;
            }
        }
        creatureSpawnPool.Clear();

        while(creatureSpawnPool.Count < creatureTypesAllowed)
        {

        }
    } */
}

