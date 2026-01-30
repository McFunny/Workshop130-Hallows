using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrubSwarm : CreatureBehaviorScript
{
    public List<GameObject> grubs = new List<GameObject>();

    public List<StructureBehaviorScript> swarmTargets = new List<StructureBehaviorScript>();

    public int grubMin, grubMax;
    public GameObject grubPrefab;

    public List<StructureObject> targettableStructures;
    private StructureBehaviorScript targetStructure;

    public List<CropData> desiredCrops; //More likely to target these than others
    public List<CropData> undesiredCrops; //Will never target these

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        int grubsToSpawn = Random.Range(grubMin, grubMax + 1);

        switch (GameSaveData.Instance.siegesCleared)
        {
            case 0:
            grubsToSpawn-= 3;
            break;
            case 1: grubsToSpawn-= 1;
            break;
            case 2:
            grubsToSpawn+= 1;
            break;
            case 3:
            grubsToSpawn+= 2;
            break;
            default:
            grubsToSpawn+= 2;
            break;
        }

        for(int i = 0; i < grubsToSpawn; i++)
        {
            Grub grub = Instantiate(grubPrefab, transform.position, Quaternion.identity).GetComponentInParent<Grub>();
            grub.homeSwarm = this;
            grubs.Add(grub.gameObject);
        }

        StartCoroutine(AssignTarget());
    }

    IEnumerator AssignTarget()
    {
        yield return new WaitForSeconds(4);
        GatherNewTargets();
        while(grubs.Count > 0)
        {
            yield return new WaitForSeconds(Random.Range(4, 7));
            for(int i = 0; i < swarmTargets.Count; ++i)
            {
                if(swarmTargets[i] == null)
                {
                    swarmTargets.RemoveAt(i);
                    --i;
                }
            }

            if(swarmTargets.Count == 0) GatherNewTargets();
        }
        Destroy(gameObject);
    }

    void GatherNewTargets()
    {
        float maxDistance = 5;

        float distanceToStructure;

        List<StructureBehaviorScript> availableStructures = new List<StructureBehaviorScript>();
        List<StructureBehaviorScript> priorityStructure = new List<StructureBehaviorScript>();

        foreach (var structure in structManager.allStructs) //Find all the valid structures
        {
            FarmLand tile = structure as FarmLand;
            if (targettableStructures.Contains(structure.structData) && !structure.absentFromFarmGrid && (!tile || (tile.crop && !tile.isWeed && tile.currentUpgrade != FarmLand.FarmTileUpgrade.Corrupt)
                && !undesiredCrops.Contains(tile.crop)))
            {
                availableStructures.Add(structure);

                if(tile && tile.crop && desiredCrops.Contains(tile.crop)) priorityStructure.Add(structure); 
            }
        }

        if (priorityStructure.Count > 0 && Random.Range(0,10) > 3)
        {
            int r = Random.Range(0, priorityStructure.Count);
            transform.position = priorityStructure[r].transform.position;

            foreach (var validStructure in priorityStructure)
            {
                distanceToStructure = Vector3.Distance(transform.position, validStructure.transform.position);
                if (targettableStructures.Contains(validStructure.structData) && !validStructure.absentFromFarmGrid && distanceToStructure < maxDistance)
                {
                    swarmTargets.Add(validStructure);
                }
            }
        }

        else if (availableStructures.Count > 0) // pick a random one and mark all of the ones nearby it as attackable
        {
            int r = Random.Range(0, availableStructures.Count);
            transform.position = availableStructures[r].transform.position;

            foreach (var validStructure in availableStructures)
            {
                distanceToStructure = Vector3.Distance(transform.position, validStructure.transform.position);
                if (targettableStructures.Contains(validStructure.structData) && !validStructure.absentFromFarmGrid && distanceToStructure < maxDistance)
                {
                    swarmTargets.Add(validStructure);
                }
            }
        }
    }
}
