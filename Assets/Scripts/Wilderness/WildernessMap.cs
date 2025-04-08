using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessMap : MonoBehaviour
{
    public GameObject mapObject;

    public Transform[] spawnPositions; //Possible player spawns
    public Transform[] wagonPositions; //Associated wagon spawns
    public Transform[] enemySpawnPositions; //Spots enemies can spawn from. Should grab the closest 2 from the player
    public Transform[] setPiecePositions; //Locations that the giant setpieces can take
    public ObjectWithProbability[] setPiecePrefabs;
    public WildernessInteractableSpot[] interactablePositions; //Locations of small things like trees with nuts, hives, and foreagables can spawn near
    public GameObject[] obstacles; //Locations that block paths. Must be enabled or disabled

    public GameObject forageablePrefab;//to make sure it no spawn new one

    List<GameObject> currentInteractables = new List<GameObject>();
    List<GameObject> currentSetPieces = new List<GameObject>();

    void Start()
    {
        if(!WildernessManager.Instance.allMaps.Contains(this))
        {
            WildernessManager.Instance.allMaps.Add(this);
            for(int i = 0; i < obstacles.Length; i++)
            {
                obstacles[i].SetActive(false);
            }
            mapObject.SetActive(false);
        }
    }

    public void InitializeMap()
    {
        int r; //random number
        int t = Random.Range(1, 5); //random number of interations
        for(int i = 0; i < t; i++) //Obstacle Generation
        {
            r = Random.Range(0, obstacles.Length);
            obstacles[r].SetActive(true);
        }
        t = Random.Range(25, 40);
        for(int i = 0; i < t; i++) //Interactables Generation
        {
            r = Random.Range(0, interactablePositions.Length);
            if(!interactablePositions[r].occupied)
            {
                int x = 0; //iterations of while loop
                int l; //random num for spawn chance 
                WildernessInteractable wI = null;
                while(x < 7 && wI == null)
                {
                    l = Random.Range(0, WildernessManager.Instance.wildernessInteractables.Length);
                    wI = WildernessManager.Instance.wildernessInteractables[l];
                    if(Random.Range(0,100) > wI.spawnChance || (!interactablePositions[r].fitsLargeObjects && wI.isLarge)) wI = null;
                    x++;
                }
                if(wI != null)
                {
                    GameObject newPrefab;
                    if(wI.prefab == forageablePrefab)
                    {
                        newPrefab = StructurePoolManager.Instance.GrabForageable(true);
                        newPrefab.transform.position = interactablePositions[r].transform.position;
                    }
                    else newPrefab = Instantiate(wI.prefab, interactablePositions[r].transform.position, Quaternion.identity);
                    interactablePositions[r].occupied = true;
                    currentInteractables.Add(newPrefab);
                }
            }
        }

        for(int i = 0; i < setPiecePositions.Length; i++) //Set Pieces Generation
        {
            GameObject prefabToSpawn = null;
            int attempts = 0;
            int s; //index
            while(!prefabToSpawn)
            {
                if(attempts > 10) r = 0;
                else r = Random.Range(0,100);
                s = Random.Range(0, setPiecePrefabs.Length);
                if(setPiecePrefabs[s]._probability >= r)
                {
                    prefabToSpawn = setPiecePrefabs[s]._object;
                }
                attempts++;
            }
            GameObject spawnedObject = Instantiate(prefabToSpawn, setPiecePositions[i].position, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
            currentSetPieces.Add(spawnedObject);
        }
    }

    bool SpotAvailable(Transform t)
    {
        Collider[] hitColliders = Physics.OverlapSphere(t.position, 1f);
        foreach(Collider collider in hitColliders)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure) return false;
        }
        return true;
    }

    public void ClearMap()
    {
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].SetActive(false);
        }

        foreach (GameObject obj in currentInteractables)
        {
            if (obj != null)
            {
                if(obj.GetComponent<Forgeable>()) obj.SetActive(false);
                else Destroy(obj);
            }
        }

        foreach(WildernessInteractableSpot spot in interactablePositions)
        {
            spot.occupied = false;
        }
        currentInteractables.Clear();

        foreach (GameObject obj in currentSetPieces)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        currentSetPieces.Clear();
    }
}
