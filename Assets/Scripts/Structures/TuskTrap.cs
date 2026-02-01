using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TuskTrap : StructureBehaviorScript
{
    public Transform triggeredPos, setPos;

    public Transform model;

    bool isArmed, isTriggered;

    public float altitude = 0; //0 meaning its at the top
    float currentRate = 0;
    float rateChange = 3;
    float setRateMax = 1; //per second.
    float distance; //progress until fully set
    bool interacting = false;

    float damageToPlayer = -15;
    float damageToCreature = 40;

    public AudioSource crankingSource;

    void Awake()
    {
        base.Awake();
        distance = Vector3.Distance(triggeredPos.position, setPos.position);
    }

    void Update()
    {
        base.Update();


        if(isArmed || isTriggered) return;

        if(altitude < distance && (!interacting || !InputManager.isHoldingInteract)) //Stopped setting the trap
        {
            altitude = 0;
            StartCoroutine(SpringTrap());
            crankingSource.Stop();
        }
        else if(interacting && InputManager.isHoldingInteract) //Setting the trap
        {
            //altitude += setRateMax * Time.deltaTime;
            currentRate = currentRate + rateChange * Time.deltaTime;

            if(currentRate < setRateMax) currentRate = setRateMax;

            altitude += currentRate * Time.deltaTime;
        }

        
        if(altitude > distance)
        {
            altitude = distance;
            currentRate = 0;

            isArmed = true;
            crankingSource.Stop();
            audioHandler.PlaySound(audioHandler.interactSound);
        }

        model.position = Vector3.Lerp(triggeredPos.position, setPos.position, altitude/distance);

        if(altitude > 0 && !isArmed && !crankingSource.isPlaying) crankingSource.Play();
    }


    public override void StructureInteraction()
    {
        if(isArmed || isTriggered) return;
        interacting = true;
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            success = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(!isArmed) return;
        if(other.gameObject.layer == 9 || other.gameObject.layer == 10)
        {
            CreatureBehaviorScript creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if(creature)
            {
                if(!creature.bearTrapVulnerable) return;

                else if(!creature.shovelVulnerable) //break it. IE golem steps on it
                {
                    TakeDamage(99);
                    return;
                }
            }
            isArmed = false;
            StartCoroutine(SpringTrap()); //pass enemy script or player script variable
        }
    }

    IEnumerator SpringTrap()
    {
        isTriggered = true;
        altitude = 0;
        yield return new WaitForSeconds(0.2f);
        audioHandler.PlaySound(audioHandler.activatedSound);

        //Deal damage using physics cast and animate it moving up

        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.playerFeet.position) < 1f)
        {
            PlayerInteraction.Instance.StaminaChange(damageToPlayer);
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 1.3f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(damageToCreature);
                creature.PlayHitParticle(creature.transform.position);
            }
        }

        yield return new WaitForSeconds(0.5f);
        isTriggered = false;
        
    }
}
