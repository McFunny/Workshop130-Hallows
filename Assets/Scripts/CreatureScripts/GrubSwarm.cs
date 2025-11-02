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

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        int grubsToSpawn = Random.Range(grubMin, grubMax + 1);

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
        while(grubs.Count > 0)
        {
            yield return new WaitForSeconds(Random.Range(10, 20));
            if(swarmTargets.Count == 0) GatherNewTargets();
        }
        Destroy(gameObject);
    }

    void GatherNewTargets()
    {
        float maxDistance = 10;

        float distanceToStructure;

        List<StructureBehaviorScript> availableStructures = new List<StructureBehaviorScript>();
        foreach (var structure in structManager.allStructs) //Find all the valid structures
        {
            FarmLand tile = structure as FarmLand;
            if (targettableStructures.Contains(structure.structData) && !structure.absentFromFarmGrid && (!tile || (tile.crop && !tile.isWeed)))
            {
                availableStructures.Add(structure);
            }
        }

        if (availableStructures.Count > 0) // pick a random one and mark all of the ones nearby it as attackable
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
