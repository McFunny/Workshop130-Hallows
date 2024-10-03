using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureBehaviorScript : MonoBehaviour
{
    //This is the base class that ALL creatures should derive from
    public float health = 10;
    public float maxHealth = 10;

    [HideInInspector] public StructureManager structManager;
    [HideInInspector] public CreatureEffectsHandler effectsHandler;
    [HideInInspector] public Transform player;

    public Rigidbody rb;
    public Animator anim;

    public float sightRange = 4; //how far can it see the player
    public bool playerInSightRange = false;

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

    
}
