using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviorScript : MonoBehaviour
{
    //This is the base class that ALL creatures should derive from
    public float health = 100;
    public float maxHealth = 100;

    [HideInInspector] public StructureManager structManager;
    [HideInInspector] public CreatureEffectsHandler effectsHandler;
    [HideInInspector] public Transform player;

    public Rigidbody rb;
    public Animator anim;

    public float sightRange = 4; //how far can it see the player
    public bool playerInSightRange = false;
    public bool shovelVulnerable = true;
    public bool isDead = false;

    public void Start()
    {
        structManager = FindObjectOfType<StructureManager>();
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
        if(health < 0)
        {
            effectsHandler.OnDeath();
            OnDeath();
            isDead = true;
            //turns into a corpse, and fertalizes nearby crops
        }
        else
        {
            effectsHandler.OnHit();
            OnDamage();
        }
        
    }

    public virtual void OnDamage(){} //Triggers creature specific effects
    public virtual void OnDeath(){} //Triggers creature specific effects


    
}
