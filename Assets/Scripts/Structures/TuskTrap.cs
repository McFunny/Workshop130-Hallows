using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TuskTrap : StructureBehaviorScript
{
    public Transform triggeredPos, setPos;

    //Vector3 originRot;

    public Transform model;

    bool isArmed, isTriggered;

    public float altitude = 0; //0 meaning its at the top
    float currentRate = 0;
    float rateChange = 0.5f;
    float setRateMax = 0.5f; //per second.
    float distance; //progress until fully set
    bool interacting = false;

    float damageToPlayer = -15;
    float damageToCreature = 40;

    public AudioSource crankingSource;

    public ParticleSystem triggeredParticles, interactingParticles;

    public GameObject leafPilePrefab;
    GameObject currentLeaves;

    public MeshRenderer r;
    public Material bloodiedMat;

    void Awake()
    {
        base.Awake();
        distance = Vector3.Distance(triggeredPos.position, setPos.position);
    }

    void Update()
    {
        base.Update();

        if(isArmed) return;

        if(isTriggered)
        {
            if(altitude < 0) altitude = 0;
            if(altitude == 0) 
            {
                currentRate = 0;
                return;
            }
            currentRate = -5;
            altitude += currentRate * Time.deltaTime;
        }
        else if(altitude != 0 && altitude < distance && (!interacting || !InputManager.isHoldingInteract)) //Stopped setting the trap
        {
            //altitude = 0;
            //currentRate = 0;
            StartCoroutine(SpringTrap());
            crankingSource.Stop();
            interactingParticles.Stop();
        }
        else if(interacting && InputManager.isHoldingInteract) //Setting the trap
        {
            //altitude += setRateMax * Time.deltaTime;
            currentRate = currentRate + rateChange * Time.deltaTime;

            if(currentRate < setRateMax) currentRate = setRateMax;

            altitude += currentRate * Time.deltaTime;
        }
        else currentRate = 0;

        
        if(altitude > distance)
        {
            altitude = distance;
            currentRate = 0;

            isArmed = true;
            crankingSource.Stop();
            interactingParticles.Stop();
            audioHandler.PlaySound(audioHandler.interactSound);
        }

        model.position = Vector3.Lerp(triggeredPos.position, setPos.position, altitude/distance);

        if(altitude > 0 && !isArmed)
        {
            if(!crankingSource.isPlaying && !isTriggered) 
            {
                crankingSource.Play();
                interactingParticles.Play();
            }
            model.Rotate(model.rotation.x, model.rotation.y + (currentRate * 3), model.rotation.z);
        }
    }

    protected override void OnHighlight(bool enabled)
    {
        if(enabled == false) interacting = false;
    }

    public override void HourPassed()
    {
        if(!isArmed) return;

        if(Random.Range(0, 200) < 3 && Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) > 40 && !currentLeaves)
        {
            currentLeaves = Instantiate(leafPilePrefab, transform.position, Quaternion.identity);
            StructureBehaviorScript leafStructure = currentLeaves.GetComponent<StructureBehaviorScript>();
            leafStructure.absentFromFarmGrid = true;
            leafStructure.clearTileOnDestroy = false;
        }
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
                TakeDamage(1);
            }
            isArmed = false;
            StartCoroutine(SpringTrap()); //pass enemy script or player script variable
        }
    }

    public void ForceActivateTrap()
    {
        if(isArmed && !isTriggered) StartCoroutine(SpringTrap());
    }

    void ForceSetTrap()
    {
        model.position = setPos.position;
    }

    IEnumerator SpringTrap()
    {
        isTriggered = true;
        //altitude = 0;
        yield return new WaitForSeconds(0.1f);
        audioHandler.PlaySound(audioHandler.activatedSound);
        triggeredParticles.Play();

        //Deal damage using physics cast and animate it moving up

        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.playerFeet.position) < 1.3f)
        {
            PlayerInteraction.Instance.StaminaChange(damageToPlayer);
            //PlayerInteraction.Instance.PlayerTrip();
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 1.3f, 1 << 9);
        List<CreatureBehaviorScript> hitCreatures = new List<CreatureBehaviorScript>();
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable && !hitCreatures.Contains(creature))
            {
                creature.TakeDamage(damageToCreature);
                creature.PlayHitParticle(creature.transform.position);
                hitCreatures.Add(creature);

                r.material = bloodiedMat;
            }
        }

        yield return new WaitForSeconds(0.5f);
        model.position = triggeredPos.position;
        isTriggered = false;
        currentRate = 0;
        
    }

    public override void LoadVariables()
    {
        isArmed = saveBool1;
        if(isArmed) ForceSetTrap();
    }

    public override void SaveVariables()
    {
        saveBool1 = isArmed;
    }
}
