using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuzzsawBot : CreatureBehaviorScript
{
    public GameObject roboModel, roboParent; //Model constantly spins, parent tilts in direction it moves. The rotation to the sides should be between -15 and 15 for both axis

    public Vector3 targetPos; //The direction where the bot is trying to push itself to move to
    Vector3 despawnPos;

    public float accelerationRate, maxWanderVelocity, maxRecoilVelocity;

    float recoilTimeLeft = 0;

    public enum CreatureState
    {
        Wander,
        Recoil,
        Stunned,
        Dead
    }

    public CreatureState currentState;

    void Start()
    {
        base.Start();
        currentState = CreatureState.Wander;
        despawnPos = NightSpawningManager.Instance.despawnPositions[Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length)].position;

        StartCoroutine(RefreshWanderPoint());
    }

    void FixedUpdate()
    {
        base.Update();
        if(isDead) return;

        CheckState(currentState);

        //For rotating the model
        roboModel.transform.Rotate(0, 800 * Time.deltaTime, 0);

        //For tilting the parent
        Vector3 velocity = rb.velocity;

        // Ignore very small movement
        if (velocity.magnitude < 0.1f)
            return;

        // Calculate tilt angles (inverted to look more natural)
        float tiltX = Mathf.Clamp(-velocity.z, -1f, 1f) * 15;
        float tiltZ = Mathf.Clamp(velocity.x, -1f, 1f) * 15;

        // Build target tilt rotation (keeping Y from original)
        Quaternion targetTilt = Quaternion.Euler(-tiltX, transform.eulerAngles.y, -tiltZ);

        // Smoothly interpolate to the target rotation
        roboParent.transform.rotation = Quaternion.Lerp(roboParent.transform.rotation, targetTilt, Time.fixedDeltaTime * 5);
    }

    private void CheckState(CreatureState state)
    {
        switch (state)
        {
            case CreatureState.Wander:
                Wander();
                break;
            case CreatureState.Stunned:
                //Stunned();
                break;
            case CreatureState.Dead:
                //
                break;
            case CreatureState.Recoil:
                Recoil();
                break;
        }
    }

    void Wander()
    {
        //add force to direction, but have a cap
        Vector3 dir = (transform.position - targetPos).normalized;
        dir *= -1f;
        rb.AddForce(dir * (accelerationRate));

        LimitVelocity();
    }

    IEnumerator RefreshWanderPoint()
    {
        if(!inWilderness) targetPos = StructureManager.Instance.GetRandomTile();
        yield return new WaitForSeconds(9);
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(2, 4));

            if(TimeManager.Instance.isDay && !inWilderness)
            {
                targetPos = despawnPos;
                continue;
            }
            else if(inWilderness || Random.Range(0, 15) == 0)
            {
                targetPos = player.position;
                continue;
            }
            float x = Random.Range(-10f, 10f);
            float z = Random.Range(-10f, 10f);
            targetPos = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
        }
    }

    void Recoil()
    {
        if(recoilTimeLeft <= 0)
        {
            currentState = CreatureState.Wander;
        }
        recoilTimeLeft -= Time.deltaTime;
        LimitVelocity();
    }

    void LimitVelocity()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        float maxVelocity;
        if(currentState == CreatureState.Recoil) maxVelocity = maxRecoilVelocity;
        else maxVelocity = maxWanderVelocity;

        // Limit velocity if needed
        if (flatVel.magnitude > maxVelocity)
        {
            Vector3 limitedVel = flatVel.normalized * maxVelocity;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        bool performRecoil = false;
        GameObject hitObject = other.gameObject;
        //ContactPoint contact = collision.contacts[0];

        if(hitObject.layer == 9 || hitObject.layer == 10 || hitObject.layer == 6);
        else return;

        if(hitObject.layer == 10)
        {
            performRecoil = true;
            PlayerInteraction.Instance.StaminaChange(damageToPlayer);
        }

        StructureBehaviorScript obstacle = hitObject.GetComponentInParent<StructureBehaviorScript>(); //To check if its a tree because trees arent "obstacles"
        if(obstacle)
        {
            if(obstacle.GetComponent<FarmTree>() || obstacle.GetComponent<Boulder>() || obstacle.isObstacle)
            {
                performRecoil = true;
                HitStructureParticle(obstacle.transform.position);
                obstacle.TakeDamage(5);
            }
            else
            {
                FarmLand tile = obstacle as FarmLand;
                if(tile && tile.crop)
                {
                    tile.ToolInteraction(ToolType.Scythe, out bool success);
                    if(!success) 
                    {
                        if(tile.currentUpgrade == FarmLand.FarmTileUpgrade.Trellis || tile.currentUpgrade == FarmLand.FarmTileUpgrade.Stone)
                        {
                            performRecoil = true;
                            HitStructureParticle(obstacle.transform.position);
                            obstacle.TakeDamage(5);
                        }
                        else tile.CropDestroyed();
                    }
                }
                else return;
            }
        }

        var creature = hitObject.GetComponentInParent<CreatureBehaviorScript>();
        if (creature != null && creature.shovelVulnerable)
        {
            HitStructureParticle(creature.transform.position);
            performRecoil = true;
            creature.PlayHitParticle(creature.transform.position);
            if(creature as BuzzsawBot == null) creature.TakeDamage(15);
        }

        if(performRecoil)
        {
            currentState = CreatureState.Recoil;
            recoilTimeLeft = Random.Range(0.5f, 2f);
            Vector3 dir = (transform.position - hitObject.transform.position).normalized;
            rb.AddForce(dir * Random.Range(60, 100), ForceMode.Impulse);
            effectsHandler.MiscSound();
        }
        
    }

    public override void TakeDamage(float damage, Vector3 source)
    {
        TakeDamage(damage);
        currentState = CreatureState.Recoil;
        recoilTimeLeft = Random.Range(0.5f, 2f);
        Vector3 dir = (transform.position - source).normalized;
        rb.AddForce(dir * Random.Range(30, 70), ForceMode.Impulse);
        effectsHandler.MiscSound();
    }

    public override void HitWithWater()
    {
        ParticlePoolManager.Instance.GrabElecZapParticle().transform.position = transform.position;
        TakeDamage(25);
    }
}
