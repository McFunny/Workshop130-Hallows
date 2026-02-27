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

    List<TruffleHog> enlistedHogs = new List<TruffleHog>();

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

    void Start()
    {
        WildernessManager.OnWildernessLeave += FreeWildernessHogs;
    }

    void OnDestroy()
    {
        WildernessManager.OnWildernessLeave -= FreeWildernessHogs;
    }

    public void AddCreatureToCritterList(CritterBehaviorScript newCritter)
    {
        if(allCritters.Contains(newCritter)) return;
        else allCritters.Add(newCritter);

        int hogs = 0; //For Achievement
        List<CreatureObject> ownedTypes = new List<CreatureObject>();
        for(int i = 0; i < allCritters.Count; ++i)
        {
            if(!ownedTypes.Contains(allCritters[i].creatureData)) ownedTypes.Add(allCritters[i].creatureData);

            TruffleHog tHog = allCritters[i] as TruffleHog;
            if(tHog) ++hogs;
        }
        if(hogs >= 5) AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Hog_House);
        if(ownedTypes.Count >= 4) AchievementManager.Instance.CompleteProgressWithEnum(ACHKey.Millers_Ark);
    }

    public bool WithinBarn(Vector3 pos)
    {
        if(Vector3.Distance(pos, barnSource.position) > 80) return false;
        else return true;
    }

    public bool GrabHogsForWilderness()
    {
        List<TruffleHog> eligibleHogs = new List<TruffleHog>();
        foreach(CritterBehaviorScript c in allCritters)
        {
            TruffleHog hog = c as TruffleHog;

            if(c && c.health == c.maxHealth) eligibleHogs.Add(hog);
        }

        if(eligibleHogs.Count < 2) return false;

        for(int i = 0; i < 2; i++)
        {
            int x = Random.Range(0, eligibleHogs.Count);
            enlistedHogs.Add(eligibleHogs[x]);
            eligibleHogs[x].usedForWagon = true;
            eligibleHogs.RemoveAt(x);
        }
        return true;
    }

    void FreeWildernessHogs()
    {
        foreach(TruffleHog hog in enlistedHogs)
        {
            hog.health = 5;
            hog.usedForWagon = false;
            hog.transform.position = WagonManager.Instance.farmWagon.critterPos.position;
        }
        enlistedHogs.Clear();
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
