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
    //public GameObject[] interactablePrefabs;
    public WildernessInteractable[] wildernessInteractables;
    //public float[] interactableSpawnChances;

    [HideInInspector] public List<WildernessMap> allMaps = new List<WildernessMap>();
    WildernessMap currentMap;

    [HideInInspector] public WildernessMerchant wagon;
    public Transform playerWagon; //Must manually assign, sigh

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

        if(!playerWagon) Debug.LogError("Player wagon variable needs to be set in the inspector!!! Take the transform of the player wagon under the Wilderness GameObject");

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
        AmbientAudioManager.Instance.ChangeMusic();
        
        currentMap = allMaps[Random.Range(0, allMaps.Count)];

        currentMap.mapObject.SetActive(true);

        int r = Random.Range(0,currentMap.spawnPositions.Length);
        PlayerInteraction.Instance.transform.position = currentMap.spawnPositions[r].position;
        playerWagon.transform.position = currentMap.wagonPositions[r].position;
        playerWagon.transform.rotation = currentMap.wagonPositions[r].rotation;
        //playerWagon.transform.LookAt(PlayerInteraction.Instance.transform.position);


        currentMap.InitializeMap();
        hoursSpentInWilderness++;
        StartCoroutine(CreatureSpawn());
    }

    public void ExitWilderness()
    {
        if(TownGate.Instance.location == PlayerLocation.InWilderness) TownGate.Instance.Transition(PlayerLocation.InTown);
        AmbientAudioManager.Instance.ChangeMusic();
        PlayerInteraction.Instance.transform.position = returnPosition.position;
        ClearCreatures();
        currentMap.ClearMap();
        currentMap.mapObject.SetActive(false);
        currentMap = null;
        hoursSpentInWilderness = 0;
        visitedWilderness = true;
    }

    public void GameOver()
    {
        if(TownGate.Instance.location == PlayerLocation.InWilderness) TownGate.Instance.Transition(PlayerLocation.InFarm);
        if(currentMap == null) return;
        PlayerInteraction.Instance.transform.position = returnPosition.position; //Maybe this can make the fix
        AmbientAudioManager.Instance.ChangeMusic();
        ClearCreatures();
        currentMap.ClearMap();
        currentMap.mapObject.SetActive(false);
        currentMap = null;
        hoursSpentInWilderness = 0;
        visitedWilderness = false;
    }

    void HourUpdate()
    {
        if(currentMap == null)
        {
            return;
        }

        if(TimeManager.Instance.currentHour == 18 && currentMap)
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
        bool skipTimer = false;
        float t = 0;
        while(currentMap)
        {
            //print("Ran");
            if(skipTimer) t = 1f;
            else t = Random.Range(10, 20);
            yield return new WaitForSeconds(t);
            if(allCreatures.Count < maxCreatures && currentMap && !DialogueController.Instance.IsTalking())
            {
                //print("Spawned");
                int r = Random.Range(0, creatures.Length);
                CreatureObject newCreature = creatures[r];
                if(newCreature.spawnChance_w > Random.Range(0,100))
                {
                    SpawnCreature(newCreature);
                    if(allCreatures.Count < maxCreatures/2 && Random.Range(0,100) > 50) SpawnCreature(newCreature);

                    skipTimer = false;
                }
                else skipTimer = true;
            }
        }
    }

    void SpawnCreature(CreatureObject c)
    {
        //Add chance of spawning variants here
        //Only spawn wilderness variants here
        GameObject prefab = null;
        int t = 0;
        while(c.creatureVariants.Count > 0 && t < c.creatureVariants.Count && prefab == null) //This method makes the first in the variant list more likely to be chosen
        {
            int p = Random.Range(0,100);
            if(c.creatureVariants[t].probabilityInWilderness > p) prefab = c.creatureVariants[t].prefab;
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
            ICritter critter = creature as ICritter;
            if (creature != null && creature.gameObject != null && critter == null)
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
        for(int i = 0; i < 2; i++)
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
        if(hoursSpentInWilderness > 6) maxCreatures = 25;
        else if(hoursSpentInWilderness > 4) maxCreatures = 20;
        else if(hoursSpentInWilderness > 2) maxCreatures = 15;
        else maxCreatures = 10;
    }
}
[System.Serializable]
public class WildernessInteractable
{
    public string name;
    public GameObject prefab;
    public bool isLarge = false;
    public float spawnChance = 100;
}
