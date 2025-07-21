using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SiegeManager : MonoBehaviour
{
    //public int siegePhase = 0; //This determines stuff like when items are sold, what siege level is next, and what macguffin crop is needed to be grown/sold

    public bool siegeCropOnFarm = false;

    public List<NightPoolObject> siegePools = new List<NightPoolObject>();

    public static SiegeManager Instance;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

    }
}
