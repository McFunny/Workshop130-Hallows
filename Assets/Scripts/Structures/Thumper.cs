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

    void Start()
    {
        UpdateModel();
        base.Start();
    }

    public override void StructureInteraction()
    {
        if(charge == 0) return;
        StartCoroutine(TriggerTrap());
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

    [ContextMenu("Update Model")]
    void UpdateModel()
    {
        for(int i = 0; i < panels.Length; ++i)
        {
            if(i < charge) panels[i].SetActive(true);
            else panels[i].SetActive(false);
        }
        anim.SetInteger("ChargeLevel", charge);
    }

    public void ForceTrap()
    {
        if(charge == 0) return;
        StartCoroutine(TriggerTrap(Random.Range(0.3f, 0.9f)));
    }

    IEnumerator TriggerTrap(float delay = 0)
    {
        anim.Play("Thumper_Activated");

        int tempCharge = charge;
        charge = 0;
        yield return new WaitForSeconds(delay);
        yield return new WaitForSeconds(0.1f);
        //triggeredParticles.Play();

        float range = 0;
        float damageDealt = 20;
        float screenShake = 0.2f;
        ParticleSystem p;

        switch(tempCharge)
        {
            case 1:
            range = 10f;
            p = smallPulse;
            break;
            case 2:
            range = 13.5f;
            damageDealt = 30;
            p = mediumPulse;
            screenShake = 0.5f;
            break;
            case 3:
            range = 18f;
            damageDealt = 40;
            p = largePulse;
            screenShake = 0.8f;
            break;
            default:
            range = 10f;
            p = smallPulse;
            break;
        }
        yield return new WaitForSeconds(0.3f);

        audioHandler.PlaySound(audioHandler.interactSound);
        ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = transform.position;

        yield return new WaitForSeconds(0.1f);

        audioHandler.PlaySound(audioHandler.miscSounds1[tempCharge - 1]);
        audioHandler.PlaySound(audioHandler.miscSounds2[1]);
        ParticlePoolManager.Instance.GrabElecZapParticle().transform.position = transform.position; 
        PlayerInteraction.Instance.ShakeScreen(screenShake);
        

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

            Thumper t = structure as Thumper;
            if(t && t != this)
            {
                t.ForceTrap();
                yield return new WaitForSeconds(0.15f);
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
