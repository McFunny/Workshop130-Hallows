using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoneTurret : StructureBehaviorScript
{
    public Transform turretHead, bulletOrigin, seedSocket;

    float maxAmmo = 15; //Dont allow any more seeds to be added to the item list after there are this many entrants
    float range = 26; //Get a debug sphere to show the range
    bool targetInSight = false;
    bool shotCooldown;
    bool returningToCenter;
    float projectileSpeed = 270;
    float minimumDistance = 1;

    float lockOnTime = 0f;
    float requiredLockDuration = 0.3f; // seconds of continuous aim before firing

    bool hidden = false;
    bool transitioning = false;
    bool canTransition = true;
    public ParticleSystem leafParticles, poofParticle;
    public GameObject destroyedLeaves;
    public Transform activePos, inactivePos;

    public InventoryItemData boneItem;

    //Maybe add a large button on the back, where when the player interacts with this, it can be turned on and off

    public List<CreatureObject> targettableCreatures; //No crows, no wraiths, no murdermancers

    CreatureBehaviorScript currentTarget;

    //Rotating Variables
    float RotAngleY;
    float RotAngleMax;
    float RotAngleMin;
    float rotateSpeed = 5f;
    float myTime; //for tracking rotation

    public AudioSource activatedSource;

    //Use DOT products to determine if it is facing it's target, then fire a raycast to check for any obstacles, then fire
    //Maybe add a feature where a seed can plant itself on the tile a target is hit on
    //Has a chance to misfire, shooting off its course
    //Seeds should deal roughly 10 damage

    void Start()
    {
        base.Start();
        RotAngleY = turretHead.eulerAngles.y;
        RotAngleMax = RotAngleY + 45;
        RotAngleMin = RotAngleY - 45;

        StartCoroutine(TargetRefreshCooldown());
    }

    void Update()
    {
        base.Update();

        if(hidden || transitioning) return;

        if(currentTarget && currentTarget.isDead) currentTarget = null;
        if(currentTarget)
        {
            RotateToTarget();
        }

        if(shotCooldown) return;
        if(!currentTarget)
        {
            if(returningToCenter)
            {
                float returnSpeed = 50f; // degrees per second, tune as needed
                turretHead.rotation = Quaternion.RotateTowards(
                    turretHead.rotation,
                    Quaternion.Euler(0, RotAngleY, 0),
                    returnSpeed * Time.deltaTime
                );

                if(Quaternion.Angle(turretHead.rotation, Quaternion.Euler(0, RotAngleY, 0)) < 1f)
                {
                    turretHead.rotation = Quaternion.Euler(0, RotAngleY, 0);
                    returningToCenter = false;
                    
                    // Calculate myTime so PingPong resumes from the correct position
                    float currentRY = turretHead.eulerAngles.y;
                    float t = Mathf.InverseLerp(RotAngleMin, RotAngleMax, currentRY);
                    myTime = t / (rotateSpeed / 8f);
                }
            }
            else
            {
                myTime += Time.deltaTime;
                float rY = Mathf.SmoothStep(RotAngleMax,RotAngleMin,Mathf.PingPong(myTime * (rotateSpeed/8),1));
                turretHead.rotation = Quaternion.Euler(0,rY,0);
            }
        }
        else if(targetInSight)
        {
            shotCooldown = true;
            StartCoroutine(Shoot());
        }

    }

    void CheckForTargets()
    {
        CreatureBehaviorScript oldTarget = currentTarget;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, range, 1 << 9);
        
        float minDistance = Mathf.Infinity; // moved outside the loop
        
        foreach (Collider collider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            CreatureBehaviorScript newCreature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if(newCreature && newCreature.creatureData && targettableCreatures.Contains(newCreature.creatureData) && distance < minDistance && !newCreature.isDead)
            {
                minDistance = distance;
                if(newCreature != currentTarget) lockOnTime = 0f; // reset on new target
                currentTarget = newCreature;
            }
        }
        if (currentTarget && oldTarget != currentTarget) audioHandler.PlaySound(audioHandler.miscSounds1[0]);
    }

    void RotateToTarget()
    {
        Vector3 targetPosition = currentTarget.transform.position;

        Vector3 direction = (targetPosition - turretHead.position);
        direction.y = 0;
        direction.Normalize(); // normalize before use
        
        Quaternion toRotation = Quaternion.LookRotation(direction);
        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, toRotation, rotateSpeed * Time.deltaTime);

        // Check for structures in the way
        if (Physics.Raycast(bulletOrigin.position, direction, out RaycastHit hit, range, 1 << 6, QueryTriggerInteraction.Ignore))
        {
            targetInSight = false;
            return;
        }

        float dist = Vector3.Distance(turretHead.position, targetPosition);
        if((dist < minimumDistance || dist > range) && !shotCooldown)
        {
            currentTarget = null;
            targetInSight = false;
            return;
        }

        Vector3 forward = turretHead.TransformDirection(Vector3.forward);
        Vector3 toTarget = (targetPosition - turretHead.position).normalized;

        // In RotateToTarget, replace the targetInSight block:
        if (Vector3.Dot(forward, toTarget) > .90f)
        {
            lockOnTime += Time.deltaTime;
            targetInSight = lockOnTime >= requiredLockDuration;
            returningToCenter = true;
        }
        else
        {
            targetInSight = false;
            lockOnTime = 0f;
        }
    }

    IEnumerator Shoot()
    {
        if(!currentTarget) yield break;
        if(savedItems.Count == 0)
        {
            audioHandler.PlaySound(audioHandler.miscSounds1[1]);
            yield return new WaitForSeconds(2f);
            shotCooldown = false;
            yield break;
        }

        canTransition = false;
        shotCooldown = true;
        targetInSight = false;
        float r;

        currentTarget.NewPriorityTarget(this);
        //fire

        Vector3 targetPosition = currentTarget.transform.position;

        audioHandler.PlaySound(audioHandler.activatedSound);
        GameObject newBullet = ProjectilePoolManager.Instance.GrabSeedBullet();
        newBullet.GetComponentInParent<BulletScript>().creatureDamage = 15; //Change bullet damage
        Vector3 dir = (targetPosition - turretHead.position).normalized;

        r = Random.Range(0,10);
        if(r > 7f)
        {
            dir = dir + new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f,1f));
            //print("MISSFIRE");
            //play misfire sound
        }
        newBullet.transform.position = bulletOrigin.position;
        newBullet.transform.rotation = Quaternion.LookRotation(dir);

        newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
        newBullet.GetComponent<Rigidbody>().AddForce(dir * projectileSpeed);
        //print("PEW");

        float saveBulletChance = 0;

        if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.PrudentPeriapt))
        {
            saveBulletChance += 20;
            TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.PrudentPeriapt);
        }
        if(MainMenuScript.currentFileMode == FileMode.Cozy) saveBulletChance += 20;

        if(saveBulletChance < Random.Range(0,100))
        {
            InventoryItemData itemShot = savedItems[0];
            savedItems.Remove(itemShot);
        }

        ParticlePoolManager.Instance.MoveAndPlayVFX(bulletOrigin.position, ParticlePoolManager.Instance.hitEffect);
        ParticlePoolManager.Instance.GrabCloudParticle().transform.position = bulletOrigin.position;
        yield return new WaitForSeconds(0.2f);
        

        canTransition = true;
        yield return new WaitForSeconds(Random.Range(2.5f, 3f));

        shotCooldown = false;
    }

    IEnumerator TargetRefreshCooldown()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(1);
            CheckForTargets();
        }
    }

    IEnumerator ToggleSentry()
    {
        transitioning = true;
        hidden = !hidden;

        audioHandler.PlaySound(audioHandler.miscSounds1[3]);

        if(hidden)
        {
            //Clear Target
            if(currentTarget) currentTarget = null;

            isObstacle = false;

            turretHead.DOMove(inactivePos.position, 0.5f, false);

            yield return new WaitForSeconds(0.2f);
            leafParticles.Play();
            audioHandler.PlaySound(audioHandler.miscSounds1[2]);
            yield return new WaitForSeconds(0.55f);

            activatedSource.Pause();
        }
        else
        {
            isObstacle = true;

            turretHead.DOMove(activePos.position, 0.5f, false);

            leafParticles.Play();
            audioHandler.PlaySound(audioHandler.miscSounds1[2]);
            activatedSource.Play();
            yield return new WaitForSeconds(0.75f);
        }

        transitioning = false;
    }

    public override void StructureInteraction()
    {
        if(transitioning || !canTransition) return;

        StartCoroutine(ToggleSentry());
    }

    public override void ItemInteraction(InventoryItemData item)
    {
        if(item && savedItems.Count < maxAmmo && item == boneItem)
        {
            if(HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize >= 5)
            {
                int amountAdded = 0;
                for(int i = 0; i < 5; i++)
                {
                    if(savedItems.Count == maxAmmo) break;
                    savedItems.Add(item);
                    amountAdded++;
                }
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(amountAdded);
            }
            else
            {
                savedItems.Add(item);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            }

            //savedItems.Add(item);
            //HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            poofParticle.Play();
            audioHandler.PlaySound(audioHandler.itemInteractSound);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        //drop seeds
        GameObject droppedItem;
        foreach(InventoryItemData item in savedItems)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(item);
            droppedItem.transform.position = seedSocket.position;
        }

        destroyedLeaves.SetActive(true);
        destroyedLeaves.transform.parent = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(turretHead.transform.position, range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(turretHead.transform.position, minimumDistance);
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        structureUIVariables.valueGroups[1].value = savedItems.Count;
        structureUIVariables.valueGroups[1].maxValue = maxAmmo;
        return structureUIVariables.valueGroups;
    }
}
