using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class NightSpawningManager : MonoBehaviour
{
    public static NightSpawningManager Instance;

    float difficultyPoints = 0;
    float highestDifficultyPoints = 0;
    float removedDifficultyPoints = 0; //accumulates when a structure is destroyed by any means

    float difficultyMultiplier = 1; // Multiplies difficulty points of structures
    public DifficultyLevel[] dLevels;
    DifficultyLevel currentDLevel;

    public CreatureObject[] creatures; //list of possible creatures to spawn
    //public CreatureObject[] fillerCreatures; //list of creatures that can spawn when out of danger points

    List<CreatureObject> selectedCreatures = new List<CreatureObject>();//List of creatures selected to spawn this specific night
    List<CreatureObject> selectedFillerCreatures = new List<CreatureObject>();//List of filler creatures selected to spawn this specific night
    
    List<int> spawnedCreaturesThisHour = new List<int>(); //tracks how many of a specific type of creature was spawned this hour //CREATURES NEED TO BE REMOVED WHEN KILLED
    Queue<CreatureObject> creatureQueue = new Queue<CreatureObject>(); //Holds the enemies that are set to spawn but have not spawned yet

    public List<CreatureBehaviorScript> allCreatures; //all creatures spawned by this manager

    public List<Transform> mistSpawns, behindCabinSpawns;
    public Transform[] despawnPositions;

    Dictionary<CreatureObject, int> creatureTallyDict = new Dictionary<CreatureObject, int>();

    List<StructureBehaviorScript> accountedStructures = new List<StructureBehaviorScript>(); //keeps track of the structures counted for wealth points. Clears at day

    public bool boxPlaced, finaleActivated;

    public ParticleSystem finaleMist;

    public CreatureObject pollinator;

    public List<NightEventObject> nightEvents = new List<NightEventObject>();
    bool eventOccured = false; //only 1 per night

    public NightPoolObject currentSpawnPool;

    public PopupScript firstNightWarning;

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
            eventOccured = false;
            currentSpawnPool = null;
            return;
        }

        if(boxPlaced && TimeManager.Instance.currentHour == 20) ActivateFinale();

        if(TimeManager.Instance.currentHour == 20)
        {
            if(TimeManager.Instance.dayNum == 1) PopupHandler.Instance.AddToQueue(firstNightWarning);

            int r = Random.Range(1,4);
            for(int i = 0; i < r; i++) SpawnCreature(pollinator);

            SelectNightPool();
        }
        if(ReportTotalOfCreature(pollinator) < 2 && Random.Range(0,4) == 1) SpawnCreature(pollinator);

        CalculateDifficulty();

        //Call an event;
        if(!eventOccured) TryToStartEvent();

        if(TownGate.Instance.location != PlayerLocation.InWilderness) HourlySpawns();
    }

    void HourlySpawns()
    {
        int totalCreatures = 0;
        for(int i = 0; i < allCreatures.Count; i++)
        {
            if(allCreatures[i].creatureData.contribuiteToCreatureCap) totalCreatures++;
        }

        int maxCreatures = currentDLevel.maxCreatures;

        creatureTallyDict.Clear();
        //Refresh the dictionary for how many of each creature has spawned
        foreach(CreatureObject c in creatures)
        {
            creatureTallyDict.Add(c, 0);
        }

        //Each monster has their weight added to a list
        List<int> weightArray = new List<int>();
        spawnedCreaturesThisHour.Clear();
        for(int i = 0; i < selectedCreatures.Count; i++)
        {
            spawnedCreaturesThisHour.Add(0);
        }


        int w = 0;
        foreach(CreatureObject c in selectedCreatures)
        {
            //If there is more max difficulty points than it's threshold, it has a chance to spawn
            if(c.dangerThreshold <= highestDifficultyPoints);
            {
                for(int s = 0; s < c.spawnWeight; s++) weightArray.Add(w);
            }
            
            foreach(CreatureBehaviorScript cs in allCreatures)
            {
                if(cs.creatureData == c) creatureTallyDict[c]++;//creatureTally[w]++;
            }

            w++;
        }

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
                && attemptedCreature.spawnCap > creatureTallyDict[attemptedCreature] && (totalCreatures < maxCreatures || !attemptedCreature.contribuiteToCreatureCap))
            {
                spawnedCreaturesThisHour[weightArray[r]]++;
                difficultyPoints -= attemptedCreature.dangerCost;
                if(creatureQueue.Count == 0) StartCoroutine(SpawnCreatures());
                creatureQueue.Enqueue(attemptedCreature);
                if(attemptedCreature.spawnType == SpawnType.Support) spawnAttempts += 0.2f; //Support creatures do not contribuite to max spawns this hour as much as non supports do. IE 5 crows = 1 hare spawn
                spawnAttempts++;
                if(attemptedCreature.contribuiteToCreatureCap) totalCreatures++;
                creatureTallyDict[attemptedCreature]++;
            }
            else 
            {
                spawnAttempts += 0.1f;
                //print("Unable to Spawn");
                //if(difficultyPoints <= threshhold) print("Points under threshhold");
            }
            
        }
        while(spawnAttempts < currentDLevel.hourlySpawnAttempts);

        if(totalCreatures < maxCreatures/2 && difficultyPoints < 8)
        {
            r = Random.Range(2,6);
            for(float i = 0; i < r; i++)
            {
                r = Random.Range(0, selectedFillerCreatures.Count);
                CreatureObject newCreature = selectedFillerCreatures[r];

                if(totalCreatures < maxCreatures && newCreature.spawnCap > creatureTallyDict[newCreature]) 
                {
                    if(newCreature.contribuiteToCreatureCap) totalCreatures++;
                    creatureTallyDict[newCreature]++;
                    SpawnCreature(newCreature);

                    if(newCreature.spawnType == SpawnType.Support) i -= 0.4f;
                }
                else i -= 0.9f;
            }
        }
    }

    public void SpawnCreature(CreatureObject c)
    {
        GameObject prefab = null;
        if(c.creatureVariants.Count > 0)
        {
            int r = Random.Range(0, c.creatureVariants.Count);
            int p = Random.Range(0,100);
            //if(c.forceSpawnVariant) p = 0;

            //Code to spawn corrupted variant
            if(c.corruptedPrefab && GameSaveData.Instance.siegesCleared > 1 && Random.Range(0,100) < CorruptionManager.Instance.CorruptedSpawnMod()) prefab = c.corruptedPrefab;

            //New Logic
            if(prefab == null)
            {
                if(c.creatureVariants[r].variantChanceInFarm.Count > 0)
                {
                    float currentChance = 0;
                    foreach (IntWithProbability chance in c.creatureVariants[r].variantChanceInFarm)
                    {
                        if(GameSaveData.Instance.siegesCleared >= chance._int) currentChance = chance._probability; //Make sure they are ordered in the list
                    }
                    if(currentChance > p) prefab = c.creatureVariants[r].prefab;
                }
                else prefab = null; //If the variant list isnt setup
            }

            if(c.creatureVariants[r].wealthPrerequisite > PlayerInteraction.Instance.totalMoneyEarned) prefab = null; //Clear it if the wealth value isnt right
        }
        if(prefab == null) prefab = c.objectPrefab;

        GameObject newCreature; 
        if(c.canSpawnBehindCabin) newCreature = Instantiate(prefab, RandomMistPosition(), Quaternion.identity);
        else newCreature = Instantiate(prefab, RandomMistPositionFrontCabin(), Quaternion.identity);

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
            yield return new WaitForSeconds(Random.Range(3f, 8f));
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
        List<Transform> possibleSpawns = new List<Transform>(mistSpawns);
        possibleSpawns.AddRange(behindCabinSpawns);
        int r = Random.Range(0, possibleSpawns.Count);
        float x = Random.Range(-2, 2);
        return possibleSpawns[r].position + (x * possibleSpawns[r].transform.right); 
        //Debug.Log(mistSpawns, mistSpawns[r]);
        //return mistSpawns, mistSpawns[r].position;
    }

    public Vector3 RandomMistPositionFrontCabin() //Does not include the positions behind the cabin
    {
        List<Transform> possibleSpawns = new List<Transform>(mistSpawns);
        int r = Random.Range(0, possibleSpawns.Count);
        float x = Random.Range(-2, 2);
        return possibleSpawns[r].position + (x * possibleSpawns[r].transform.right); 
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
            ICritter critter = creature as ICritter;
            if (creature != null && creature.gameObject != null && critter == null && !creature.persistAfterNewDay) //Add a check for farm critters as well so they arent deleted
            {
                Destroy(creature.gameObject);
            }
        }
        allCreatures.RemoveAll(item => item == null);
        //allCreatures.Clear();
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
        bool overrideDifficulty = false;
        if(finaleActivated)
        {
            if(difficultyPoints < 100)
            {
                difficultyPoints = 100;
                highestDifficultyPoints = 300;

                if(MainMenuScript.currentFileMode == FileMode.Cozy)
                {
                    difficultyPoints = 50;
                    highestDifficultyPoints = 150;
                }
            }
            overrideDifficulty = true;
            //return;
        }

        if(currentSpawnPool)
        {
            if(currentSpawnPool.forceSetDifficultyPoints > 0) overrideDifficulty = true;
            if(TimeManager.Instance.currentHour == 20)
            {
                difficultyPoints = currentSpawnPool.forceSetDifficultyPoints;
                highestDifficultyPoints = currentSpawnPool.forceSetDifficultyPoints;
            }
        }

        if(!overrideDifficulty)
        {
            if(MainMenuScript.currentFileMode == FileMode.Survival || SiegeManager.Instance.siegeCropOnFarm) difficultyMultiplier = 1;

            else if(GameSaveData.Instance.siegesCleared == 0) difficultyMultiplier = .75f;
            else if(GameSaveData.Instance.siegesCleared == 1) difficultyMultiplier = 1f;
            else if(GameSaveData.Instance.siegesCleared == 2) difficultyMultiplier = 1.25f;
            else if(GameSaveData.Instance.siegesCleared == 3) difficultyMultiplier = 1.50f;
            
            /*if(PlayerInteraction.Instance.totalMoneyEarned > 10000) difficultyMultiplier = 1.6f;
            else if(PlayerInteraction.Instance.totalMoneyEarned > 6000) difficultyMultiplier = 1.4f;
            else if(PlayerInteraction.Instance.totalMoneyEarned > 3000) difficultyMultiplier = 1.25f;
            else if(TimeManager.Instance.dayNum < 3) difficultyMultiplier = 0.75f;
            else difficultyMultiplier = 1;*/ //The old way

            if(MainMenuScript.currentFileMode == FileMode.Cozy) difficultyMultiplier -= 0.25f;

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
        

        currentDLevel = null;
        foreach(DifficultyLevel l in dLevels)
        {
            if(currentDLevel == null || (currentDLevel.difficultyPointThreshold < l.difficultyPointThreshold && highestDifficultyPoints >= l.difficultyPointThreshold))
            {
                currentDLevel = l;
            }
        }

        if(selectedCreatures.Count == 0) SelectCreaturesForNight(); //potentially call this if the current d level increases
    }

    void SelectNightPool()
    {
        currentSpawnPool = null;
        if(SiegeManager.Instance.siegeCropOnFarm)
        {
            currentSpawnPool = SiegeManager.Instance.siegePools[GameSaveData.Instance.siegesCleared];
        }
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

        if(currentSpawnPool)
        {
            selectedCreatures = currentSpawnPool.creatures.ToList();
            selectedFillerCreatures = currentSpawnPool.creatures.ToList();
            return;
        }

        int a = 0; //iterations
        int r = 0; //random
        List<CreatureObject> temp = new List<CreatureObject>();
        //Common creatures to spawn
        //2,5
        a = Random.Range(currentDLevel.c_varietyMin, currentDLevel.c_varietyMax + 1);
        foreach(CreatureObject c in creatures)
        {
            if(c.spawnType == SpawnType.Common && c.CanSpawnThisNight() && !c.excludeFromNormalNights) temp.Add(c);
            //c.forceSpawnVariant = false;
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
        a = Random.Range(currentDLevel.r_varietyMin, currentDLevel.r_varietyMax + 1);
        foreach(CreatureObject c in creatures)
        {
            if(c.spawnType == SpawnType.Rare && c.CanSpawnThisNight() && !c.excludeFromNormalNights) temp.Add(c);
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
        a = Random.Range(currentDLevel.s_varietyMin, currentDLevel.s_varietyMax + 1);
        foreach(CreatureObject c in creatures)
        {
            if(c.spawnType == SpawnType.Support && c.CanSpawnThisNight() && !c.excludeFromNormalNights) temp.Add(c);
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

    void TryToStartEvent()
    {
        for(int i = 0; i < nightEvents.Count; i++)
        {
            if(Random.Range(0, 100f) < nightEvents[i].occurenceChance  && !eventOccured && TimeManager.Instance.dayNum > 1)
            {
                nightEvents[i].InitiateEvent();
                difficultyPoints -= nightEvents[i].difficultyPointsCost;
                eventOccured = true;
                return;
            }
        }
    }

    /*int CalculateMaxCreatures()
    {
        if(finaleActivated) return 8;

        if(highestDifficultyPoints > 350) return 16;
        else if(highestDifficultyPoints > 250) return 12;
        else if(highestDifficultyPoints > 150) return 8;
        else if(highestDifficultyPoints > 50) return 6;
        else return 4;

    }*/

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
        // Set finale to player prefs to be true
        PlayerPrefs.SetInt("FinaleCompleted", 1);

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

    public int maxCreatures = 4;
}

