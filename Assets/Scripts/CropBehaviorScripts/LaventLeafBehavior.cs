using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/LaventLeaf")]
public class LaventLeafBehavior : CropBehavior
{
    public override void BehaviorUpdate(FarmLand tile)
    {
        float range = 1f;
        if(tile.growthStage > 4) range = 5f;
        else if(tile.growthStage > 2) range = 3.5f;
        else if(tile.growthStage == 1) return;
        Collider[] hitEnemies = Physics.OverlapSphere(tile.transform.position, range, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null)
            {
                creature.NearLaventLeaf(tile.transform.position);
            }
        }

        Collider[] hitBugs = Physics.OverlapSphere(tile.transform.position, range, 1 << 21);
        foreach(Collider collider in hitBugs)
        {
            var bug = collider.GetComponentInParent<BugBehaviorScript>();
            if (bug != null)
            {
                bug.NearLaventLeaf(tile.transform.position);
            }
        }
    }
}
