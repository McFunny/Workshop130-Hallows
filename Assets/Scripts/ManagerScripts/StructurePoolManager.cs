using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePoolManager : MonoBehaviour
{
    //currently should use this for spawning forgeables and crows

    public static StructurePoolManager Instance;

    public List<GameObject> forageablePool = new List<GameObject>();
    public GameObject forageablePrefab;

    public Transform[] forageableSpots, crowSpots;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        PopulateForageablePool();
    }

    void PopulateForageablePool()
    {
        for(int i = 0; i < 15; i++)
        {
            GameObject newStructure = Instantiate(forageablePrefab);
            forageablePool.Add(newStructure);
            newStructure.SetActive(false);
        }
    }

    public GameObject GrabForageable(bool inWilderness)
    {
        foreach (GameObject structure in forageablePool)
        {
            if(!structure.activeSelf)
            {
                structure.SetActive(true);
                structure.GetComponent<Forgeable>().Refresh(inWilderness);
                return structure;
            }
        }
        
        int r = Random.Range(0, forageablePool.Count);
        forageablePool[r].SetActive(true);
        forageablePool[r].GetComponent<Forgeable>().Refresh(inWilderness);
        return forageablePool[r];
    }
}
