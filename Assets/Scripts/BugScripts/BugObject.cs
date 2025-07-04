using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Bug Object", menuName = "Bug")]
public class BugObject : ScriptableObject
{
    public GameObject objectPrefab;

    public int id = -1;

    public int spawnChance = 0; //Out of 100

    public int wealthPrerequisite = 0; //How much money should the player have collected prior to seeing this bug

    public int amountCaught = 0; //If 0, should not appear in codex

    public List<BugSpawnMethod> spawnMethod = new List<BugSpawnMethod>();
    public List<TimeOfDay> activeHours = new List<TimeOfDay>();
    public List<BugSpawnArea> spawnLocations = new List<BugSpawnArea>();

    public List<StructureObject> homeStructures = new List<StructureObject>();
}
public enum BugSpawnMethod
{
    Ground, //Spawn on the ground anywhere over time
    Trees, //Spawns in/near trees over time
    Corpses, //Spawns on top of corpses over time
    Weeds, //Spawns from weeds over time
    BugSpawner //Spawns from a spawner or specified event, such as another creature or structure
}
