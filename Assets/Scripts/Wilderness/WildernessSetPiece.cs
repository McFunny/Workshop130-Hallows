using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessSetPiece : MonoBehaviour
{
    public Transform[] interactablePositions; //Locations that the giant setpieces can take
    public ObjectWithProbability[] interactablePrefabs;
    // Start is called before the first frame update
    void Awake()
    {
        int r; //random number
        for(int i = 0; i < interactablePositions.Length; i++) //Interactables Generation
        {
            if(Random.Range(0, 100) < 60) continue;


            int x = 0; //iterations of while loop
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
    }
}
