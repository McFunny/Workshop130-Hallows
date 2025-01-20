using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PyreFly : CreatureBehaviorScript
{
    private bool isMoving = false;
    private bool coroutineRunning = false;
    private Transform target;
    public List<StructureObject> targettableStructures;

    private Vector3 despawnPos;
    [HideInInspector] public NavMeshAgent agent;

    float igniteDistance = 3; //distance to ignite structures/be ignited/enter hive

    bool ignited = true;
    public GameObject pyreFire;
    public Material ignitedMat, extinguishedMat;
    public MeshRenderer meshRenderer;
    float textureOffset = 0;
    public float offsetRate = .005f;


    public PyreFlyHive homeHive;
    private StructureBehaviorScript targetStructure; //Struct to burn
    private Brazier targetFireSource;

    public LayerMask layerMask;


    public enum CreatureState
    {
        Wander,
        WalkTowardsClosestStructure,
        WalkTowardsClosestFlame,
        ReturnToHive,
        Stun,
        Die
        //WalkTowardsPlayer,
        //AttackPlayer,
    }

    public CreatureState currentState;


    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        if(ignited) currentState = CreatureState.WalkTowardsClosestStructure;
        else currentState = CreatureState.WalkTowardsClosestFlame;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0) isDead = true;

        if (!isDead && currentState != CreatureState.Stun && currentState != CreatureState.Die)
        {
            CheckState(currentState);
        }

        textureOffset = textureOffset + offsetRate;
        meshRenderer.material.mainTextureOffset = new Vector2(0, textureOffset);
        if(textureOffset > 500) textureOffset = 0;
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.Wander:
                Wander();
                break;

            case CreatureState.WalkTowardsClosestStructure:
                WalkTowardsClosestStructure();
                break;

            case CreatureState.WalkTowardsClosestFlame:
                WalkTowardsClosestFlame();
                break;

            case CreatureState.ReturnToHive:
                ReturnToHive();
                break;

            case CreatureState.Stun:
                break;

            case CreatureState.Die:
                // OnDeath();
                break;

            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    void Wander()
    {
        if (!isMoving && currentState == CreatureState.Wander)
        {
            Vector3 randomPoint = StructureManager.Instance.GetRandomTile();
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    private IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;
        coroutineRunning = true;

        if (TimeManager.Instance.isDay) destination = despawnPos;

        agent.destination = destination;

        float timeSpent = 0; //to make sure it doesnt get stuck

        while ((agent.pathPending || agent.remainingDistance > agent.stoppingDistance) && timeSpent < 25)
        {
            timeSpent += 0.01f;
            yield return null;
        }

        float r = Random.Range(0,10);

        if(r < 7) //otherwise wander
        {
            if(ignited)
            {
                FindBurnableStructure();
                if(targetStructure) currentState = CreatureState.WalkTowardsClosestStructure;
            }
            else
            {
                FindFireSource();
                if(targetFireSource) currentState = CreatureState.WalkTowardsClosestFlame;
                else if(homeHive) currentState = CreatureState.ReturnToHive;
            }
        }
        else currentState = CreatureState.Wander;

        isMoving = false;
        coroutineRunning = false;
    }

    void WalkTowardsClosestStructure()
    {
        if (targetStructure == null || !targetStructure.gameObject.activeSelf)
        {
            FindBurnableStructure();
            if (targetStructure != null)
            {
                target = targetStructure.transform;
                agent.destination = target.position;
            }
            else
            {
                currentState = CreatureState.Wander;
            }
        }
        else if (Vector3.Distance(transform.position, targetStructure.transform.position) < igniteDistance)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            IgniteStructure();
        }
        else if(target == null || agent.destination != target.position)
        {
            target = targetStructure.transform;
            agent.destination = target.position;
        }
    }

    void FindBurnableStructure() //Find thing to burn, regardless of distance
    {
        List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();
        foreach (var structure in structManager.allStructs)
        {
            if (targettableStructures.Contains(structure.structData) && structure.IsFlammable())
                availableStructure.Add(structure);
        }

        if (availableStructure.Count > 0)
        {
            int r = Random.Range(0, availableStructure.Count);
            targetStructure = availableStructure[r];
        }
        //print("Searched for struct to burn. Found: " + targetStructure);
    }

    void WalkTowardsClosestFlame()
    {
        if (targetFireSource == null || !targetFireSource.gameObject.activeSelf)
        {
            FindBurnableStructure();
            if (targetFireSource != null)
            {
                target = targetFireSource.transform;
                agent.destination = target.position;
            }
            else
            {
                currentState = CreatureState.Wander;
            }
        }
        else if (Vector3.Distance(transform.position, targetFireSource.transform.position) < igniteDistance + 1)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            IgniteSelf();
        }
        else if(target == null || agent.destination != target.position)
        {
            target = targetFireSource.transform;
            agent.destination = target.position;
        }
    }

    void FindFireSource() //Find a way to ignite self
    {
        foreach (var structure in structManager.allStructs)
        {
            Brazier brazier = structure as Brazier;
            if (brazier && brazier.flameLeft > 0)
            {
                targetFireSource = brazier;
                print(targetFireSource);
                return;
            }
        }
        print("Searched for fire source. Found None");
    }

    void ReturnToHive()
    {
        if (homeHive == null)
        {
            currentState = CreatureState.Wander;
        }
        else if (Vector3.Distance(transform.position, homeHive.transform.position) < igniteDistance)//(!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 1f)
        {
            agent.ResetPath();
            EnterHive();
        }
        else if(target == null || agent.destination != target.position)
        {
            target = homeHive.transform;
            agent.destination = target.position;
        }
    }

    void IgniteStructure() //burn it
    {
        if(ignited)
        {
            if(targetStructure && targetStructure.IsFlammable())
            {
                IgnitionToggle(false);
                targetStructure.LitOnFire();
                targetStructure = null;
                currentState = CreatureState.Wander;
            }
            else
            {
                FindBurnableStructure();
                if(targetStructure) currentState = CreatureState.WalkTowardsClosestStructure;
                else currentState = CreatureState.Wander;
            }
        }
        else
        {
            currentState = CreatureState.Wander;
        }
    }

    void IgniteSelf()
    {
        if(targetFireSource && targetFireSource.flameLeft > 0)
        {
            IgnitionToggle(true);
            currentState = CreatureState.Wander;
        }
        else
        {
            FindFireSource();
            if(targetFireSource) currentState = CreatureState.WalkTowardsClosestFlame;
            else if(homeHive) currentState = CreatureState.ReturnToHive;
            else currentState = CreatureState.Wander;
        }
        
    }

    public void IgnitionToggle(bool IsIgnited)
    {
        if(ignited == IsIgnited) return;
        ignited = IsIgnited;

        if(ignited)
        {
            pyreFire.SetActive(true);
            meshRenderer.material = ignitedMat;
            //mat changes
        }
        else
        {
            pyreFire.SetActive(false);
            meshRenderer.material = extinguishedMat;
            if(currentState == CreatureState.WalkTowardsClosestStructure)
            {
                currentState = CreatureState.Wander;
            }

            if(effectsHandler)
            {
                ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = transform.position;
                effectsHandler.MiscSound();
            }
        }

    }

    void EnterHive()
    {
        if(homeHive == null) currentState = CreatureState.Wander;
        if(homeHive.ignited)
        {
            IgnitionToggle(true);
            currentState = CreatureState.Wander;
        }
        else
        {
            homeHive.FlyLost();
            Destroy(this.gameObject);
        }
    }

    public override void HitWithWater()
    {
        IgnitionToggle(false);
    }

    public void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;
        if(homeHive) homeHive.FlyLost();

        if(ignited)
        {
            ParticlePoolManager.Instance.GrabExplosionParticle().transform.position = corpseParticleTransform.position;
            if(PlayerInteraction.Instance.stamina > 0) effectsHandler.ThrowSound(effectsHandler.deathSound);
            if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 8.1f) PlayerInteraction.Instance.StaminaChange(-damageToPlayer);
            Collider[] hitStructures = Physics.OverlapSphere(transform.position, 1.5f, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
                if(structure && structure.IsFlammable())
                {
                    structure.LitOnFire();
                }
            }

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 8f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null && creature.shovelVulnerable)
                {
                    creature.TakeDamage(75);
                    creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
                }
            }
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.Torch && !PlayerInteraction.Instance.torchLit && ignited)
        {
            HandItemManager.Instance.TorchFlameToggle(true);
            success = true;
        }
        else success = false;
    }

    //
}
