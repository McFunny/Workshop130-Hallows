using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thumper : StructureBehaviorScript
{
    public Animator anim;
    public GameObject[] panels;

    public ParticleSystem smallPulse, mediumPulse, largePulse;

    public int charge = 0;
    int maxCharge = 3;
    int chargeProgress = 0;
    int progressNeeded = 3;

    public List<StructureObject> breakableStructures;



    public override void StructureInteraction()
    {
        if(charge == 0) return;
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            success = true;
        }
    }

    public override void HourPassed()
    {
        if(charge == maxCharge) return;

        chargeProgress++;
        if(chargeProgress >= progressNeeded)
        {
            ++charge;
            chargeProgress = 0;
            UpdateModel();
            audioHandler.PlaySound(audioHandler.activatedSound);
        }
    }

    void UpdateModel()
    {
        for(int i = 0; i < panels.Length; ++i)
        {
            if(i < charge) panels[i].SetActive(true);
            else panels[i].SetActive(false);
        }
    }

    IEnumerator TriggerTrap()
    {
        int tempCharge = charge;
        charge = 0;
        yield return new WaitForSeconds(0.1f);
        //triggeredParticles.Play();

        float range = 0;
        float damageDealt = 20;
        ParticleSystem p;

        switch(tempCharge)
        {
            case 1:
            range = 4.5f;
            p = smallPulse;
            break;
            case 2:
            range = 9f;
            damageDealt = 30;
            p = mediumPulse;
            break;
            case 3:
            range = 12.5f;
            damageDealt = 40;
            p = largePulse;
            break;
            default:
            range = 3.5f;
            p = smallPulse;
            break;
        }
        yield return new WaitForSeconds(0.3f);

        audioHandler.PlaySound(audioHandler.interactSound);

        yield return new WaitForSeconds(0.1f);

        audioHandler.PlaySound(audioHandler.miscSounds1[tempCharge - 1]);

        p.Play();

        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.playerFeet.position) < range && tempCharge == 3)
        {
            //PlayerInteraction.Instance.StaminaChange(damageToPlayer);
            PlayerInteraction.Instance.PlayerTrip();
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, range, 1 << 9);
        List<CreatureBehaviorScript> hitCreatures = new List<CreatureBehaviorScript>();
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && !hitCreatures.Contains(creature))
            {
                creature.TakeDamage(damageDealt);
                creature.PlayHitParticle(creature.transform.position);
                hitCreatures.Add(creature);
            }
        }

        Collider[] hitStructures = Physics.OverlapSphere(transform.position, range, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            var structure = collider.GetComponentInParent<StructureBehaviorScript>();
            if(!structure) continue;

            if(breakableStructures.Contains(structure.structData))
            {
                structure.TakeDamage(99);
                continue;
            }

            TuskTrap trap = structure as TuskTrap;
            if(trap)
            {
                trap.ForceActivateTrap();
                continue;
            }

            BucketStructure bucket = structure as BucketStructure;
            if(bucket)
            {
                bucket.ForceSpillBucket();
                continue;
            }

            FarmLand tile = structure as FarmLand;
            if(tile && !tile.isWeed)
            {
                if(tile.harvestable) tile.ToolInteraction(ToolType.Scythe, out bool success);
                else if(!tile.crop) structure.TakeDamage(99);
            }

        }

        UpdateModel();
        
    }

    public override void LoadVariables()
    {
        charge = saveInt1;
        UpdateModel();
    }

    public override void SaveVariables()
    {
        saveInt1 = charge;
    }
}
