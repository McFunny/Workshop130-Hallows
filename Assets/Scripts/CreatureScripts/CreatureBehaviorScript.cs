using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    [HideInInspector] public StructureManager structManager;
    [HideInInspector] public CreatureEffectsHandler effectsHandler;
    [HideInInspector] public Transform player;

    public Collider[] allColliders; //to be disabled when a corpse
    public Transform corpseParticleTransform;
    public CorpseParticleType corpseType;

    public Rigidbody rb;
    public Animator anim;

    public InventoryItemData[] droppedItems;
    public float[] dropChance;

    public float sightRange = 20; //how far can it see the player
    public float attackRange = 6;
    public bool playerInSightRange = false;
    public bool playerInAttackRange = false;
    public bool shovelVulnerable = true;
    public bool fireVulnerable = true;
    public bool bearTrapVulnerable = true;
    //public bool isTrapped = false;
    public bool isDead = false;
    bool corpseDestroyed = false;
    public int damageToStructure; //number must be positive
    public int damageToPlayer; //number must be negative
    public bool canCorpseBreak;

    List <Material> allMats = new List<Material>();
    List <Color> allMatColors = new List<Color>();
    bool flashing = false;
    public Color hitColor;

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
        health -= damage;
        if(!flashing && hitColor != Color.black) StartCoroutine(DamageFlash());
        if(!isDead)
        {
            OnDamage();
        }
        if(health <= 0 && !isDead)
        {
            effectsHandler.OnDeath();
            OnDeath();
            RefreshEmmision();
            isDead = true;
            //turns into a corpse, and fertilizes nearby crops
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
                        droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

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
                Destroy(this.gameObject);
            }
        }
        
    }

    public void PlayHitParticle(Vector3 pos) //pass (0,0,0) for it to use its own transform instead
    {
        if(corpseType == CorpseParticleType.Red) 
        {
            GameObject bloodParticle = ParticlePoolManager.Instance.GrabBloodDropParticle();
            if(pos == new Vector3(0,0,0))
            {
                if(corpseParticleTransform) pos = corpseParticleTransform.position;
                else pos = transform.position;
            }
            bloodParticle.transform.position = pos;
        }
    }

    public virtual void OnDamage(){} //Triggers creature specific effects
    public virtual void OnDeath()
    {
        if(NightSpawningManager.Instance.allCreatures.Contains(this)) NightSpawningManager.Instance.allCreatures.Remove(this);
        if(WildernessManager.Instance.allCreatures.Contains(this)) WildernessManager.Instance.allCreatures.Remove(this);
        foreach(Collider collider in allColliders)
        {
            collider.isTrigger = true;
        }
    } //Triggers creature specific effects

    public void OnDestroy()
    {
        if(NightSpawningManager.Instance.allCreatures.Contains(this))NightSpawningManager.Instance.allCreatures.Remove(this);
        if(WildernessManager.Instance.allCreatures.Contains(this)) WildernessManager.Instance.allCreatures.Remove(this);
    }

    public virtual void OnSpawn(){}
    public virtual void OnStun(float duration){}

    public virtual void EnteredFireRadius(FireFearTrigger fireSource, out bool fearSuccessful)
    {
        fearSuccessful = false;
    }

    public virtual void HitWithWater(){}

    public virtual void NewPriorityTarget(StructureBehaviorScript newStruct){}

    public virtual void ToolInteraction(ToolType tool, out bool success)
    {
        success = false;
    }

    public StructureBehaviorScript CheckForObstacle(Transform checkTransform)
    {
        RaycastHit hit;
        if (Physics.Raycast(checkTransform.position, checkTransform.forward, out hit, 2, 1 << 6))
        {
            StructureBehaviorScript obstacle = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if(obstacle && obstacle.isObstacle) return obstacle;
            else return null;
        }
        else return null;
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


    
}
