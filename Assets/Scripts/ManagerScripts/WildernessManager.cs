using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessManager : MonoBehaviour
{
    public static WildernessManager Instance;

    int hoursSpentInWilderness = 0;
    
    int maxCreatures = 5;

    public List<CreatureBehaviorScript> allCreatures;

    public CreatureObject[] creatures;
    public GameObject[] interactablePrefabs;
    public float[] interactableSpawnChances;
    public GameObject[] setPiecePrefabs;

    [HideInInspector] public List<WildernessMap> allMaps = new List<WildernessMap>();
    WildernessMap currentMap;

    [HideInInspector] public WildernessMerchant wagon;

    public Transform returnPosition;

    public bool visitedWilderness = false; //marked true when leaving, cannot return until next day

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourUpdate;
    }

    void Start()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    void Update()
    {
        
    }

    public void EnterWilderness()
    {
        if(allMaps.Count == 0)
        {
            Debug.LogError("There are no available maps. Are they not active in your scene?");
            return;
        }

        TownGate.Instance.Transition(PlayerLocation.InWilderness);
        
        currentMap = allMaps[Random.Range(0, allMaps.Count)];

        int r = Random.Range(0,currentMap.spawnPositions.Length);
        PlayerInteraction.Instance.transform.position = currentMap.spawnPositions[r].position;
        wagon.transform.position = currentMap.wagonPositions[r].position;
        wagon.transform.LookAt(PlayerInteraction.Instance.transform.position);


        currentMap.InitializeMap();
        hoursSpentInWilderness++;
        StartCoroutine(CreatureSpawn());
    }

    public void ExitWilderness()
    {
        if(TownGate.Instance.location == PlayerLocation.InWilderness) TownGate.Instance.Transition(PlayerLocation.InTown);
        PlayerInteraction.Instance.transform.position = returnPosition.position;
        ClearCreatures();
        currentMap.ClearMap();
        currentMap = null;
        hoursSpentInWilderness = 0;
        visitedWilderness = true;
    }

    void HourUpdate()
    {
        if(currentMap == null)
        {
            return;
        }

        if(!TimeManager.Instance.isDay && currentMap)
        {
            //Play the force cutscene back to the town
            wagon.StartCoroutine(wagon.PlayerTooLate());
            return;
        }
        
        hoursSpentInWilderness++;
        CalculateDifficulty();
    }

    IEnumerator CreatureSpawn()
    {
        //If the cap is reached (or randomly), pick a random monster that is far from the player and teleport them elsewhere
        while(currentMap)
        {
            float t = Random.Range(5, 15);
            yield return new WaitForSeconds(t);
            if(allCreatures.Count < maxCreatures && currentMap && DialogueController.Instance.IsTalking())
            {
                int r = Random.Range(0, creatures.Length);
                CreatureObject newCreature = creatures[r];
                SpawnCreature(newCreature);
            }
        }
    }

    void SpawnCreature(CreatureObject c)
    {
        //Add chance of spawning variants here
        //Only spawn wilderness variants here
        GameObject prefab = null;
        int t = 0;
        while(c.creatureVariants.Count > 0 && t < 5 && prefab == null)
        {
            int r = Random.Range(0, c.creatureVariants.Count);
            int p = Random.Range(0,100);
            if(c.creatureVariants[r].probabilityInWilderness > p) prefab = c.creatureVariants[r].prefab;
            t++;
        }
        if(prefab == null) prefab = c.objectPrefab;

        GameObject newCreature = Instantiate(prefab, RandomSpawnPosition(), Quaternion.identity);
        if(newCreature.TryGetComponent<CreatureBehaviorScript>(out var enemy))
        {
            enemy.inWilderness = true;
            enemy.OnSpawn();
            allCreatures.Add(enemy);
        }
    }

    public void ClearCreatures()
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

    Vector3 RandomSpawnPosition()
    {
        int r;
        Vector3 closestPos = new Vector3 (0,0,0);
        float minDistance = 1000;
        float dist;
        for(int i = 0; i < 4; i++)
        {
            r = Random.Range(0, currentMap.enemySpawnPositions.Length);
            dist = Vector3.Distance(PlayerInteraction.Instance.transform.position, currentMap.enemySpawnPositions[r].position);
            if(dist < minDistance)
            {
                closestPos = currentMap.enemySpawnPositions[r].position;
                minDistance = dist;
            }
        }
        return closestPos; 
    }

    void CalculateDifficulty()
    {
        if(hoursSpentInWilderness > 6) maxCreatures = 16;
        else if(hoursSpentInWilderness > 4) maxCreatures = 12;
        else if(hoursSpentInWilderness > 2) maxCreatures = 8;
        else maxCreatures = 6;
    }
}
