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
    public GameObject[] interactables;
    public GameObject[] setPieces;

    public WildernessMap[] allMaps;
    WildernessMap currentMap;

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

    // Start is called before the first frame update
    void Start()
    {
        TimeManager.OnHourlyUpdate += HourUpdate;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnterWilderness()
    {
        currentMap = allMaps[Random.Range(0, allMaps.Length)];
        PlayerInteraction.Instance.transform.position = currentMap.spawnPositions[Random.Range(0,currentMap.spawnPositions.Length)].position;
        hoursSpentInWilderness++;
        CreatureSpawn();
    }

    public void ExitWilderness()
    {
        PlayerInteraction.Instance.transform.position = returnPosition.position;
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

        if(!TimeManager.Instance.isDay)
        {
            //Play the force cutscene back to the town
            return;
        }
        
        hoursSpentInWilderness++;
        CalculateDifficulty();
    }

    IEnumerator CreatureSpawn()
    {
        while(hoursSpentInWilderness > 0)
        {
            float t = Random.Range(5, 20);
            yield return new WaitForSeconds(t);
            if(allCreatures.Count < maxCreatures)
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

        GameObject newCreature = Instantiate(c.objectPrefab, RandomSpawnPosition(), Quaternion.identity);
        if(newCreature.TryGetComponent<CreatureBehaviorScript>(out var enemy))
        {
            enemy.OnSpawn();
            allCreatures.Add(enemy);
        }
    }

    //probably higher after 2 days ago

    Vector3 RandomSpawnPosition()
    {
        int r;
        List<Vector3> possiblePos = new List<Vector3>();
        for(int i = 0; i < 4; i++)
        {
            r = Random.Range(0, currentMap.enemySpawnPositions.Length);
            if(!possiblePos.Contains(currentMap.enemySpawnPositions[r].position)) possiblePos.Add(currentMap.enemySpawnPositions[r].position);
        }

        int x = Random.Range(0, possiblePos.Count);
        return possiblePos[x]; 
    }

    void CalculateDifficulty()
    {
        if(hoursSpentInWilderness > 6) maxCreatures = 16;
        else if(hoursSpentInWilderness > 4) maxCreatures = 12;
        else if(hoursSpentInWilderness > 2) maxCreatures = 8;
        else maxCreatures = 4;
    }
}

public class WildernessMap
{
    public Transform[] spawnPositions; //Possible player spawns
    public Transform[] enemySpawnPositions; //spots enemies can spawn from. Should grab the closest 2 from the player
    public Transform[] setPiecePositions; //Locations that the giant setpieces can take
    public Transform[] interactablePositions; //Locations of small things like trees with nuts, hives, and foreagables can spawn near
}
