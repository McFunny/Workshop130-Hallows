using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessSetPiece : MonoBehaviour
{
    public Transform[] interactablePositions; //Locations that the giant setpieces can take
    public ObjectWithProbability[] interactablePrefabs;

    public int chanceToSpawnCreature; //Chance to pick a creature to spawn
    public int maxSpawnAttempts; //How many times it will try to spawn something
    public ObjectWithProbability[] creaturePrefabs;
    public CreatureBehaviorScript[] preSpawnedCreatures; //Things like hives; something guaranteed to spawn
    // Start is called before the first frame update
    void Awake()
    {
        int r; //random number
        int x = 0; //iterations of while loop
        for(int i = 0; i < interactablePositions.Length; i++) //Interactables Generation
        {
            if(Random.Range(0, 100) < 60) continue;


            x = 0;
            GameObject newPrefab = null;
            while(newPrefab == null)
            {
                r = Random.Range(0, interactablePrefabs.Length);
                if(interactablePrefabs[r]._probability > Random.Range(0,100) || x >= 10)
                {
                    newPrefab = Instantiate(interactablePrefabs[r]._object, interactablePositions[i].position, Quaternion.Euler(0, Random.Range(0.0f, 360.0f), 0));
                }
                x++;

            }
            newPrefab.transform.SetParent(gameObject.transform);
        }

        //Code to make prespawned creatures be added to the wilderness creature list
        foreach(CreatureBehaviorScript creature in preSpawnedCreatures)
        {
            //WildernessManager.Instance.allCreatures.Add(creature);
            //Decided not to track this. It messes with the spawn cap of creatures
        }


        for(int i = 0; i < maxSpawnAttempts; i++) //Creature Generation
        {
            if(Random.Range(0, 100) > chanceToSpawnCreature) continue;


            x = 0;
            GameObject newPrefab = null;
            while(newPrefab == null)
            {
                r = Random.Range(0, creaturePrefabs.Length);
                if(creaturePrefabs[r]._probability > Random.Range(0,100) || x >= 10)
                {
                    newPrefab = Instantiate(creaturePrefabs[r]._object, transform.position, Quaternion.identity);
                }
                x++;

            }
            CreatureBehaviorScript c = newPrefab.GetComponentInChildren<CreatureBehaviorScript>();
            c.patrolPoint = transform;
            c.inWilderness = true;
            //WildernessManager.Instance.allCreatures.Add(c);
        }
    }
}
