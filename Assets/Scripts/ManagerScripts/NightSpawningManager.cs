using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightSpawningManager : MonoBehaviour
{
    float difficultyPoints = 0;
    //float originalDifficultyPoints = 0;

    public CreatureObject[] creatures;
    List<int> spawnedCreatures = new List<int>(); //tracks how many of a specific type of creature was spawned this hour

    public List<CreatureBehaviorScript> allCreatures; //all creatures in the scene, have a limit to how many there can be in a scene
    //this list saves all current creatures, and all spawned creatures through this/saved by this manager should be assigned to this list

    public Transform[] testSpawns;

    void Start()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;
        //load old danger values
    }

    void HourUpdate()
    {
        if(TimeManager.isDay)
        {
            return;
        }
        if(TimeManager.currentHour == 20)
        {
            foreach(StructureBehaviorScript structure in StructureManager.Instance.allStructs)
            {
               difficultyPoints += structure.wealthValue;
            }
            //difficultyPoints += TimeManager.dayNum;
            //originalDifficultyPoints = difficultyPoints;
        }
        if(difficultyPoints > 0) HourlySpawns();
    }

    void HourlySpawns()
    {
        List<int> creatureTally = new List<int>(); //this list keeps track of the amount of each specific creature
        //Each monster has their weight added to a list
        List<int> weightArray = new List<int>();
        spawnedCreatures.Clear();
        for(int i = 0; i < creatures.Length; i++)
        {
            spawnedCreatures.Add(0);
            creatureTally.Add(0);
        }


        int w = 0;
        foreach(CreatureObject c in creatures)
        {
            //If there is more difficulty points than it's threshold, it has a chance to spawn
            if(c.dangerThreshold <= difficultyPoints)
            {
                for(int s = 0; s < c.spawnWeight; s++) weightArray.Add(w);
            }
            w++;
        }

        //try to spawn up to 10 things per hour, with a failed attempt counting for 0.25f tries
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
            if(attemptedCreature.dangerCost <= difficultyPoints && spawnedCreatures[weightArray[r]] < attemptedCreature.spawnCapPerHour && difficultyPoints > threshhold)
            {
                spawnedCreatures[weightArray[r]]++;
                difficultyPoints -= attemptedCreature.dangerCost;
                SpawnCreature(attemptedCreature);
                spawnAttempts++;
                print("Spawned Creature");
            }
            else 
            {
                spawnAttempts += 0.25f;
                print("Unable to Spawn");
            }
            
        }
        while(spawnAttempts < 10); //add threshhold req too
    }

    void SpawnCreature(CreatureObject c)
    {
        int t = Random.Range(0,testSpawns.Length);
        GameObject newCreature = Instantiate(c.objectPrefab, testSpawns[t].position, Quaternion.identity);
        if(newCreature.TryGetComponent<CreatureBehaviorScript>(out var enemy))
        {
            enemy.OnSpawn();
        }
    }

    float GetThreshold()
    {
        switch (TimeManager.currentHour)
            {
                case 1:
                    return 0.2f;
                case 2:
                    return 0.2f;
                case 3:
                    return 0;
                case 4:
                    return 0;
                case 5:
                    return 0;
                case 6:
                    return 0;
                case 20:
                    return 0.9f;
                case 21:
                    return 0.7f;
                case 22:
                    return 0.7f;;
                case 23:
                    return 0.4f;
                case 0:
                    return 0.4f;
                default:
                    return 1;
            }
    }
}
