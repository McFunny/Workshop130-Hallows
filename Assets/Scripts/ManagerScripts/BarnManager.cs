using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnManager : MonoBehaviour
{
    /////THIS CLASS WILL HOLD ALL REFERENCES TO FARM CRITTERS, AND IDENTIFY IF THEY ARE WITHIN THE BARN OR THE FARM, AS WELL AS OTHER FUNCTIONS/////
    /// 
    public static BarnManager Instance;

    public List<CritterBehaviorScript> allCritters = new List<CritterBehaviorScript>();

    public Transform barnSource, barnWell; //The point used to identify distance and the well for hydroflies

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
    }

    public bool WithinBarn(Vector3 pos)
    {
        if(Vector3.Distance(pos, barnSource.position) > 80) return false;
        else return true;
    }

    public void SaveStats(out CritterData[] critterStats)
    {
        List<CritterData> temp = new List<CritterData>();

        foreach(CritterBehaviorScript c in allCritters)
        {
            ICritter critter = c as ICritter;
            temp.Add(critter.GetCritterData());
        }
        critterStats = temp.ToArray();
    }

    public void LoadStats(AllGameSaveData data)
    {
        int i = 0;
        foreach(CritterData c in data.critterStats)
        {
            if(c.id == -1 || c.health <= 0) continue;
            CritterBehaviorScript newCritter = Instantiate(CreatureDatabase.Instance.GetCreature(c.id).objectPrefab, barnSource.position, Quaternion.identity).GetComponent<CritterBehaviorScript>();
            if(newCritter) newCritter.LoadData(c);
        }
    }
}
