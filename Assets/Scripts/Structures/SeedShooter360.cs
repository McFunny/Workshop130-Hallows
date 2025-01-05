using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedShooter360 : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    public Transform turretHead, bulletOrigin, seedSocket;

    float maxAmmo = 10; //Dont allow any more seeds to be added to the item list after there are 10 entrants
    float range = 20;
    bool targetInSight = false;
    bool shotCooldown;
    float projectileSpeed = 150;

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
    //Seeds should deal roughly 20 damage
    //To reduce its power, maybe make it unable to fire over barricades

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
        float minimumDistance = Mathf.Infinity;
        foreach (Collider collider in hitColliders)
        {
            float distance = Vector3.Distance(transform.position, collider.transform.position);
            CreatureBehaviorScript newCreature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if(newCreature && newCreature.creatureData && targettableCreatures.Contains(newCreature.creatureData) && distance < minimumDistance && !newCreature.isDead)
            {
                minimumDistance = distance;
                currentTarget = newCreature;
            }
        }
    }

    void RotateToTarget()
    {
        Vector3 direction = currentTarget.transform.position - transform.position;
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
        audioHandler.PlaySound(audioHandler.activatedSound);
        GameObject newBullet = ProjectilePoolManager.Instance.GrabSeedBullet();
        Vector3 dir = (currentTarget.transform.position - turretHead.position).normalized;

        float r = Random.Range(0,10);
        if(r > 8)
        {
            dir = dir + new Vector3(Random.Range(-0.5f,0.5f), 0, Random.Range(-0.5f,0.5f));
            print("MISSFIRE");
            //play misfire sound
        }
        newBullet.transform.position = bulletOrigin.position;
        newBullet.transform.rotation = Quaternion.identity;

        newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 20);
        newBullet.GetComponent<Rigidbody>().AddForce(dir * projectileSpeed);
        print("PEW");

        InventoryItemData seedShot = savedItems[0];
        savedItems.Remove(seedShot);

        ParticlePoolManager.Instance.MoveAndPlayVFX(bulletOrigin.position, ParticlePoolManager.Instance.hitEffect);
        yield return new WaitForSeconds(1.5f);

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
        if(seed && savedItems.Count < 15 && seed.ableToBeShot)
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
}
