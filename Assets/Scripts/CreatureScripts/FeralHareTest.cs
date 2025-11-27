using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeralHareTest : CreatureBehaviorScript
{
    public Variant variant; // what variant of creature is this?

    //public List<CropData> desiredCrops; // what crops does this creature want to eat
    public List<CropData> undesiredCrops; // what crops does this creature ignore

    public CropData carrotCrop;

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
    public ParticleSystem biteParticles;

    public Collider attackCollider;
    bool attackingPlayer = false;

    [Header("Corrupt Variables")]
    CorruptedTile cTile, patrolTile;
    public StructureObject nodeData, tileData;
    public ThingToMake newObject;
    public CropData purifyingCrop;

    public enum CreatureState
    {
        Wander,
        MoveTowardsCrop,
        Eat,
        FleeFromPlayer,
        Stunned,
        Dead,
        MakingBurrow,
        Run
        //AttackPlayer,
        //AttackCooldown
    }

    public enum Variant
    {
        Normal,
        Albino,
        Tunneler,
        Corrupt
    }

    public enum ThingToMake //corrupted hare only
    {
        Burrow,
        Node,
        Tile
    }

    public CreatureState currentState;

    public LayerMask obstacleMask;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        currentState = CreatureState.Wander;
        if(variant != Variant.Albino && !inWilderness) StartCoroutine(CropCheck());
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
                if(variant != Variant.Albino && variant != Variant.Corrupt) //Immediately find crop
                {
                    FindCrop();
                }
            }
        }

        if(MainMenuScript.currentFileMode == FileMode.Cozy) actionSpeedMod -= 0.1f;

        if(variant == Variant.Corrupt && !inWilderness)
        {
            StartCoroutine(RefreshCorruptPatrol());
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
        if (playerInSightRange && currentState != CreatureState.Eat && variant != Variant.Albino && variant != Variant.Corrupt)
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
            case CreatureState.Run:
                RunToTarget();
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
            if(variant == Variant.Albino || variant == Variant.Corrupt) //Seek the player
            {
                if(patrolPoint && !playerInSightRange)
                {
                    Vector3 hopPoint = PointAroundPatrolPoint(10);
                    hopPoint.y = 0;
                    Hop(hopPoint);
                }
                else Hop(player.position);
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

            eatingTimeLeft -= Time.deltaTime * actionSpeedMod;
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
                else
                {
                    SearchWanderPoint();
                    Hop(jumpPos);
                }

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

            if(variant == Variant.Corrupt)
            {
                if(newObject == ThingToMake.Node && cTile)  newBurrowPos = cTile.transform.position;
                else if(newObject == ThingToMake.Tile) newBurrowPos = StructureManager.Instance.CheckTile(transform.position);
                else newBurrowPos = new Vector3(0,0,0);
            }
            else
            {
                newBurrowPos = StructureManager.Instance.CheckTile(transform.position);
            }

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

        diggingTimeLeft -= Time.deltaTime * actionSpeedMod;

    }

    IEnumerator MakeBurrowCoroutine()
    {
        anim.SetBool("IsDigging", true);
        effectsHandler.MiscSound();
        diggingTimeLeft = 4;
        yield return new WaitUntil(() => diggingTimeLeft <= 0 || playerInSightRange);
        if (!playerInSightRange && (StructureManager.Instance.CheckTile(newBurrowPos) != new Vector3(0,0,0) || (newObject != ThingToMake.Node || (cTile != null && cTile.containedStructure == null))))
        {
            if(newObject == ThingToMake.Tile) StructureManager.Instance.SpawnStructure(tileData.objectPrefab, newBurrowPos);
            else 
            {
                if(cTile) cTile.containedStructure = StructureManager.Instance.SpawnStructureWithInstance(burrow, newBurrowPos).GetComponent<StructureBehaviorScript>();
                else StructureManager.Instance.SpawnStructure(burrow, newBurrowPos);
            }

            if(variant == Variant.Tunneler)
            {
                Transform burrow2 = StructureManager.Instance.SpawnStructureWithInstance(burrow, StructureManager.Instance.FindFreeTileNearCrop()).transform;
                transform.position = burrow2.position;
                FindCrop();
            }
        }
        anim.SetBool("IsDigging", false);
        cTile = null;
        if(currentState == CreatureState.MakingBurrow) currentState = CreatureState.Wander;
        isDigging = false;
    }

    bool CanMakeBurrow()
    {
        int burrowChance = Random.Range(0,10);
        if(variant == Variant.Tunneler) burrowChance += 5;
        if(variant == Variant.Corrupt) 
        {
            int currentNodes = StructureManager.Instance.TallyStructure(nodeData);
            float maxNodes = (CorruptionManager.Instance.corruptedTiles/10) + 1; //How many nodes can be present on the farm
            if(currentNodes >= maxNodes) burrowChance = 0;
            
            if(burrowChance > 4) 
            {
                Collider[] hitStructures = Physics.OverlapSphere(transform.position, 3f, 1 << 6);
                foreach(Collider collider in hitStructures)
                {
                    CorruptedTile foundTile = collider.gameObject.GetComponentInParent<CorruptedTile>();
                    if(foundTile && foundTile.containedStructure == null)
                    {
                        cTile = foundTile;
                        newObject = ThingToMake.Node;
                        return true; //Found a nearby empty tile
                    }
                }
            }

            
            burrowChance = Random.Range(0,10); //try again to plant a tile
            if(!patrolPoint) burrowChance += 2;
            if(structManager.CheckTile(transform.position) == Vector3.zero) return false;
            if(burrowChance >= 4 && structManager.ValidateGridType(transform.position, GridType.Farm)) 
            {
                newObject = ThingToMake.Tile;
                return true;
            }

            return false;
        }

        if(structManager.CheckTile(transform.position) == Vector3.zero) return false;
        if(burrowChance >= 7 && structManager.BurrowCount() < 15 && currentState == CreatureState.Wander && structManager.ValidateGridType(transform.position, GridType.Farm))
        {
            if(variant == Variant.Tunneler) return true;

            Collider[] hitStructures = Physics.OverlapSphere(transform.position, 5f, 1 << 6);
            foreach(Collider collider in hitStructures)
            {
                Burrow foundBurrow = collider.gameObject.GetComponentInParent<Burrow>();
                if(foundBurrow)
                {
                    return false; //Found a nearby burrow
                }
            }
            return true;
        }
        return false;
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
            if(CanMakeBurrow())
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
                        if (potentialFarmTile && !undesiredCrops.Contains(potentialFarmTile.crop) && !potentialFarmTile.rotted)
                        {
                            if(potentialFarmTile.crop != purifyingCrop && variant == Variant.Corrupt) continue;
                            availableLands.Add(potentialFarmTile);
                        }
                    }
                    if (availableLands.Count > 0)
                    {
                        float minDistance = 1000;
                        float dist;
                        FarmLand closestTile = availableLands[0];
                        for(int i = 0; i < availableLands.Count; i++)
                        {
                            if(Random.Range(0,10) > 7) continue;
                            dist = Vector3.Distance(transform.position, availableLands[i].transform.position);
                            if(dist < minDistance && (closestTile.crop != carrotCrop || availableLands[i].crop == carrotCrop))
                            {
                                minDistance = dist;
                                closestTile = availableLands[i];
                            }
                            if(availableLands[i].crop == carrotCrop && variant != Variant.Tunneler)
                            {
                                closestTile = availableLands[i];
                            }
                        }
                        foundFarmTile = closestTile;
                    }
                }
            }
        } while (gameObject.activeSelf);
    }

    IEnumerator RefreshCorruptPatrol()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(3);
            if(patrolTile == null || Random.Range(0,10) > 9)
            {
                CorruptionManager.Instance.ReturnFreeTileList(out List<CorruptedTile> cTiles);
                if(cTiles.Count == 0)
                {
                    patrolPoint = null;
                    continue;
                }

                patrolTile = cTiles[Random.Range(0, cTiles.Count)];
                patrolPoint = patrolTile.transform;
            }
        }
    }

    public void Hop(Vector3 destination)
    {
        if(burrowCooldown)
        {
            burrowCooldown = false;
            SearchWanderPoint();
            return;
        }

        if(variant == Variant.Albino || variant == Variant.Corrupt) rb.velocity = new Vector3(0,0,0);

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

        if (currentState == CreatureState.FleeFromPlayer && !obstructed && !targetBurrow && Vector3.Distance(transform.position, player.position) < 40) //will flee from player instead
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

        if(variant == Variant.Albino || variant == Variant.Corrupt)
        {
            attackCollider.enabled = true;
            attackingPlayer = true;
        }

        float time = Random.Range(0.9f, 1.3f);
        if(currentState == CreatureState.FleeFromPlayer) time = time / 2.7f;
        else time = time/actionSpeedMod;
        if((variant == Variant.Albino || variant == Variant.Corrupt) && playerInSightRange) time =  0.4f;
        yield return new WaitForSeconds(time);

        if(variant == Variant.Albino || variant == Variant.Corrupt)
        {
            attackCollider.enabled = false;
            attackingPlayer = false;
        }

        if(burstJumps <= 0 && (variant == Variant.Albino || variant == Variant.Corrupt))
        {
            cooldownEffect.SetActive(true);
            if(variant == Variant.Corrupt) yield return new WaitForSeconds(Random.Range(2, 3)/actionSpeedMod);
            else yield return new WaitForSeconds(Random.Range(3, 5)/actionSpeedMod);
            cooldownEffect.SetActive(false);
            burstJumps = Random.Range(3,5);
        }

        //Idle Anims
        if(currentState != CreatureState.FleeFromPlayer && !playerInSightRange && !foundFarmTile && !isDead && variant != Variant.Corrupt)
        {
            int r = Random.Range(0, 20);
            if(r < 1) //Stand Idle
            {
                anim.Play("RabbitStandUp");
                yield return new WaitForSeconds(2.5f);
            }
            else if(r < 3) //Thump Idle
            {
                anim.Play("RabbitThump");
                yield return new WaitForSeconds(2.2f);
            }
        }
        if(variant == Variant.Corrupt && Random.Range(0, 100) > 98 && currentState != CreatureState.FleeFromPlayer)
        {
            currentState = CreatureState.Run;
            anim.Play("RabbitStandRun");
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

    void FindCrop()
    {
        List<FarmLand> availableLands = new List<FarmLand>();
        foreach (StructureBehaviorScript structure in structManager.allStructs)
        {
            FarmLand potentialFarmTile = structure as FarmLand;
            if (potentialFarmTile && !undesiredCrops.Contains(potentialFarmTile.crop) && Vector3.Distance(transform.position, potentialFarmTile.transform.position) < 25 && 
            potentialFarmTile.currentUpgrade != FarmLand.FarmTileUpgrade.Corrupt)
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
            if(foundFarmTile.crop.behavior) 
            {
                if(foundFarmTile.harvestable)
                {
                    foundFarmTile.crop.behavior.OnConsumed(this);
                }
                else foundFarmTile.crop.behavior.OnConsumedBeforeMaturity(this);
                foundFarmTile.CropDestroyed();
            }
            else if(Random.Range(0, 10) > 5 && StructureManager.Instance.BurrowCount() < 20 && foundFarmTile.currentUpgrade != FarmLand.FarmTileUpgrade.Trellis)
            {
                Vector3 pos = foundFarmTile.transform.position;
                Destroy(foundFarmTile.gameObject);
                yield return new WaitForSeconds(0.2f);
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

    public override void OnCorpseDamage()
    {
        if(health <= 0 && canCorpseBreak)
        {
            anim.Play("DeathRecoil", -1, 0f);
        }
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
                if(obstacle.GetComponent<FarmTree>() || obstacle.GetComponent<Boulder>() || obstacle.isObstacle) return true;
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
        
        if(targetBurrow) targetBurrow.GetComponent<Burrow>().UseBurrow();
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;

        fleeTimeLeft = 0;
        rb.velocity = new Vector3(0,0,0);

        transform.position = exitBurrow.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        if(exitBurrow) exitBurrow.GetComponent<Burrow>().UseBurrow();

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
            biteParticles.Play();
        }

        if(other.transform == targetBurrow && exitBurrow)
        {
            EnterBurrow();
        }
    }

    void RunToTarget()
    {
        float maxSpeed = 12 * actionSpeedMod;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= 3)
        {
            rb.velocity = Vector3.zero;
            currentState = CreatureState.Wander;
            anim.Play("RabbitIdle");
            return;
        }

        // Move toward lagged target position
        Vector3 toTarget = player.position - transform.position;
        toTarget.y = 0;

        Vector3 direction = toTarget.normalized;

        Vector3 desiredVelocity = direction * maxSpeed;
        Vector3 velocityDelta = desiredVelocity - rb.velocity;
        velocityDelta.y = 0;

        Vector3 force = velocityDelta * 10;
        rb.AddForce(force, ForceMode.Acceleration);

        var rotation = Quaternion.LookRotation(toTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);
    }

}
