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

    [HideInInspector] public StructureManager structManager;
    [HideInInspector] public CreatureEffectsHandler effectsHandler;
    [HideInInspector] public Transform player;

    public Rigidbody rb;
    public Animator anim;

    public InventoryItemData[] droppedItems;
    public float[] dropChance;

    public float sightRange = 4; //how far can it see the player
    public bool playerInSightRange = false;
    public bool playerInAttackRange = false;
    public bool shovelVulnerable = true;
    public bool isTrapped = false;
    public bool isDead = false;
    bool corpseDestroyed = false;
    public int damageToStructure;
    public int damageToPlayer;
    public bool canCorpseBreak;

    public void Start()
    {
        structManager = StructureManager.Instance;
        effectsHandler = FindObjectOfType<CreatureEffectsHandler>();
        player = FindObjectOfType<PlayerInteraction>().transform;
    }

    // Update is called once per frame
    public void Update()
    {
        
    }

    public void TakeDamage(float damage)
    {
        print("Ouch");
        health -= damage;
        if(health <= 0 && !isDead)
        {
            effectsHandler.OnDeath();
            OnDeath();
            isDead = true;
            //turns into a corpse, and fertalizes nearby crops
        }
        else if(canCorpseBreak)
        {
            effectsHandler.OnHit();
            OnDamage();
            if(health < corpseHealth && isDead && !corpseDestroyed)
            {
                corpseDestroyed = true;
                for(int i = 0; i < droppedItems.Length; i++)
                {
                    if(Random.Range(0f,10f) < dropChance[i])
                    {
                        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(droppedItems[i]);
                        float x = Random.Range(-0.5f,0.5f);
                        float z = Random.Range(-0.5f,0.5f);
                        droppedItem.transform.position = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
                    }
                }
                Destroy(this.gameObject);
            }
        }
        
    }

    public virtual void OnDamage(){} //Triggers creature specific effects
    public virtual void OnDeath()
    {
        if(ichorWorth > 0) structManager.IchorRefill(transform.position, ichorWorth, ichorDropRadius);
        NightSpawningManager.Instance.allCreatures.Remove(this);
    } //Triggers creature specific effects

    public virtual void OnSpawn(){}


    
}
