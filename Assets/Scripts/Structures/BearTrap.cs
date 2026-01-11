using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BearTrap : StructureBehaviorScript
{
    //public InventoryItemData recoveredItem;
    public Transform topClamp, bottomClamp;
    float animationTimeLeft;
    bool isTriggered, rearming, caughtSomething;

    public AudioClip triggeredSFX;

    Vector3 targetAngleTop = new Vector3(-161, 90, -90);
    Vector3 targetAngleBottom = new Vector3(-20, 90, -90);

    Vector3 currentAngleTop;
    Vector3 currentAngleBottom;

    Vector3 startingAngleTop;
    Vector3 startingAngleBottom;

    float stunTime = 5;

    CreatureBehaviorScript capturedCreature;

    Collider collider;

    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();

        collider = GetComponent<Collider>();
    }

    void Start()
    {
        currentAngleTop = topClamp.eulerAngles;
        currentAngleBottom = bottomClamp.eulerAngles;
        startingAngleTop = topClamp.eulerAngles;
        startingAngleBottom = bottomClamp.eulerAngles;
        if(isTriggered)
        {
            topClamp.rotation = Quaternion.Euler(-161, 90, -90);
            bottomClamp.rotation = Quaternion.Euler(-20, 90, -90);
        }
        
        if(TownGate.Instance.location == PlayerLocation.InWilderness) absentFromGrid = true;
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if(Tutorial.Instance) health = maxHealth;

        if(!caughtSomething) base.Update();

        if(animationTimeLeft > 0)
        {
            currentAngleTop = new Vector3( Mathf.LerpAngle(currentAngleTop.x, targetAngleTop.x, Time.deltaTime * 1f), 90, -90);

            topClamp.eulerAngles = currentAngleTop;

            currentAngleBottom = new Vector3( Mathf.LerpAngle(currentAngleBottom.x, targetAngleBottom.x, Time.deltaTime * 1f), 90, -90);

            bottomClamp.eulerAngles = currentAngleBottom;


            animationTimeLeft -= Time.deltaTime;

        }
    }

    public override void StructureInteraction()
    {
        if(isTriggered && !rearming)
        {
            StartCoroutine(Rearm());
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(caughtSomething) return;
        if(type == ToolType.Shovel && !rearming)
        {
            //StartCoroutine(DugUp());
            success = true;
        }
    }

    public override void DigAction()
    {
        if(!caughtSomething)
        {
            base.DigAction();
        }
        
    }

    IEnumerator SpringTrap(Collider victim)
    {
        animationTimeLeft = 0.01f; //old value was .2
        caughtSomething = true;
        yield return new WaitForSeconds(animationTimeLeft);
        topClamp.rotation = Quaternion.Euler(-161, 90, -90);
        bottomClamp.rotation = Quaternion.Euler(-20, 90, -90);
        audioHandler.PlaySound(triggeredSFX);

        //if(victim.gameObject.layer == 9) victim.transform.position = transform.position;
        Vector3 victimPos = new Vector3(victim.transform.position.x, transform.position.y, victim.transform.position.z);

        float distance = Vector3.Distance(victimPos, transform.position);
        //print(distance);
        if(victim/*distance < 1.5f*/)
        {
            PlayerInteraction player = victim.GetComponent<PlayerInteraction>();
            //does the damage
            if(player && distance < 1.5f && !player.TripCheck())
            {
                player.rb.velocity = Vector3.zero;
                player.StaminaChange(-25);

                //restrictplayermovement
                player.transform.position = new Vector3(transform.position.x, victim.transform.position.y, transform.position.z);
                PlayerMovement.restrictMovementTokens += 1;
                //yield return new WaitForSeconds(0.2f);

                PlayerCam.Instance.NewObjectOfInterest(transform.position);
                
                yield return new WaitForSeconds(1);
                StartCoroutine(Rearm());

                PlayerCam.Instance.ClearObjectOfInterest();

                yield return new WaitForSeconds(0.5f);
                PlayerMovement.restrictMovementTokens -= 1;
                //enable player movement

                TakeDamage(1);
            } 
            else
            {
                capturedCreature = victim.GetComponentInParent<CreatureBehaviorScript>();
                if(!capturedCreature)
                {
                    caughtSomething = false;
                    yield break;
                }
                //creature.isTrapped = true;
                if(capturedCreature.health >= 75)
                {
                    //stun and damage
                    StartCoroutine(HoldCreature());
                }
                else
                {
                    //kill
                    capturedCreature.transform.position = transform.position;
                    capturedCreature.TakeDamage(999);
                    TakeDamage(3);
                    capturedCreature.PlayHitParticle(new Vector3(0, 0, 0));
                    StartCoroutine(HoldCorpse());
                }
            }
        }
        caughtSomething = false;
        
    }

    IEnumerator Rearm()
    {
        float lerp = 0;
        rearming = true;
        audioHandler.PlaySound(audioHandler.activatedSound);
        do
        {
            lerp += 0.1f;
            currentAngleTop = new Vector3( Mathf.LerpAngle(currentAngleTop.x, startingAngleTop.x, lerp), 90, -90);

            topClamp.eulerAngles = currentAngleTop;

            currentAngleBottom = new Vector3( Mathf.LerpAngle(currentAngleBottom.x, startingAngleBottom.x, lerp), 90, -90);

            bottomClamp.eulerAngles = currentAngleBottom;
            yield return new WaitForSeconds(0.1f);
        }
        while(lerp < 1);
        topClamp.eulerAngles = startingAngleTop;
        bottomClamp.eulerAngles = startingAngleBottom;

        yield return new WaitForSeconds(1f);
        
        isTriggered = false;
        rearming = false;
    }

    IEnumerator HoldCreature() 
    {
        rearming = true;
        if(!capturedCreature.OnBearTrapStun(this) || !capturedCreature.bearTrapVulnerable)  capturedCreature = null;
        else
        {
            capturedCreature.transform.position = transform.position;
            capturedCreature.TakeDamage(25);
            capturedCreature.PlayHitParticle(new Vector3(0, 0, 0));
            yield return new WaitForSeconds(1f);
        }

        while(capturedCreature && health > 0 && capturedCreature.health > 0)
        {
            capturedCreature.transform.position = transform.position;
            yield return new WaitForSeconds(2f);
            if(capturedCreature.health > 0)
            {
                int damage = 0;
                int calculatedHealth = 0;
                while(calculatedHealth < capturedCreature.health)
                {
                    calculatedHealth += 25;
                    damage++;
                }

                TakeDamage(damage);
            }
        }

        StartCoroutine(HoldCorpse());
        //StartCoroutine(Rearm());

        ///////////////
        /*

        if(!capturedCreature.OnStun(2) || !capturedCreature.bearTrapVulnerable) capturedCreature = null;
        else
        {
            capturedCreature.transform.position = transform.position;
            capturedCreature.TakeDamage(25);
            capturedCreature.PlayHitParticle(new Vector3(0, 0, 0));
            yield return new WaitForSeconds(2f);
        }

        while(capturedCreature && health > 0 && capturedCreature.health > 0)
        {
            if(!capturedCreature.OnStun(2)) capturedCreature = null;
            else
            {
                capturedCreature.transform.position = transform.position;
                yield return new WaitForSeconds(2f);
                if(capturedCreature.health > 0) TakeDamage(1);
            }
        }

        StartCoroutine(HoldCorpse());
        //StartCoroutine(Rearm());
        */
    }

    IEnumerator HoldCorpse()
    {
        rearming = true;
        collider.enabled = false;
        while(capturedCreature) yield return null;
        rearming = false;
        collider.enabled = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if(isTriggered) return;
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
            }
            isTriggered = true;
            StartCoroutine(SpringTrap(other)); //pass enemy script or player script variable
        }
    }
}
