using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructurePoolManager : MonoBehaviour
{
    //currently should use this for spawning weeds and forgeables

    public static StructurePoolManager Instance;

    public List<GameObject> itemPool = new List<GameObject>();
    public GameObject itemPrefab;

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

        //PopulateItemPool();
    }
}
