using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BoneTurret : StructureBehaviorScript
{
    public Transform turretHead, bulletOrigin, seedSocket;

    float maxAmmo = 10; //Dont allow any more seeds to be added to the item list after there are this many entrants
    float range = 15; //Get a debug sphere to show the range
    bool targetInSight = false;
    bool shotCooldown;
    bool returningToCenter;
    float projectileSpeed = 270;
    float minimumDistance = 2;

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
    float rotateSpeed = 2f;
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
                if(RotAngleY > turretHead.eulerAngles.y) turretHead.rotation = Quaternion.Euler(0,turretHead.eulerAngles.y + 0.1f,0);
                else turretHead.rotation = Quaternion.Euler(0,turretHead.eulerAngles.y - 0.1f,0);
                if(turretHead.eulerAngles.y < RotAngleY + 5 && turretHead.eulerAngles.y > RotAngleY - 5)
                {
                    returningToCenter = false;
                    myTime = 0;
                }
            }
            else
            {
                myTime += Time.deltaTime;
                float rY = Mathf.SmoothStep(RotAngleMax,RotAngleMin,Mathf.PingPong(myTime * (rotateSpeed/4),1));
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
        foreach (Collider collider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            float minDistance = Mathf.Infinity;
            CreatureBehaviorScript newCreature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if(newCreature && newCreature.creatureData && targettableCreatures.Contains(newCreature.creatureData) && distance < minDistance && !newCreature.isDead)
            {
                minDistance = distance;
                currentTarget = newCreature;
            }
        }
        if (currentTarget && oldTarget != currentTarget) audioHandler.PlaySound(audioHandler.miscSounds1[0]);
    }

    void RotateToTarget()
    {
        Vector3 targetPosition;
        /*if(currentTarget.corpseParticleTransform) targetPosition = currentTarget.corpseParticleTransform.position;
        else*/ targetPosition = currentTarget.transform.position;

        Vector3 direction = targetPosition - turretHead.position;
        direction.y = 0;
        Quaternion toRotation = Quaternion.LookRotation(direction);

        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, toRotation, rotateSpeed * Time.deltaTime);

        RaycastHit hit;
        if (Physics.Raycast(turretHead.position, direction/*turretHead.forward*/, out hit, range, 1 << 6))
        {
            targetInSight = false;
            return;
            //structure in the way
        }

        //If the rotation is close enough to target, fire
        Vector3 forward = turretHead.TransformDirection(Vector3.forward);
        Vector3 toTarget = Vector3.Normalize(targetPosition - turretHead.position);

        float dist = Vector3.Distance(turretHead.position, targetPosition);

        if((dist < minimumDistance || dist > range) && !shotCooldown)
        {
            currentTarget = null;
            targetInSight = false;
            return;
        }

        if (Vector3.Dot(forward, toTarget) > .95f)
        {
            targetInSight = true;
            returningToCenter = true;
        }
        else targetInSight = false;
    }

    IEnumerator Shoot()
    {
        if(savedItems.Count == 0)
        {
            audioHandler.PlaySound(audioHandler.miscSounds1[1]);
            yield return new WaitForSeconds(2f);
            shotCooldown = false;
            yield break;
        }

        canTransition = false;

        Vector3 targetPosition;
        /*if(currentTarget.corpseParticleTransform) targetPosition = currentTarget.corpseParticleTransform.position;
        else*/ targetPosition = currentTarget.transform.position;

        shotCooldown = true;
        targetInSight = false;
        float r;

        currentTarget.NewPriorityTarget(this);
        //fire
        for(int i = 0; i < 1; i++)
        {
            if(!currentTarget) break;
            audioHandler.PlaySound(audioHandler.activatedSound);
            GameObject newBullet = ProjectilePoolManager.Instance.GrabSeedBullet();
            Vector3 dir = (targetPosition - turretHead.position).normalized;

            r = Random.Range(0,10);
            if(r > 7f)
            {
                dir = dir + new Vector3(Random.Range(-1f,1f), 0, Random.Range(-1f,1f));
                //print("MISSFIRE");
                //play misfire sound
            }
            newBullet.transform.position = bulletOrigin.position;
            newBullet.transform.rotation = Quaternion.identity;

            newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
            newBullet.GetComponent<Rigidbody>().AddForce(dir * projectileSpeed);
            //print("PEW");

            ParticlePoolManager.Instance.MoveAndPlayVFX(bulletOrigin.position, ParticlePoolManager.Instance.hitEffect);
            ParticlePoolManager.Instance.GrabCloudParticle().transform.position = bulletOrigin.position;
            yield return new WaitForSeconds(0.2f);
        }

        canTransition = true;
        yield return new WaitForSeconds(3f);

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
            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            /*GameObject poofParticle = ParticlePoolManager.Instance.GrabExtinguishParticle();
            poofParticle.transform.position = seedSocket.position;*/
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
