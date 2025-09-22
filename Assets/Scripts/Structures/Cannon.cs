using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Cannon : StructureBehaviorScript
{
    public Transform cannonHead, bulletOrigin;

    public List<GameObject> loadedAmmo;

    public GameObject primedEffect;
    public ParticleSystem firedEffect, smokeEffect;

    bool isPrimed, forceFire;

    float range = 70; //Get a debug sphere to show the range
    //bool targetInSight = false;
    bool shotCooldown;
    float projectileSpeed = 230;

    public List<CreatureObject> targettableCreatures; //No crows, no wraiths, no murdermancers

    CreatureBehaviorScript currentTarget;


    void Start()
    {
        base.Start();

        if(savedItems.Count > 0 && savedItems[0] != null) UpdateModel(savedItems[0].ID, out bool success);
        else UpdateModel(-1, out bool success);

        StartCoroutine(TargetRefreshCooldown());
    }

    void Update()
    {
        base.Update();

        if(currentTarget && currentTarget.isDead) currentTarget = null;

        if(shotCooldown) return;

        //TargetIsVisible();

        if(savedItems.Count > 0 && isPrimed && (currentTarget || forceFire))
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

        Vector3 targetPosition;
        if(currentTarget) targetPosition = currentTarget.transform.position;
        else targetPosition = transform.forward;

        shotCooldown = true;
        isPrimed = false;
        forceFire = false;
        primedEffect.SetActive(false);
        StopCoroutine(PrimedRoutine());

        currentTarget.NewPriorityTarget(this);

        if(currentTarget)
        {
            currentTarget.NewPriorityTarget(this);
        }
        audioHandler.PlaySound(audioHandler.activatedSound);
        GameObject newBullet;

        switch(savedItems[0].ID)
        {
            case 116:
                newBullet = ProjectilePoolManager.Instance.GrabTimberEarBullet();
                break;
            case 115:
                newBullet = ProjectilePoolManager.Instance.GrabCannonRockBullet();
                break;
            case 142:
                newBullet = ProjectilePoolManager.Instance.GrabPyreflyBullet();
                break;
            default:
            newBullet = ProjectilePoolManager.Instance.GrabTimberEarBullet();
            break;
        }
        audioHandler.PlaySound(audioHandler.interactSound);
        firedEffect.Play();
        smokeEffect.Play();
        UpdateModel(-1, out bool success);

        Vector3 dir = (targetPosition - cannonHead.position).normalized;

        newBullet.transform.position = bulletOrigin.position;
        newBullet.transform.rotation = Quaternion.identity;

        newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 5);
        newBullet.GetComponent<Rigidbody>().AddForce(dir * projectileSpeed);
        //print("PEW");

        ParticlePoolManager.Instance.MoveAndPlayVFX(bulletOrigin.position, ParticlePoolManager.Instance.hitEffect);
        ParticlePoolManager.Instance.GrabCloudParticle().transform.position = bulletOrigin.position;

        cannonHead.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.5f, 0, 0.2f);
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
        if(savedItems.Count == 0)
        {
            UpdateModel(item.ID, out bool success);
            if(!success) return;

            savedItems.Add(item);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            audioHandler.PlaySound(audioHandler.itemInteractSound);
        }
    }

    void UpdateModel(int itemID, out bool success)
    {
        foreach(GameObject obj in loadedAmmo) obj.SetActive(false);
        if(itemID <= -1)
        {
            success = false;
            return;
        }
        switch(itemID) //This is where we enable objects in the cannon
        {
            case 116:
                loadedAmmo[0].SetActive(true);
                success = true;
                break;
            case 115:
                loadedAmmo[1].SetActive(true);
                success = true;
                break;
            case 142:
                loadedAmmo[2].SetActive(true);
                success = true;
                break;

            default:
                success = false;
                break;
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
        if(type == ToolType.Torch && PlayerInteraction.Instance.torchLit && !isPrimed && savedItems.Count > 0)
        {
            StartCoroutine(PrimedRoutine());
            success = true;
        }
        if(type == ToolType.Pyrefly && savedItems.Count == 0)
        {
            ItemInteraction(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
            success = false;
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

    IEnumerator PrimedRoutine()
    {
        shotCooldown = true;
        primedEffect.SetActive(true);
        yield return new WaitForSeconds(1);
        shotCooldown = false;
        isPrimed = true;
        yield return new WaitForSeconds(30);
        forceFire = true;
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
