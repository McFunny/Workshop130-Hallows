using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Wraith : CreatureBehaviorScript
{

    //Wraith only moves towards player slowly. If it lingers in fire, its model gets brighter until it teleports. Doesn't avoid Braziers. Frosts crops it touches
    //Add frost collider and function to a child object

    Renderer[] renderers; //make it a list of all children renderers

    public GameObject flowerPrefab;

    public float timeSpentInFire;
    public float flameDecayRate = 0.5f;
    public float maxFlameTime = 1.5f;
    private FireFearTrigger fireSource;
    List<FireFearTrigger> nearbyFires = new List<FireFearTrigger>();

    private Coroutine trackPlayerRoutine; 

    [HideInInspector] public NavMeshAgent agent;

    public Color unlightColor, lightColor;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        base.Start();
        SpawnFlower();
        renderers = GetComponentsInChildren<Renderer>();
    }


    void Update()
    {
        if (!isDead)
        {
            if(TownGate.Instance.inTown) Freeze();
            else ChasePlayer();
        }

        if(nearbyFires.Count > 0)
        {
            //float distFromFire = Vector3.Distance(fireSource.transform.position, transform.position);

            timeSpentInFire += Time.deltaTime; //will this work

            //if(fireSource.gameObject.activeSelf == false || distFromFire > fireSource.fleeRange)
            //{
                //fireSource = null;
            //}
            if(timeSpentInFire > maxFlameTime)
            {
                timeSpentInFire = 0;
                Teleport();
            }
        }
        else if(timeSpentInFire > 0)
        {
            timeSpentInFire -= Time.deltaTime * 0.5f;
        }

        UpdateTransparency();
    }

    void ChasePlayer()
    {
        if (trackPlayerRoutine == null)
        {
            agent.isStopped = false;
            trackPlayerRoutine = StartCoroutine(TrackPlayer());
        }
    }

    void Freeze()
    {
        if(trackPlayerRoutine != null)
        {
            trackPlayerRoutine = null;
            agent.isStopped = true;
        }
        
    }

    private IEnumerator TrackPlayer()
    {
        while (!isDead)
        {
            agent.destination = player.position;
            yield return new WaitForSeconds(1.5f); // update destination every 0.5 seconds to prevent overloading it
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= 3.5f)
            {
                PlayerInteraction.Instance.StaminaChange(-7);
            }
        }
        trackPlayerRoutine = null;
    }

    void Teleport()
    {
        ParticlePoolManager.Instance.GrabCloudParticle().transform.position = corpseParticleTransform.position;
        transform.position = NightSpawningManager.Instance.RandomMistPosition();
    }

    public override void EnteredFireRadius(FireFearTrigger _fireSource, out bool successful)
    {
        //fireSource = _fireSource;
        if(!nearbyFires.Contains(_fireSource)) nearbyFires.Add(_fireSource);
        successful = true;
    }

    void OnTriggerExit(Collider other)
    {
        FireFearTrigger trigger = other.GetComponent<FireFearTrigger>();
        if(trigger && nearbyFires.Contains(trigger)) nearbyFires.Remove(trigger);
    }

    void SpawnFlower()
    {
        Vector3 flowerSpawn = StructureManager.Instance.GetRandomClearTile();
        if(flowerSpawn == new Vector3(0,0,0)) Destroy(this.gameObject);
        else
        {
            Instantiate(flowerPrefab, flowerSpawn, Quaternion.identity).GetComponent<WraithFlower>().assignedWraith = this;
        }
    }

    void UpdateTransparency()
    {
        /*Color newColor = renderers.material.color;
        if(timeSpentInFire <= 0) newColor.a = 0;
        else newColor.a = timeSpentInFire/maxFlameTime;
        renderers.material.color = newColor; */

        for(int i = 0; i < renderers.Length; i++)
        {
            Material mat = renderers[i].material;
            Color newColor = Color.Lerp(unlightColor, lightColor, timeSpentInFire/maxFlameTime);
            mat.SetColor("_EmissionColor", newColor);
        }

        //change volume of fizzling sound when its getting deterred
    }

    public override void OnDeath()
    {
        if (!isDead)
        {
            isDead = true;
            base.OnDeath();
            agent.enabled = false;
            StopAllCoroutines();
        }
    }
}
