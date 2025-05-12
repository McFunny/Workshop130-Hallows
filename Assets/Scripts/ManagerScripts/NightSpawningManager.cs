using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class NightSpawningManager : MonoBehaviour
{
    //Consider using "pools" of enemies. Example: Pool 1, hare, walker, fly. Pool 2, hare, hive, mancer

    public static NightSpawningManager Instance;

    float difficultyPoints = 0;
    float highestDifficultyPoints = 0;
    float removedDifficultyPoints = 0; //accumulates when a structure is destroyed by any means

    float difficultyMultiplier = 1; //Increases to 1.25 after 2000 mints are collected. Multiplies difficulty points of structures
    public DifficultyLevel[] dLevels;
    DifficultyLevel currentDLevel;

    public CreatureObject[] creatures; //list of possible creatures to spawn
    public CreatureObject[] fillerCreatures; //list of creatures that can spawn when out of danger points

    List<CreatureObject> selectedCreatures = new List<CreatureObject>();//List of creatures selected to spawn this specific night
    List<CreatureObject> selectedFillerCreatures = new List<CreatureObject>();//List of filler creatures selected to spawn this specific night
    
    List<int> spawnedCreaturesThisHour = new List<int>(); //tracks how many of a specific type of creature was spawned this hour //CREATURES NEED TO BE REMOVED WHEN KILLED
    Queue<CreatureObject> creatureQueue = new Queue<CreatureObject>(); //Holds the enemies that are set to spawn but have not spawned yet

    public List<CreatureBehaviorScript> allCreatures; //all creatures in the scene
    //this list saves all current creatures, and all spawned creatures through this/saved by this manager should be assigned to this list

    public List<Transform> testSpawns;
    public Transform[] despawnPositions;

    Dictionary<CreatureObject, int> creatureTallyDict = new Dictionary<CreatureObject, int>();

    List<StructureBehaviorScript> accountedStructures = new List<StructureBehaviorScript>(); //keeps track of the structures counted for wealth points. Clears at day

    public bool boxPlaced, finaleActivated;

    public ParticleSystem finaleMist;

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

    }

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.I) && !TimeManager.Instance.isDay)
        {
            SpawnCreature(creatures[6]);
        }
        if (Input.GetKeyDown(KeyCode.O) && !TimeManager.Instance.isDay)
        {
            SpawnCreature(creatures[7]);
        }
        if (Input.GetKeyDown(KeyCode.P) && !TimeManager.Instance.isDay)
        {
            SpawnCreature(creatures[0]);
        }*/
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
            selectedCreatures.Clear();
            return;
        }

        if(boxPlaced && TimeManager.Instance.currentHour == 20) ActivateFinale();

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

        creatureTallyDict.Clear();
        //Refresh the dictionary for creature spawns
        foreach(CreatureObject c in creatures)
        {
            creatureTallyDict.Add(c, 0);
        }

        //List<int> creatureTally = new List<int>(); //this list keeps track of the amount of each specific creature
        //Each monster has their weight added to a list
        List<int> weightArray = new List<int>();
        spawnedCreaturesThisHour.Clear();
        for(int i = 0; i < selectedCreatures.Count; i++)
        {
            spawnedCreaturesThisHour.Add(0);
            //creatureTally.Add(0);
        }


        int w = 0;
        foreach(CreatureObject c in selectedCreatures)
        {
            //If there is more max difficulty points than it's threshold, it has a chance to spawn
            if(c.dangerThreshold <= highestDifficultyPoints && c.wealthPrerequisite <= PlayerInteraction.Instance.totalMoneyEarned);
            {
                for(int s = 0; s < c.spawnWeight; s++) weightArray.Add(w);
            }
            
            foreach(CreatureBehaviorScript cs in allCreatures)
            {
                if(cs.creatureData == c) creatureTallyDict[c]++;//creatureTally[w]++;
            }

            w++;
        }

        //try to spawn up to 6 things per hour, with a failed attempt counting for 0.5f tries
        float spawnAttempts = 0;
        int r;
        float threshhold = difficultyPoints * GetThreshold();
        //Each hour only a fraction of the points can be used, to prevent overwhelming spawns
        print("Difficulty points currently is: " + difficultyPoints);
        print("Threshold is: " + threshhold);
        do
        {
            r = Random.Range(0, weightArray.Count);
            CreatureObject attemptedCreature = selectedCreatures[weightArray[r]];
            //If there is enough points to afford the creature and it hasnt reached it's spawn cap, spawn it
            if(attemptedCreature.dangerCost <= difficultyPoints && spawnedCreaturesThisHour[weightArray[r]] < attemptedCreature.spawnCapPerHour && difficultyPoints > threshhold
                && attemptedCreature.spawnCap > creatureTallyDict[attemptedCreature] && PlayerInteraction.Instance.totalMoneyEarned >= attemptedCreature.wealthPrerequisite
                && totalCreatures < maxCreatures)
            {
                spawnedCreaturesThisHour[weightArray[r]]++;
                difficultyPoints -= attemptedCreature.dangerCost;
                //SpawnCreature(attemptedCreature); //this is to spawn creatures instantly
                if(creatureQueue.Count == 0) StartCoroutine(SpawnCreatures());
                creatureQueue.Enqueue(attemptedCreature);
                spawnAttempts++;
                totalCreatures++;
                //print("Spawned Creature");
            }
            else 
            {
                spawnAttempts += 0.5f;
                //print("Unable to Spawn");
                //if(difficultyPoints <= threshhold) print("Points under threshhold");
            }
            
        }
        while(spawnAttempts < currentDLevel.hourlySpawnAttempts);

        if(allCreatures.Count < maxCreatures && difficultyPoints < 6)
        {
            r = Random.Range(1,3);
            for(int i = 0; i < r; i++)
            {
                r = Random.Range(0, selectedFillerCreatures.Count);
                CreatureObject newCreature = selectedFillerCreatures[r];

                if(newCreature.wealthPrerequisite <= PlayerInteraction.Instance.totalMoneyEarned && totalCreatures < maxCreatures && newCreature.spawnCap > creatureTallyDict[newCreature]) 
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
        creatureTallyDict[c]++;

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
            enemy.OnSpawn(); 
            allCreatures.Add(enemy);
            if(enemy.creatureData) enemy.creatureData.hasSpawned = true;
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
        CreatureBehaviorScript[] creaturesOnFarm = FindObjectsOfType<CreatureBehaviorScript>();

        foreach (CreatureBehaviorScript creature in creaturesOnFarm)
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

        if(finaleActivated)
        {
            if(difficultyPoints < 100)
            {
                difficultyPoints = 100;
                highestDifficultyPoints = 300;
            }
            return;
        }

        if(PlayerInteraction.Instance.totalMoneyEarned > 5000) difficultyMultiplier = 1.5f;
        else if(PlayerInteraction.Instance.totalMoneyEarned > 3000) difficultyMultiplier = 1.25f;
        else if(TimeManager.Instance.dayNum == 1) difficultyMultiplier = 0.75f;
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

        currentDLevel = null;
        foreach(DifficultyLevel l in dLevels)
        {
            if(currentDLevel == null || (currentDLevel.difficultyPointThreshold < l.difficultyPointThreshold && highestDifficultyPoints >= l.difficultyPointThreshold))
            {
                currentDLevel = l;
            }
        }

        if(selectedCreatures.Count == 0) SelectCreaturesForNight();
    }

    [ContextMenu("RefreshNightCreatures")]
    void SelectCreaturesForNight()
    {
        selectedCreatures.Clear();
        selectedFillerCreatures.Clear();

        if(finaleActivated)
        {
            selectedCreatures = creatures.ToList();
            selectedFillerCreatures = creatures.ToList();
            return;
        }

        int a = 0; //iterations
        int r = 0; //random
        List<CreatureObject> temp = new List<CreatureObject>();
        //Common creatures to spawn
        //2,5
        a = Random.Range(currentDLevel.c_varietyMin, currentDLevel.c_varietyMin);
        foreach(CreatureObject c in creatures)
        {
            if(c.spawnType == SpawnType.Common && c.wealthPrerequisite <= PlayerInteraction.Instance.totalMoneyEarned) temp.Add(c);
        }
        for(int i = 0; i < a; i++)
        {
            if(temp.Count == 0) continue;
            r = Random.Range(0, temp.Count);
            selectedCreatures.Add(temp[r]);
            selectedFillerCreatures.Add(temp[r]);
            temp.Remove(temp[r]);
        }

        temp.Clear();

        //Rare creatures to spawn
        //1,5
        a = Random.Range(currentDLevel.r_varietyMin, currentDLevel.r_varietyMin);
        foreach(CreatureObject c in creatures)
        {
            if(c.spawnType == SpawnType.Rare && c.wealthPrerequisite <= PlayerInteraction.Instance.totalMoneyEarned) temp.Add(c);
        }
        for(int i = 0; i < a; i++)
        {
            if(temp.Count == 0) continue;
            r = Random.Range(0, temp.Count);
            selectedCreatures.Add(temp[r]);
            temp.Remove(temp[r]);
        }

        temp.Clear();

        //Support creatures to spawn
        //0,4
        a = Random.Range(currentDLevel.s_varietyMin, currentDLevel.s_varietyMin);
        foreach(CreatureObject c in creatures)
        {
            if(c.spawnType == SpawnType.Support && c.wealthPrerequisite <= PlayerInteraction.Instance.totalMoneyEarned) temp.Add(c);
        }
        for(int i = 0; i < a; i++)
        {
            if(temp.Count == 0) continue;
            r = Random.Range(0, temp.Count);
            selectedCreatures.Add(temp[r]);
            selectedFillerCreatures.Add(temp[r]);
            temp.Remove(temp[r]);
        }

        temp.Clear();
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
        if(finaleActivated) return 8;

        if(highestDifficultyPoints > 350) return 16;
        else if(highestDifficultyPoints > 250) return 12;
        else if(highestDifficultyPoints > 150) return 8;
        else if(highestDifficultyPoints > 50) return 6;
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

    void ActivateFinale()
    {
        finaleActivated = true;
        boxPlaced = false;
        
        //AmbientAudioManager.Instance.ChangeMusic();
        AmbientAudioManager.Instance.StartFinaleTheme();
        if(finaleMist) finaleMist.Play();
    }

    public void DeactivateFinale()
    {
        finaleActivated = false;
        difficultyPoints = 0;
        highestDifficultyPoints = 0;
        
        //AmbientAudioManager.Instance.ChangeMusic();
        AmbientAudioManager.Instance.EndFinaleTheme();
        if(finaleMist) finaleMist.Stop();
    }

    public void FinaleComplete()
    {
        AmbientAudioManager.Instance.WinFinaleTheme();
        StartCoroutine(GameCompleted());
        /*for(int i = 0; i < allCreatures.Count; i++)
        {
            allCreatures[i].TakeDamage(999);
        }*/

        CreatureBehaviorScript[] creaturesOnFarm = FindObjectsOfType<CreatureBehaviorScript>();

        foreach (CreatureBehaviorScript creature in creaturesOnFarm)
        {
            if (creature != null && creature.gameObject != null)
            {
                creature.TakeDamage(999);
            }
        }
    }

    IEnumerator GameCompleted()
    {
        TimeManager.Instance.stopTime = true;
        PlayerInteraction.Instance.invincible = true;
        yield return new WaitForSeconds(5);
        FadeScreen.coverScreen = true;
        PlayerMovement.restrictMovementTokens++;
        //AmbientAudioManager.Instance.FadeMusic();
        yield return new WaitForSeconds(10);
        //Credits screen
        SceneManager.LoadSceneAsync(2);
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

//Not incorporated yet. Can be used to track things like spawns per hour and the creature density. Will need a function comparing the thresholds of levels to determine which one is active
[System.Serializable]
public class DifficultyLevel
{
    public float difficultyPointThreshold; //how much difficulty points are required to reach this level
    public int c_varietyMin, c_varietyMax; //min and max of common spawns
    public int r_varietyMin, r_varietyMax; //min and max of rare spawns
    public int s_varietyMin, s_varietyMax; //min and max of support spawns

    public int hourlySpawnAttempts = 5;
}

