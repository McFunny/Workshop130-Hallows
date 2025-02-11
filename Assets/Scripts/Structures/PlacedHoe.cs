using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedHoe : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    Vector3 startingAngle;
    Vector3 currentAngle;
    Vector3 endingAngle;

    public Transform hoe;

    bool isTriggered;

    void Awake()
    {
        base.Awake();

        //add random starting rotation

        startingAngle = hoe.eulerAngles;
        currentAngle = startingAngle;
        endingAngle = new Vector3(hoe.eulerAngles.x + 90, 0, 0);

        isTriggered = true;
        StartCoroutine(StartingCooldown());
    }

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        base.Update();
    }

    IEnumerator StartingCooldown()
    {
        isTriggered = true;
        yield return new WaitForSeconds(0.7f);
        isTriggered = false;
    }

    IEnumerator Activate(Collider victim)
    {
        var player = victim.GetComponent<PlayerInteraction>();
        var creature = victim.GetComponentInParent<CreatureBehaviorScript>();
        if(player) PlayerMovement.restrictMovementTokens++;

        float lerp = 0;
        isTriggered = true;
        do
        {
            lerp += 0.1f;
            currentAngle = new Vector3( Mathf.LerpAngle(currentAngle.x, endingAngle.x, lerp), 0, -0);

            hoe.eulerAngles = currentAngle;


            yield return new WaitForSeconds(0.025f);
        }
        while(lerp < 1);

        audioHandler.PlaySound(audioHandler.activatedSound);
        
        if(player)
        {
            player.StaminaChange(-10);
            PlayerMovement.restrictMovementTokens--;
        } 
        else if(creature)
        {
            creature.TakeDamage(25);
            creature.PlayHitParticle(new Vector3(0, 0, 0));
        }

        yield return new WaitForSeconds(0.3f);

        do
        {
            lerp -= 0.1f;
            currentAngle = new Vector3( Mathf.LerpAngle(currentAngle.x, startingAngle.x, lerp), 0, -0);

            hoe.eulerAngles = currentAngle;


            yield return new WaitForSeconds(0.025f);
        }
        while(lerp > 0);

        yield return new WaitForSeconds(0.5f);
        
        isTriggered = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if(isTriggered) return;
        if(other.gameObject.layer == 9 || other.gameObject.layer == 10)
        {
            CreatureBehaviorScript creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if(creature && !creature.bearTrapVulnerable) return;
            isTriggered = true;
            StartCoroutine(Activate(other)); //pass enemy script or player script variable
        }
    }

    public override void StructureInteraction()
    {
        if(isTriggered) return;
        bool addedSuccessfully = PlayerInventoryHolder.Instance.AddToInventory(recoveredItem, 1);
        if (addedSuccessfully)
        {
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        //if (!gameObject.scene.isLoaded) return; 
    }
}
