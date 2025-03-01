using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeralHareTest : CreatureBehaviorScript
{
    public Variant variant; // what variant of creature is this?

    public List<CropData> desiredCrops; // what crops does this creature want to eat

    FarmLand foundFarmTile;

    Vector3 jumpPos, startingDestination;
    bool isFleeing = false;
    bool jumpCooldown = false;
    bool isDigging = false;
    bool inEatingRange = false;
    bool isStunned = false;
    bool burrowCooldown = false;
    float eatingTimeLeft = 5f; // how many seconds does it take to eat a crop
    float fleeTimeLeft = 0;
    float yOrigin;
    float diggingTimeLeft = 0;

    float extendedSightRange;

    Vector3 despawnPos;

    Transform targetBurrow, exitBurrow;

    public GameObject burrow;
    Vector3 newBurrowPos;

    [Header("Albino Variables")]

    int burstJumps = 3; //How many attacks in quick succession the hare can do

    public GameObject cooldownEffect;

    public Collider attackCollider;
    bool attackingPlayer = false;

    public enum CreatureState
    {
        Wander,
        MoveTowardsCrop,
        Eat,
        FleeFromPlayer,
        Stunned,
        Dead,
        MakingBurrow
        //AttackPlayer,
        //AttackCooldown
    }

    public enum Variant
    {
        Normal,
        Albino
    }

    public CreatureState currentState;

    public LayerMask obstacleMask;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        currentState = CreatureState.Wander;
        if(variant == Variant.Normal && !inWilderness) StartCoroutine(CropCheck());
        despawnPos = NightSpawningManager.Instance.despawnPositions[Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length)].position;
        yOrigin = transform.position.y;
        StartCoroutine(IdleSoundTimer());

        if(attackCollider) attackCollider.enabled = false;

        if(!inWilderness && Random.Range(0,10) > 4)
        {
            exitBurrow = StructureManager.Instance.FindBurrow(false, transform.position);
            if(exitBurrow != null)
            {
                EnterBurrow();
                if(variant == Variant.Normal) //Immediately find crop
                {

                    List<FarmLand> availableLands = new List<FarmLand>();
                    foreach (StructureBehaviorScript structure in structManager.allStructs)
                    {
                        FarmLand potentialFarmTile = structure as FarmLand;
                        if (potentialFarmTile && desiredCrops.Contains(potentialFarmTile.crop) && Vector3.Distance(transform.position, potentialFarmTile.transform.position) < 25);
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
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        if(isDead) return;

        // Check distance from the player
        if(currentState != CreatureState.FleeFromPlayer)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            playerInSightRange = distance <= sightRange;
        } 

        // If the player is in sight, switch to flee state
        if (playerInSightRange && currentState != CreatureState.Eat && variant == Variant.Normal)
        {
            fleeTimeLeft = 1;
            currentState = CreatureState.FleeFromPlayer;
        }
        if (!isStunned)
        {
            CheckState(currentState);
        }

        if(startingDestination != new Vector3(0,0,0) && Vector3.Distance(startingDestination, transform.position) < 3) startingDestination = new Vector3(0,0,0);

        if(fleeTimeLeft > 0)
        {
            fleeTimeLeft -= Time.deltaTime;
        }

        if(inEatingRange && foundFarmTile)
        {
            float distance = Vector3.Distance(foundFarmTile.transform.position, transform.position);
            if(distance > 2.5f) inEatingRange = false;
        }
    }

    private void CheckState(CreatureState state)
    {
        switch (state)
        {
            case CreatureState.Wander:
                Wander();
                break;
            case CreatureState.MoveTowardsCrop:
                MoveTowardsCrop();
                break;
            case CreatureState.Eat:
                Eat();
                break;
            case CreatureState.FleeFromPlayer:
                FleeFromPlayer();
                break;
            case CreatureState.Stunned:
                Stunned();
                break;
            case CreatureState.Dead:
                //Dead();
                break;
            case CreatureState.MakingBurrow:
                MakeBurrow();
                break;
        }
    }

    public override void OnSpawn()
    {
        if(!inWilderness) startingDestination = StructureManager.Instance.GetRandomTile();
    }

    private void Wander()
    {
        if (!jumpCooldown)
        {
            StartCoroutine(JumpCooldownTimer());
            if(variant == Variant.Albino) //Seek the player
            {
                Hop(player.position);
                if(playerInSightRange) burstJumps--;
            }
            else
            {
                if(startingDestination != new Vector3(0,0,0)) Hop(startingDestination);
                else Hop(jumpPos);
            }
        }
        float r = Random.Range(0, 10f);
        if (r > 2 && foundFarmTile)
        { 
            currentState = CreatureState.MoveTowardsCrop;
        }
        if(targetBurrow) targetBurrow = null;
    }

    private void MoveTowardsCrop()
    {
        if (foundFarmTile && foundFarmTile.crop)
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
                currentState = CreatureState.Eat;
            }
        }
        else
        {
            foundFarmTile = null;
            currentState = CreatureState.Wander;
        }
    }

    private void Eat()
    {
        if (!isDigging)
        {
            isDigging = true;
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
        if (playerInSightRange || fleeTimeLeft > 0)
        {
            if (!jumpCooldown)
            {
                StartCoroutine(JumpCooldownTimer());

                //Look for nearest
                if(!targetBurrow && StructureManager.Instance.BurrowCount() > 1)
                {
                    Collider[] hitColliders = Physics.OverlapSphere(transform.position, 20f, 1 << 6);
                    foreach(Collider collider in hitColliders)
                    {
                        Burrow burrow = collider.gameObject.GetComponentInParent<Burrow>();
                        if(burrow)
                        {
                            targetBurrow = burrow.transform;
                            break;
                        }
                    }
                }
                exitBurrow = StructureManager.Instance.FindBurrow(true, transform.position);
                if(StructureManager.Instance.BurrowCount() < 2 || exitBurrow == null) targetBurrow = null;

                if(targetBurrow) Hop(targetBurrow.position);
                else Hop(jumpPos);
                if(foundFarmTile) foundFarmTile = null;
            }
        }
        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= (sightRange + 8);

        if(!playerInSightRange && fleeTimeLeft <= 0)
        {
            currentState = CreatureState.Wander;
            targetBurrow = null;
        }
    }

    void MakeBurrow()
    {
        if (!isDigging)
        {
            isDigging = true;
            newBurrowPos = StructureManager.Instance.CheckTile(transform.position);
            if(newBurrowPos == new Vector3(0,0,0))
            {
                currentState = CreatureState.Wander;
                return;
            }
            StartCoroutine(MakeBurrowCoroutine());
        }


        var lookPos = newBurrowPos - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);

        diggingTimeLeft -= Time.deltaTime;

    }

    IEnumerator MakeBurrowCoroutine()
    {
        anim.SetBool("IsDigging", true);
        effectsHandler.MiscSound();
        diggingTimeLeft = 3;
        yield return new WaitUntil(() => diggingTimeLeft <= 0 || playerInSightRange);
        if (!playerInSightRange)
        {
            StructureManager.Instance.SpawnStructure(burrow, newBurrowPos);
        }
        anim.SetBool("IsDigging", false);
        if(currentState == CreatureState.MakingBurrow) currentState = CreatureState.Wander;
        isDigging = false;
    }

    private void Stunned()
    {
        StartCoroutine(Stun(3));
    }

    public override void OnDeath()
    {
        base.OnDeath();
        anim.SetTrigger("IsDead");
        rb.isKinematic = true;
        if(cooldownEffect) cooldownEffect.SetActive(false);
    }

    // CropCheck Coroutine to search for crops periodically
    IEnumerator CropCheck()
    {
        yield return new WaitForSeconds(2);
        do
        {
            yield return new WaitForSeconds(10);
            if(StructureManager.Instance.CheckTile(transform.position) != new Vector3(0,0,0) && Random.Range(0,10) > 7 && StructureManager.Instance.BurrowCount() < 5 && currentState == CreatureState.Wander)
            {
                currentState = CreatureState.MakingBurrow;
            }
            else
            {
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
            }
        } while (gameObject.activeSelf);
    }

    public void Hop(Vector3 destination)
    {
        if(burrowCooldown)
        {
            burrowCooldown = false;
            SearchWanderPoint();
            return;
        }

        if(variant == Variant.Albino) rb.velocity = new Vector3(0,0,0);

        bool obstructed = false;
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        if(TimeManager.Instance.isDay && !inWilderness)
        {
            destination = despawnPos;
        }
        if(CheckForObstruction()) //randomizes jump is running into a wall/tree
        {
            //print("Obstruction Detected");
            destination = jumpPos;
            obstructed = true;
        }
        // hare will jump toward a random direction using physics, using rb.addforce to a random vector3 position in addition to a vector3.up force
        Vector3 jumpDirection = (transform.position - destination).normalized;
        jumpDirection *= -1f;

        if (currentState == CreatureState.FleeFromPlayer && !obstructed && !targetBurrow) //will flee from player instead
        {
            jumpDirection = (transform.position - player.position).normalized;
            destination = new Vector3(transform.position.x + jumpDirection.x, yOrigin, transform.position.z + jumpDirection.z);
            anim.SetBool("IsDigging", false);
        }

        // Apply force to make the hare hop
        float r = Random.Range(170, 210f);
        float v = 80; //vertical height
        if(playerInSightRange)
        {
            r += 150;
            v = 40;
        }
        rb.AddForce(Vector3.up * v);
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

        if(variant == Variant.Albino)
        {
            attackCollider.enabled = true;
            attackingPlayer = true;
        }

        float time = Random.Range(0.9f, 1.3f);
        if(currentState == CreatureState.FleeFromPlayer) time = time / 2.7f;
        if(variant == Variant.Albino && playerInSightRange) time =  0.4f;
        yield return new WaitForSeconds(time);

        if(variant == Variant.Albino)
        {
            attackCollider.enabled = false;
            attackingPlayer = false;
        }

        if(burstJumps <= 0 && variant == Variant.Albino)
        {
            cooldownEffect.SetActive(true);
            yield return new WaitForSeconds(Random.Range(3, 5));
            cooldownEffect.SetActive(false);
            burstJumps = Random.Range(3,5);
        }
        jumpCooldown = false;
    }

    IEnumerator Stun(int stunduration)
    {
        isStunned = true;
        yield return new WaitForSeconds(stunduration);
        isStunned = false;
        currentState = CreatureState.Wander;
    }

    IEnumerator EatCrop()
    {
        inEatingRange = true;
        anim.SetBool("IsDigging", true);
        effectsHandler.MiscSound();
        eatingTimeLeft = 7f;
        transform.LookAt(foundFarmTile.transform.position);
        yield return new WaitUntil(() => !inEatingRange || eatingTimeLeft <= 0 || foundFarmTile == null || foundFarmTile.crop == null || currentState != CreatureState.Eat);
        if (inEatingRange && foundFarmTile && foundFarmTile.crop && currentState == CreatureState.Eat)
        {
            if(Random.Range(0, 10) > 5 && StructureManager.Instance.BurrowCount() < 20)
            {
                Vector3 pos = foundFarmTile.transform.position;
                Destroy(foundFarmTile.gameObject);
                StructureManager.Instance.SpawnStructure(burrow, StructureManager.Instance.GetTileCenter(pos));
            }
            else
            {
                foundFarmTile.CropDestroyed();
            }
            foundFarmTile = null;
            inEatingRange = false;
            effectsHandler.MiscSound2();
            health = maxHealth;
        }
        anim.SetBool("IsDigging", false);
        isDigging = false;
        if(currentState == CreatureState.Eat) currentState = CreatureState.Wander;
    }

    public override void OnDamage()
    {
        if(currentState != CreatureState.Stunned && health > 0 && variant != Variant.Albino)
        {
            //if(currentState == CreatureState.Eat && health != maxHealth) TakeDamage(20);
            //else
            //{
                fleeTimeLeft = 3.5f;
                currentState = CreatureState.FleeFromPlayer;
            //}
        } 
        effectsHandler.OnHit();
    }

    IEnumerator IdleSoundTimer()
    {
        while(health > 0)
        {
            int i = Random.Range(7,16);
            effectsHandler.RandomIdle();
            yield return new WaitForSeconds(i);
        }
    }

    bool CheckForObstruction()
    {
        //if (CheckForObstacle(transform) != null) return true;
        Vector3 checkPos = new Vector3(transform.position.x, transform.position.y + 0.2f, transform.position.z);

        RaycastHit hit;
        if (Physics.Raycast(checkPos, transform.forward, out hit, 5, obstacleMask))
        {
            StructureBehaviorScript obstacle = hit.collider.GetComponentInParent<StructureBehaviorScript>(); //To check if its a tree because trees arent "obstacles"
            if(obstacle)
            {
                if(obstacle.GetComponent<FarmTree>() || obstacle.isObstacle) return true;
                else return false;
            }
            if(hit.collider) return true;
            else return false;
        }
        else return false;
    }

    void EnterBurrow()
    {
        burrowCooldown = true;
        

        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;

        fleeTimeLeft = 0;
        rb.velocity = new Vector3(0,0,0);

        transform.position = exitBurrow.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        targetBurrow = null;
        exitBurrow = null;

        startingDestination = new Vector3(0,0,0);
        if(currentState == CreatureState.FleeFromPlayer) currentState = CreatureState.Wander;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (attackingPlayer && other.CompareTag("Player") && !isDead)
        {
            PlayerInteraction.Instance.StaminaChange(damageToPlayer);
            attackCollider.enabled = false;
        }

        if(other.transform == targetBurrow && exitBurrow)
        {
            EnterBurrow();
        }
    }

}
