using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/WhipWeed")]
public class WhipWeedBehavior : CropBehavior
{
    //It will consume ichor per whip. When its out of ichor, it just stays there. Attacks units standing on a tile

    public bool corruptionVariant = false;

    public StructureObject tileData;

    public GameObject weedAttack;

    public float range = 15;

    float attackCost = 0.25f;

    public LayerMask targetMask;

    public override void BehaviorUpdate(FarmLand tile)
    {
        if(tile.growthStage != tile.crop.growthStages || (tile.GetCropStats().ichorLevel < attackCost && !corruptionVariant)) return;
        Collider[] hitEnemies = Physics.OverlapSphere(tile.transform.position, range, targetMask);
        foreach(Collider collider in hitEnemies)
        {
            var c = collider.GetComponentInParent<CreatureBehaviorScript>();
            PlayerInteraction player = null;
            if(collider.gameObject.layer == 10) player = PlayerInteraction.Instance;
            if ((c != null && c.health > 0 && c.shovelVulnerable && c.bearTrapVulnerable && (!corruptionVariant || c.corpseType != CorpseParticleType.Corrupted)) || player != null)
            {
                Vector3 target;
                if(player) target = player.playerFeet.position;
                else target = c.transform.position;

                target.y = 0;

                Collider[] nearbyTiles = Physics.OverlapSphere(target, 2, 1 << 6);
                Debug.Log(nearbyTiles.Length);
                for(int i = 0; i < nearbyTiles.Length; ++i)
                {
                    StructureBehaviorScript structure = nearbyTiles[i].GetComponent<StructureBehaviorScript>();
                    if(structure && structure.structData && structure.structData.id == tileData.id)
                    {
                        //attack the target
                        Instantiate(weedAttack, tile.transform.position, Quaternion.identity).GetComponent<RootWhip>().target = target;
                        if(!corruptionVariant) tile.GetCropStats().ichorLevel -= attackCost;
                        return;
                    }
                }
            }
        }
    }

    public override void CropRemovalBonusYield(FarmLand tile, out int secondaryCropBonus)
    {
        if(tile.growthStage == 6) secondaryCropBonus = 1;
        else secondaryCropBonus = 0;
    }
}
