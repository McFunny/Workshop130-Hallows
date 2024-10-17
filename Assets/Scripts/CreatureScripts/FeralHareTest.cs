using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeralHareTest : CreatureBehaviorScript
{
    public List<CropData> desiredCrops; // what crops does this creature want to eat

    FarmLand foundFarmTile;

    Vector3 jumpPos;
    bool isFleeing = false;
    bool jumpCooldown = false;
    bool isEating = false;
    bool inEatingRange = false;
    bool isStunned = false;
    float eatingTimeLeft = 5f; // how many seconds does it take to eat a crop

    public enum HareStates
    {
        Wander,
        MoveTowardsCrop,
        Eat,
        FleeFromPlayer,
        Stunned,
        Dead
    }

    public HareStates currentState;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        currentState = HareStates.Wander;
        StartCoroutine(CropCheck());
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if(isDead) return;

        // Check distance from the player
        if(currentState != HareStates.FleeFromPlayer)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            playerInSightRange = distance <= sightRange;
        } 

        // If the player is in sight, switch to flee state
        if (playerInSightRange)
        {
            currentState = HareStates.FleeFromPlayer;
        }
        if (!isStunned)
        {
            CheckState(currentState);
        }
    }

    private void CheckState(HareStates state)
    {
        switch (state)
        {
            case HareStates.Wander:
                Wander();
                break;
            case HareStates.MoveTowardsCrop:
                MoveTowardsCrop();
                break;
            case HareStates.Eat:
                Eat();
                break;
            case HareStates.FleeFromPlayer:
                FleeFromPlayer();
                break;
            case HareStates.Stunned:
                Stunned();
                break;
            case HareStates.Dead:
                //Dead();
                break;
        }
    }

    private void Wander()
    {
        if (!jumpCooldown)
        {
            StartCoroutine(JumpCooldownTimer());
            Hop(jumpPos);
        }
        float r = Random.Range(0, 10f);
        if (r > 2 && foundFarmTile)
        { 
            currentState = HareStates.MoveTowardsCrop;
        }
    }

    private void MoveTowardsCrop()
    {
        if (foundFarmTile)
        {
            float distance = Vector3.Distance(foundFarmTile.transform.position, transform.position);
            if (distance > 1.5f)
            {
                if (!jumpCooldown)
                {
                    StartCoroutine(JumpCooldownTimer());
                    Hop(foundFarmTile.transform.position);
                }
            }
            else
            {
                currentState = HareStates.Eat;
            }
        }
    }

    private void Eat()
    {
        if (!isEating)
        {
            isEating = true;
            StartCoroutine(EatCrop());
        }

        if (inEatingRange && eatingTimeLeft > 0 && foundFarmTile)
        {
            var lookPos = foundFarmTile.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);

            eatingTimeLeft -= Time.deltaTime;
        }
    }

    private void FleeFromPlayer()
    {
        if (playerInSightRange)
        {
            if (!jumpCooldown)
            {
                StartCoroutine(JumpCooldownTimer());
                Hop(jumpPos);
                if(foundFarmTile) foundFarmTile = null;
            }
        }
        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= (sightRange + 6);

        if(!playerInSightRange) currentState = HareStates.Wander;
    }

    private void Stunned()
    {
        StartCoroutine(Stun(3));
    }

    public override void OnDeath()
    {
        anim.SetTrigger("IsDead");
    }

    // CropCheck Coroutine to search for crops periodically
    IEnumerator CropCheck()
    {
        yield return new WaitForSeconds(2);
        do
        {
            yield return new WaitForSeconds(10);
            if ((foundFarmTile && foundFarmTile.crop) || structManager.allStructs.Count == 0)
            {
                yield return new WaitForSeconds(5);
            }
            else
            {
                List<FarmLand> availableLands = new List<FarmLand>();
                foreach (StructureBehaviorScript structure in structManager.allStructs)
                {
                    FarmLand potentialFarmTile = structure as FarmLand;
                    if (potentialFarmTile && desiredCrops.Contains(potentialFarmTile.crop))
                    {
                        availableLands.Add(potentialFarmTile);
                    }
                }
                if (availableLands.Count > 0)
                {
                    int r = Random.Range(0, availableLands.Count);
                    foundFarmTile = availableLands[r];
                }
            }
        } while (gameObject.activeSelf);
    }

    public void Hop(Vector3 destination)
    {
        // hare will jump toward a random direction using physics, using rb.addforce to a random vector3 position in addition to a vector3.up force
        Vector3 jumpDirection = (transform.position - destination).normalized;
        jumpDirection *= -1;

        if (playerInSightRange)
        {
            jumpDirection = (transform.position - player.position).normalized;
            destination = new Vector3(transform.position.x + jumpDirection.x, transform.position.y, transform.position.z + jumpDirection.z);
            anim.SetBool("IsDigging", false);
        }

        // Apply force to make the hare hop
        float r = Random.Range(170, 210f);
        rb.AddForce(Vector3.up * 100);
        rb.AddForce(jumpDirection * r);
        transform.LookAt(destination);

        anim.SetTrigger("IsHopping");

        SearchWanderPoint();

        effectsHandler.OnMove(0.8f);
    }

    public void SearchWanderPoint()
    {
        float x = Random.Range(-5f, 5f);
        float z = Random.Range(-5f, 5f);
        jumpPos = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);
    }

    IEnumerator JumpCooldownTimer()
    {
        jumpCooldown = true;
        float time = Random.Range(0.9f, 1.6f);
        yield return new WaitForSeconds(playerInSightRange ? time / 3 : time);
        jumpCooldown = false;
    }

    IEnumerator Stun(int stunduration)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunduration);
        isStunned = false;
        currentState = HareStates.Wander;
    }

    IEnumerator EatCrop()
    {
        inEatingRange = true;
        anim.SetBool("IsDigging", true);
        eatingTimeLeft = 5f;
        transform.LookAt(foundFarmTile.transform.position);
        yield return new WaitUntil(() => !inEatingRange || eatingTimeLeft <= 0 || foundFarmTile.crop == null || currentState != HareStates.Eat);
        if (inEatingRange && foundFarmTile.crop != null && currentState == HareStates.Eat)
        {
            foundFarmTile.CropDestroyed();
            foundFarmTile = null;
            inEatingRange = false;
        }
        anim.SetBool("IsDigging", false);
        isEating = false;
        currentState = HareStates.Wander;
    }
}
