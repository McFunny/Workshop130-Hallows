using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessMap : MonoBehaviour
{
    public Transform[] spawnPositions; //Possible player spawns
    public Transform[] wagonPositions; //Associated wagon spawns
    public Transform[] enemySpawnPositions; //Spots enemies can spawn from. Should grab the closest 2 from the player
    public Transform[] setPiecePositions; //Locations that the giant setpieces can take
    public Transform[] interactablePositions; //Locations of small things like trees with nuts, hives, and foreagables can spawn near
    public GameObject[] obstacles; //Locations that block paths. Must be enabled or disabled

    void Start()
    {
        if(!WildernessManager.Instance.allMaps.Contains(this)) WildernessManager.Instance.allMaps.Add(this);
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].SetActive(false);
        }
    }

    public void InitializeMap()
    {
        int r; //random number
        int t = Random.Range(1, 5); //random number of interations
        for(int i = 0; i < t; i++)
        {
            r = Random.Range(0, obstacles.Length);
            obstacles[r].SetActive(true);
        }

        List<Transform> usedSpots = new List<Transform>();
        t = Random.Range(15, 25);
        for(int i = 0; i < t; i++)
        {
            r = Random.Range(0, interactablePositions.Length);
            if(!usedSpots.Contains(interactablePositions[r]))
            {
                usedSpots.Add(interactablePositions[r]);
                int x = 0; //iterations of while loop
                int l; //random num for spawn chance
                GameObject prefab = null;
                while(x < 0 && prefab == null)
                {
                    l = Random.Range(0, WildernessManager.Instance.interactablePrefabs.Length);
                    prefab = WildernessManager.Instance.interactablePrefabs[l];
                    if(Random.Range(0,100) <= WildernessManager.Instance.interactableSpawnChances[l])
                    x++;
                }
                if(prefab != null) Instantiate(prefab, interactablePositions[r].position, Quaternion.identity);
            }
        }
    }

    public void ClearMap()
    {
        for(int i = 0; i < obstacles.Length; i++)
        {
            obstacles[i].SetActive(false);
        }
    }
}
