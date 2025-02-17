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


        startingAngle = hoe.eulerAngles;
        currentAngle = startingAngle;
        endingAngle = new Vector3(hoe.eulerAngles.x + 90, hoe.eulerAngles.y, hoe.eulerAngles.z); //change this to just adding 90 to the start angle. Reference Seed Shooter

        isTriggered = true;
        StartCoroutine(StartingCooldown());
    }

    void Start()
    {
        base.Start();
        ChangeRotation();
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
            currentAngle = new Vector3( Mathf.LerpAngle(currentAngle.x, endingAngle.x, lerp), currentAngle.y, currentAngle.z);

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
            currentAngle = new Vector3( Mathf.LerpAngle(currentAngle.x, startingAngle.x, lerp), currentAngle.y, currentAngle.z);

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

    void ChangeRotation() //Issue: This screws up the animation of it
    {
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) > 10) return;
        
        
        int r = Random.Range(0,4);

        switch(r)
        {
            case 0:
            break;
            case 1:
            transform.Rotate(0, 90, 0);
            break;
            case 2:
            transform.Rotate(0, 180, 0);
            break;
            case 3:
            transform.Rotate(0, 270, 0);
            break;
        }

        startingAngle = startingAngle + transform.eulerAngles;
        currentAngle = startingAngle;
        endingAngle = endingAngle + transform.eulerAngles;

        /*float rotation = player.eulerAngles.y; 

        //Debug.Log(rotation);

        if(rotation <= 45 || rotation >= 315)
        {
            newDirection = Direction.South;
            transform.rotation.y = 5;
        }

        else if(rotation >= 45 && rotation <= 135)
        {
            newDirection = Direction.East;
        }

        else if(rotation >= 135 && rotation <= 225)
        {
            newDirection = Direction.North;
        }

        else 
        {
            newDirection = Direction.West;
        } */

    }
}
