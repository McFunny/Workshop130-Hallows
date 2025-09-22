using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cannon : StructureBehaviorScript
{
    public Transform cannonHead, bulletOrigin;

    public List<GameObject> loadedAmmo;

    float range = 60; //Get a debug sphere to show the range
    //bool targetInSight = false;
    bool shotCooldown;
    float projectileSpeed = 230;

    public List<CreatureObject> targettableCreatures; //No crows, no wraiths, no murdermancers

    CreatureBehaviorScript currentTarget;


    void Start()
    {
        base.Start();

        StartCoroutine(TargetRefreshCooldown());
    }

    void Update()
    {
        base.Update();

        if(currentTarget && currentTarget.isDead) currentTarget = null;

        if(shotCooldown) return;

        //TargetIsVisible();

        if(currentTarget && savedItems.Count >= 0)
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
            if(newCreature && newCreature.creatureData && targettableCreatures.Contains(newCreature.creatureData) && distance < minDistance && !newCreature.isDead && TargetIsVisible(newCreature.transform))
            {
                minDistance = distance;
                currentTarget = newCreature;
            }
        }
        if (currentTarget && oldTarget != currentTarget) audioHandler.PlaySound(audioHandler.miscSounds1[0]);
    }

    bool TargetIsVisible(Transform target)
    {
        if(target == null) return false;

        Vector3 direction = target.position - cannonHead.position;
        direction.y = 0;

        Vector3 forward = transform.TransformDirection(Vector3.forward);
        //Vector3 toTarget = Vector3.Normalize(currentTarget.transform.position - cannonHead.position);

        RaycastHit hit;
        if (Physics.Raycast(cannonHead.position, direction/*cannonHead.forward*/, out hit, range, 1 << 6))
        {
            //targetInSight = false;
            return false;
            //structure in the way
        }

        if (Vector3.Dot(forward, direction) > .7f)
        {
            return true;        }
        else return false;
    }

    IEnumerator Shoot()
    {

        Vector3 targetPosition = currentTarget.transform.position;

        shotCooldown = true;
        //targetInSight = false;

        currentTarget.NewPriorityTarget(this);
        //fire
        if(!currentTarget) //No Target
        {
            shotCooldown = false;
            yield break;
        }
        audioHandler.PlaySound(audioHandler.activatedSound);
        GameObject newBullet;

        switch(savedItems[0].ID)
        {
            default:
            newBullet = ProjectilePoolManager.Instance.GrabTimberEarBullet();
            break;
        }

        Vector3 dir = (targetPosition - cannonHead.position).normalized;

        newBullet.transform.position = bulletOrigin.position;
        newBullet.transform.rotation = Quaternion.identity;

        newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
        newBullet.GetComponent<Rigidbody>().AddForce(dir * projectileSpeed);
        //print("PEW");

        ParticlePoolManager.Instance.MoveAndPlayVFX(bulletOrigin.position, ParticlePoolManager.Instance.hitEffect);
        ParticlePoolManager.Instance.GrabCloudParticle().transform.position = bulletOrigin.position;
        yield return new WaitForSeconds(0.2f);
        savedItems.Clear();
        
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
        bool itemInserted = false;
        switch(item.ID) //This is where we enable objects in the cannon
        {
            default:
            return;
            break;
        }
        if(itemInserted && savedItems.Count < 1)
        {
            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            audioHandler.PlaySound(audioHandler.itemInteractSound);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(shotCooldown) return;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
        if(type == ToolType.Torch && PlayerInteraction.Instance.torchLit)
        {
            //
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
            droppedItem.transform.position = bulletOrigin.position;
        }
    }

    public override List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;

        //structureUIVariables.valueGroups[1].value = savedItems.Count;
        //structureUIVariables.valueGroups[1].maxValue = maxAmmo;
        return structureUIVariables.valueGroups;
    }
}
