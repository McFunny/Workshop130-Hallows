using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessMap : MonoBehaviour
{
    public Transform[] spawnPositions; //Possible player spawns
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
        int r;
        int t = Random.Range(1, 5);
        for(int i = 0; i < t; i++)
        {
            r = Random.Range(0, obstacles.Length);
            obstacles[r].SetActive(true);
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
