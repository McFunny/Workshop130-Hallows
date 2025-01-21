using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedShooter360 : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    public Transform turretHead, bulletOrigin, seedSocket;

    float maxAmmo = 20; //Dont allow any more seeds to be added to the item list after there are this many entrants
    float range = 20; //Get a debug sphere to show the range
    bool targetInSight = false;
    bool shotCooldown;
    float projectileSpeed = 150;
    float minimumDistance = 6;

    //Maybe add a large button on the back, where when the player interacts with this, it can be turned on and off

    public List<CreatureObject> targettableCreatures; //No crows, no wraiths, no murdermancers

    CreatureBehaviorScript currentTarget;

    //Rotating Variables
    float RotAngleY;
    float RotAngleMax;
    float RotAngleMin;
    float rotateSpeed = 1f;

    //Use DOT products to determine if it is facing it's target, then fire a raycast to check for any obstacles, then fire
    //Maybe add a feature where a seed can plant itself on the tile a target is hit on
    //Has a chance to misfire, shooting off its course
    //Seeds should deal roughly 10 damage

    void Awake()
    {
        base.Awake();
    }

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

        if(currentTarget && currentTarget.isDead) currentTarget = null;
        if(currentTarget)
        {
            RotateToTarget();
        }

        if(shotCooldown) return;
        if(!currentTarget)
        {
            float rY = Mathf.SmoothStep(RotAngleMax,RotAngleMin,Mathf.PingPong(Time.time * (rotateSpeed/2),1));
            turretHead.rotation = Quaternion.Euler(0,rY,0);
        }
        else if(targetInSight && savedItems.Count > 0)
        {
            shotCooldown = true;
            StartCoroutine(Shoot());
        }

    }

    void CheckForTargets()
    {
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
    }

    void RotateToTarget()
    {
        Vector3 direction = currentTarget.transform.position - turretHead.position;
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
        Vector3 toTarget = Vector3.Normalize(currentTarget.transform.position - turretHead.position);

        float dist = Vector3.Distance(turretHead.position, currentTarget.transform.position);

        if((dist < minimumDistance || dist > range) && !shotCooldown)
        {
            currentTarget = null;
            targetInSight = false;
            return;
        }

        if (Vector3.Dot(forward, toTarget) > .95f)
        {
            targetInSight = true;
        }
        else targetInSight = false;
    }

    IEnumerator Shoot()
    {
        shotCooldown = true;
        targetInSight = false;

        currentTarget.NewPriorityTarget(this);
        //fire
        for(int i = 0; i < 4; i++)
        {
            if(!currentTarget) break;
            audioHandler.PlaySound(audioHandler.activatedSound);
            GameObject newBullet = ProjectilePoolManager.Instance.GrabSeedBullet();
            Vector3 dir = (currentTarget.transform.position - turretHead.position).normalized;

            float r = Random.Range(0,10);
            if(r > 6.5f)
            {
                dir = dir + new Vector3(Random.Range(-0.5f,0.5f), 0, Random.Range(-0.5f,0.5f));
                print("MISSFIRE");
                //play misfire sound
            }
            newBullet.transform.position = bulletOrigin.position;
            newBullet.transform.rotation = Quaternion.identity;

            newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
            newBullet.GetComponent<Rigidbody>().AddForce(dir * projectileSpeed);
            print("PEW");

            ParticlePoolManager.Instance.MoveAndPlayVFX(bulletOrigin.position, ParticlePoolManager.Instance.hitEffect);
            yield return new WaitForSeconds(0.2f);
        }
        InventoryItemData seedShot = savedItems[0];
        savedItems.Remove(seedShot);
        yield return new WaitForSeconds(2.5f);

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

    public override void ItemInteraction(InventoryItemData item)
    {
        CropItem seed = item as CropItem;
        if(seed && savedItems.Count < maxAmmo && seed.ableToBeShot)
        {
            savedItems.Add(seed);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            GameObject poofParticle = ParticlePoolManager.Instance.GrabExtinguishParticle();
            poofParticle.transform.position = seedSocket.position;

            audioHandler.PlaySound(audioHandler.itemInteractSound);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;

        Destroy(this.gameObject);
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
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(turretHead.transform.position, range);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(turretHead.transform.position, minimumDistance);
    }
}
