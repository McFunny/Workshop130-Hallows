using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CreatureBehaviorScript : MonoBehaviour
{
    //This is the base class that ALL creatures should derive from
    public float health = 100;
    public float maxHealth = 100;
    public float corpseHealth = -50; //what does the health need to be at for corpse removal
    public float ichorWorth = 5; //How much ichor does killing this provide to surrounding tiles
    public float ichorDropRadius = 2;

    public CreatureObject creatureData;
    public bool inWilderness = false; //Creatures have dif behavior depending on where they are. This is changed by the Wilderness Manager
    public Transform patrolPoint; //Creature will patrol this area instead of their wander behavior
    public bool persistAfterNewDay = false; //If true, will persist when the day transition occurs

    [HideInInspector] public StructureManager structManager;
    [HideInInspector] public CreatureEffectsHandler effectsHandler;
    [HideInInspector] public Transform player;

    public Collider[] allColliders; //to be disabled when a corpse
    public Transform corpseParticleTransform, knifeLodgeTransform;
    public CorpseParticleType corpseType;

    public Rigidbody rb;
    public Animator anim;

    public InventoryItemData[] droppedItems;
    public float[] dropChance;
    [Header("Sight Variables")]
    public float sightRange = 20; //how far can it see the player
    public float attackRange = 6;
    public bool playerInSightRange = false;
    public bool playerInAttackRange = false;

    //Vulnerabilities
    [Header("Vulnerabilities")]
    public bool shovelVulnerable = true; //More like physical attack vulnerable
    public bool fireVulnerable = true;
    public bool bearTrapVulnerable = true;
    public bool frostVulnerable = true;

    public bool isDead = false;
    bool corpseDestroyed = false;
    public int damageToStructure; //number must be positive
    public int damageToPlayer; //number must be negative
    public bool canCorpseBreak;
    public float actionSpeedMod = 1; //Dictates the speed of specific interactions per creature

    List <Material> allMats = new List<Material>();
    List <Color> allMatColors = new List<Color>();
    bool flashing = false;
    public Color hitColor;

    public List<StatusEffect> currentEffects = new List<StatusEffect>();

    public void Start()
    {
        structManager = StructureManager.Instance;
        effectsHandler = GetComponentInChildren<CreatureEffectsHandler>();
        player = PlayerInteraction.Instance.playerFeet;

        if(hitColor != Color.black)
        {
            SkinnedMeshRenderer[] allChildRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            for(int i = 0; i < allChildRenderers.Length; i++)
            {
                foreach(Material mat in allChildRenderers[i].materials)
                {
                    mat.EnableKeyword("_EMISSION");
                    allMats.Add(mat);
                    allMatColors.Add(mat.GetColor("_EmissionColor"));
                }
            }
        }
    }

    // Update is called once per frame
    public void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        print("Ouch");
        if(StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.Dare, this) && damage > 0) damage *= 1.5f;
        health -= damage;
        if(!flashing && hitColor != Color.black) StartCoroutine(DamageFlash());
        if(!isDead) OnDamage();
        else OnCorpseDamage();
        if(health <= 0 && !isDead) //turns into a corpse, and fertilizes nearby crops
        {
            effectsHandler.OnDeath();
            OnDeath();
            RefreshEmmision();
            isDead = true;

            //Send event of death for quests
            QuestManager.Instance.CreatureDeath(creatureData);
        }
        if(canCorpseBreak)
        {
            if(health <= corpseHealth && isDead && !corpseDestroyed)
            {
                corpseDestroyed = true;
                for(int i = 0; i < droppedItems.Length; i++)
                {
                    if(Random.Range(0f,10f) < dropChance[i])
                    {
                        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(droppedItems[i]);
                        Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
                        if(corpseParticleTransform) droppedItem.transform.position = new Vector3(corpseParticleTransform.position.x, corpseParticleTransform.position.y + 0.5f, corpseParticleTransform.position.z);
                        else droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                        Vector3 dir3 = Random.onUnitSphere;
                        dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                        itemRB = droppedItem.GetComponent<Rigidbody>();
                        itemRB.AddForce(dir3 * 20);
                        itemRB.AddForce(Vector3.up * 50);
                    }
                }
                if(ichorWorth > 0)
                {
                    if(corpseParticleTransform) structManager.IchorRefill(corpseParticleTransform.position, ichorWorth, ichorDropRadius);
                    else structManager.IchorRefill(transform.position, ichorWorth, ichorDropRadius);
                }
                GameObject corpseParticle = ParticlePoolManager.Instance.GrabCorpseParticle(corpseType);
                if(corpseParticle)
                {
                    if(corpseParticleTransform) corpseParticle.transform.position = corpseParticleTransform.position;
                    else corpseParticle.transform.position = transform.position;
                }
                if(Tutorial.Instance) Tutorial.Instance.ClearedCorpse();
                Destroy(this.gameObject);
            }
        }
        
    }

    public virtual void TakeDamage(float damage, Vector3 source)
    {
        TakeDamage(damage);
        //For stuff like golem and buzzsaw bot. Will need to be called from the shovel behavior script at least
    }

    public void PlayHitParticle(Vector3 pos) //pass (0,0,0) for it to use its own transform instead
    {
        GameObject bloodParticle;
        if(corpseType == CorpseParticleType.Red) 
        {
            bloodParticle = ParticlePoolManager.Instance.GrabBloodDropParticle();
        }
        else if(corpseType == CorpseParticleType.Slime) 
        {
            bloodParticle = ParticlePoolManager.Instance.GrabSlimeSplashParticle();
        }
        if(corpseType == CorpseParticleType.Corrupted) 
        {
            bloodParticle = ParticlePoolManager.Instance.GrabCorruptBloodDropParticle();
        }
        else return;

        if(pos == new Vector3(0,0,0))
        {
            if(corpseParticleTransform) pos = corpseParticleTransform.position;
            else pos = transform.position;
        }
        bloodParticle.transform.position = pos;
    }

    public virtual void OnDamage(){} //Triggers creature specific effects
    public virtual void OnCorpseDamage(){}
    public virtual void OnDeath()
    {
        if(NightSpawningManager.Instance.allCreatures.Contains(this)) NightSpawningManager.Instance.allCreatures.Remove(this);
        if(WildernessManager.Instance.allCreatures.Contains(this)) WildernessManager.Instance.allCreatures.Remove(this);
        foreach(Collider collider in allColliders)
        {
            collider.isTrigger = true;
        }
        if(creatureData)
        {
            creatureData.amountKilled++;
            creatureData.hasSpawned = true;
        }

        if(Tutorial.Instance) Tutorial.Instance.KillCreature();
    } //Triggers creature specific effects

    public void OnDestroy()
    {
        if(NightSpawningManager.Instance.allCreatures.Contains(this))NightSpawningManager.Instance.allCreatures.Remove(this);
        if(WildernessManager.Instance.allCreatures.Contains(this)) WildernessManager.Instance.allCreatures.Remove(this);
    }

    public virtual void OnSpawn(){}
    public virtual bool OnStun(float duration)
    {
        return false;
    }
    public virtual bool OnBearTrapStun(StructureBehaviorScript b)
    {
        return false;
    }

    public virtual void EnteredFireRadius(FireFearTrigger fireSource, out bool fearSuccessful)
    {
        fearSuccessful = false;
    }

    public virtual void HitWithWater()
    {
        if(StatusEffectManager.Instance.FindStatusOnCreature(StatusEffectName.Frost, this))
        {
            TakeDamage(25);
            ParticlePoolManager.Instance.GrabFrostBurstParticle().transform.position = transform.position;
        }
    }

    public virtual void NewPriorityTarget(StructureBehaviorScript newStruct){}

    public virtual void ToolInteraction(ToolType tool, out bool success)
    {
        success = false;
    }

    public virtual void FogTeleport(){}

    public virtual void NearLaventLeaf(Vector3 pos){}

    public StructureBehaviorScript CheckForObstacle(Transform checkTransform)
    {
        RaycastHit hit;
        if (Physics.Raycast(checkTransform.position, checkTransform.forward, out hit, 3, 1 << 6))
        {
            StructureBehaviorScript obstacle = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if(obstacle && obstacle.isObstacle) return obstacle;
            else return null;
        }
        else return null;
    }

    public bool CheckForPlayer(Transform checkTransform)
    {
        RaycastHit hit;
        if (Physics.Raycast(checkTransform.position, checkTransform.forward, out hit, 8, 1 << 10))
        {
            return true;
        }
        else return false;
    }

    IEnumerator DamageFlash()
    {
        flashing = true;
        for(int t = 0; t < 2; t++)
        {
            for(int i = 0; i < allMats.Count; i++)
            {
                allMats[i].SetColor("_EmissionColor", hitColor);
            }
            yield return new WaitForSeconds(0.1f);
            for(int i = 0; i < allMats.Count; i++)
            {
                allMats[i].SetColor("_EmissionColor", allMatColors[i]);
            }
            yield return new WaitForSeconds(0.1f);
        }
        flashing = false;
    }

    void RefreshEmmision()
    {
        for(int i = 0; i < allMats.Count; i++)
        {
            allMats[i].SetColor("_EmissionColor", allMatColors[i]);
        }
    }

    public Vector3 PointAroundPatrolPoint(float radius)
    {
        Vector2 randomDirection = Random.insideUnitCircle * radius;
        Vector3 randomPoint = new Vector3(randomDirection.x, patrolPoint.position.y, randomDirection.y) + patrolPoint.position;
        return randomPoint;
    }

    public void ApplyStatusEffect(StatusEffectObject status, int duration)
    {
        if(currentEffects.Count == 0)
        {
            currentEffects.Add(new StatusEffect(status, duration));
            currentEffects[currentEffects.Count - 1].effect.OnEffectApplied(this);
            GameObject vfx = StatusEffectManager.Instance.GrabStatusVFX(status.name);
            if(vfx == null) return;
            VFXStatusObject vfxObj = vfx.GetComponent<VFXStatusObject>();
            if(vfxObj == null) return;
            vfxObj.afflictedCreature = this;
            if(corpseParticleTransform)
            {
                vfxObj.followTransform = corpseParticleTransform;
                //vfx.transform.position = corpseParticleTransform.position;
                //vfx.transform.parent = corpseParticleTransform;
            } 
            else
            {
                vfxObj.followTransform = transform;
                //vfx.transform.position = transform.position;
                //vfx.transform.parent = transform;
            }
            return;
        }
        for(int x = 0; x < currentEffects.Count; x++)
        {
            //do the effects referencing the scriptable object here
            if(currentEffects[x].effect.name == status.name)
            {
                if(currentEffects[x].remainingDuration < duration) currentEffects[x].remainingDuration = duration;
                return;
            }
        }
    }

    public virtual bool CaughtByBugNet(out InventoryItemData item)
    {
        item = null;
        return false;
    }

    public void HitStructureParticle(Vector3 hitPoint)
    {
        Vector3 origin;
        if(corpseParticleTransform) origin = corpseParticleTransform.position;
        else origin = transform.position;
        Vector3 direction = (hitPoint - origin).normalized;
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, 20, 1 << 6))
        {
            ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hit.point;
            print("Played");
        }
    }

    public Transform GrabKnifeParent()
    {
        if(knifeLodgeTransform) return knifeLodgeTransform;
        else return corpseParticleTransform;
    }


    
}
