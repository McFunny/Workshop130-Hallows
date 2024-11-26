using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePoolManager : MonoBehaviour
{
    //currently should use this for spawning weeds and forgeables

    public static StructurePoolManager Instance;

    public List<GameObject> forageablePool = new List<GameObject>();
    public GameObject forageablePrefab;

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
        //

        for(int i = 0; i < 5; i++)
        {
            GameObject newStructure = Instantiate(forageablePrefab);
            forageablePool.Add(newStructure);
            newStructure.SetActive(false);
        }
    }

    public GameObject GrabForageable()
    {
        foreach (GameObject structure in forageablePool)
        {
            if(!structure.activeSelf)
            {
                structure.SetActive(true);
                structure.GetComponent<Forgeable>().Refresh();
                return structure;
            }
        }

        //No available structure, must make a new one
        GameObject newStructure = Instantiate(forageablePrefab);
        forageablePool.Add(newStructure);
        newStructure.GetComponent<Forgeable>().Refresh();
        return newStructure;
    }
}
