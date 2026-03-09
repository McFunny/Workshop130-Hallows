using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugSpawningManager : MonoBehaviour
{
    public static BugSpawningManager Instance;

    public List<GameObject> allBugs = new List<GameObject>();

    public List<BugSpawnLocation> bugSpawns = new List<BugSpawnLocation>();

    public StructureObject weedData;

    int maxBugs = 20; //Will not spawn any more hourly after this cap

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        TimeManager.OnHourlyUpdate += SpawnHourlyBugs;
        StructureBehaviorScript.OnStructureDestroyed += SpawnBugFromStructure;
        StartCoroutine(DelayedStart());
    }

    void OnDisable()
    {
        TimeManager.OnHourlyUpdate -= SpawnHourlyBugs;
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);
        SpawnHourlyBugs();
    }

    void SpawnHourlyBugs()
    {
        int hourlyBugCap = Random.Range(-3, 6); //Max amount to spawn per hour
        Vector3 spawnPos = Vector3.zero;

        //Standard spawning of hourly bugs that spawn over time in the Farm, Town, Wilderness, ect
        for(int i = 0; i < hourlyBugCap; i++)
        {
            if(allBugs.Count >= maxBugs) continue;
            //spawn hourly bugs
            int r;
            //if(TownGate.Instance.playerLocation == Location.Wilderness) r = GrabSpecificSpot(BugSpawnArea.Wilderness)
            //else
            r = Random.Range(0, bugSpawns.Count);
            spawnPos = bugSpawns[r].transform.position;
            float x = Random.Range(-5, 5);
            float z = Random.Range(-5, 5);
            spawnPos = new Vector3(spawnPos.x + x, spawnPos.y, spawnPos.z + z);

            SpawnBug(spawnPos, BugSpawnMethod.Ground, bugSpawns[r].areaName);
        }

        //function for spawning weed based bugs, higher chance the more weeds
        int totalWeeds = StructureManager.Instance.TallyStructure(weedData);
        if(totalWeeds == 0) hourlyBugCap = 0;
        else
        {
            hourlyBugCap = (int) Mathf.Round(totalWeeds * 0.2f) + 1;
            for(int i = 0; i < hourlyBugCap; i++)
            {
                if(allBugs.Count >= maxBugs) continue;
                if((totalWeeds * .15f) > Random.Range(0, 100))
                {
                    List<GameObject> weeds = StructureManager.Instance.ReturnStructuresOfType(weedData);
                    SpawnBug(weeds[Random.Range(0, weeds.Count)].transform.position, BugSpawnMethod.Weeds, BugSpawnArea.Farm);
                }
            }
        }

        //function for spawning corpse based bugs, 5% per corpse per hour
    }

    //Will need a function to subscribe to structures getting destroyed and spawning bugs (worms for crops+weeds, beetles for rocks, ect)

    public void SpawnBug(Vector3 spawnPos, BugSpawnMethod spawnMethod, BugSpawnArea location)
    {
        List<BugObject> possibleBugs = new List<BugObject>();

        //Sort by time of day available and method and location
        foreach(BugObject bug in BugDatabase.Instance._bugDatabase)
        {
            if(bug.spawnMethod.Contains(spawnMethod) && bug.activeHours.Contains(TimeManager.Instance.timeOfDay) && bug.spawnLocations.Contains(location)
            && PlayerInteraction.Instance.totalMoneyEarned >= bug.wealthPrerequisite) possibleBugs.Add(bug);
        }
        if(possibleBugs.Count == 0) return;

        int iterations = 0;
        GameObject chosenBug = null;
        while(iterations < 10 && !chosenBug)
        {
            int r = Random.Range(0, possibleBugs.Count);
            if(possibleBugs[r].spawnChance > Random.Range(0,100)) chosenBug = possibleBugs[r].objectPrefab;
            iterations++;
        }

        if(chosenBug) allBugs.Add(Instantiate(chosenBug, spawnPos, Quaternion.identity));

        Debug.Log("Spawned a " + chosenBug);
    }

    public void SpawnBug(Vector3 spawnPos, BugObject bugObject) //Spawns a bug via reference
    {
        if(!bugObject.activeHours.Contains(TimeManager.Instance.timeOfDay) || PlayerInteraction.Instance.totalMoneyEarned < bugObject.wealthPrerequisite) return;

        allBugs.Add(Instantiate(bugObject.objectPrefab, spawnPos, Quaternion.identity));
    }

    public void SpawnBugFromStructure(StructureObject structure, Vector3 spawnPos) //Spawns a bug from specified structure
    {
        if(structure.bugSpawnChance < Random.Range(0, 100)) return;

        List<BugObject> possibleBugs = new List<BugObject>();

        //Sort by time of day available and method and location
        foreach(BugObject bug in BugDatabase.Instance._bugDatabase)
        {
            if(bug.activeHours.Contains(TimeManager.Instance.timeOfDay) && PlayerInteraction.Instance.totalMoneyEarned >= bug.wealthPrerequisite
            && bug.homeStructures.Contains(structure)) possibleBugs.Add(bug);
        }
        if(possibleBugs.Count == 0) return;

        int iterations = 0;
        GameObject chosenBug = null;
        while(iterations < 20 && !chosenBug)
        {
            int r = Random.Range(0, possibleBugs.Count);
            if(possibleBugs[r].spawnChance > Random.Range(0,100)) chosenBug = possibleBugs[r].objectPrefab;
            iterations++;
        }

        if(chosenBug) allBugs.Add(Instantiate(chosenBug, spawnPos, Quaternion.identity));

        Debug.Log("Spawned a " + chosenBug);
    }

    public void SpawnCorpseBug(Vector3 pos)
    {
        List<BugObject> possibleBugs = new List<BugObject>();

        //Sort by time of day available and method and location
        foreach(BugObject bug in BugDatabase.Instance._bugDatabase)
        {
            if(bug.spawnMethod.Contains(BugSpawnMethod.Corpses) && PlayerInteraction.Instance.totalMoneyEarned >= bug.wealthPrerequisite) possibleBugs.Add(bug);
        }
        if(possibleBugs.Count == 0) return;

        int iterations = 0;
        GameObject chosenBug = null;
        while(iterations < 5 && !chosenBug)
        {
            int r = Random.Range(0, possibleBugs.Count);
            if(possibleBugs[r].spawnChance > Random.Range(0,100)) chosenBug = possibleBugs[r].objectPrefab;
            iterations++;
        }

        if(chosenBug) allBugs.Add(Instantiate(chosenBug, pos, Quaternion.identity));
    }

    int GrabSpecificSpot(BugSpawnArea location)
    {
        List<int> potentialLocations = new List<int>();
        for(int i = 0; i < bugSpawns.Count; i++)
        {
            if(bugSpawns[i].areaName == location) potentialLocations.Add(i);
        }

        if(potentialLocations.Count == 0) return 0;

        return potentialLocations[Random.Range(0, potentialLocations.Count)];
    }
}
