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

    public BugSpawnMethod spawnMethod;
}
public enum BugSpawnMethod
{
    Ground, //Spawn on the ground anywhere
    Trees, //Spawns in/near trees
    Corpses, //Spawns on top of corpses
    Weeds, //Spawns from weeds
    StructureDestruction, //Spawns from structure getting destroyed, such as a rock or a plant
    BugSpawner //Spawns from a spawner or specified event, such as another creature or structure
}
