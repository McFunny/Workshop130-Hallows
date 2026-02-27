using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/ShotGun")]
public class ShotGunBehavior : ToolBehavior
{
    public InventoryItemData bulletItem, pinexBulletItem;
    public List<InventoryItemData> acceptableAmmo;

    InventoryItemData bulletFired;

    public AudioClip shoot, reload, jamClick;
    int bulletCount = 6;
    int pinexBulletCount = 10;

    Transform bulletStart;

    float speed = 240;
    float bulletSpread = 0.07f;
    float pinexBulletSpread = 0.075f;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || TownGate.Instance.location == PlayerLocation.InTown) return;
        if (!player) player = _player;

        if(EndingManager.Instance.endingPlaying)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(jamClick);
            return;
        }


        var inventory = PlayerInventoryHolder.Instance.PrimaryInventorySystem;
        if (inventory.ContainsItems(acceptableAmmo, out List<InventorySlot> invSlot))
        {
            if(invSlot[0].ItemData == bulletItem)
            {
                bulletFired = bulletItem;
                inventory.RemoveItemsFromInventory(bulletItem, 1);
            }
            else if(invSlot[0].ItemData == pinexBulletItem)
            {
                bulletFired = pinexBulletItem;
                inventory.RemoveItemsFromInventory(pinexBulletItem, 1);
            }
        }
        else 
        {
            Debug.Log("No Bullet In Primary");
            inventory = PlayerInventoryHolder.Instance.secondaryInventorySystem;
            if (inventory.ContainsItems(acceptableAmmo, out List<InventorySlot> invSlot2))
            {
                if(invSlot2[0].ItemData == bulletItem)
                {
                    bulletFired = bulletItem;
                    inventory.RemoveItemsFromInventory(bulletItem, 1);
                }
                else if(invSlot2[0].ItemData == pinexBulletItem)
                {
                    bulletFired = pinexBulletItem;
                    inventory.RemoveItemsFromInventory(pinexBulletItem, 1);
                }
            }
            else
            {
                Debug.Log("No Bullet In Secondary");
                HandItemManager.Instance.toolSource.PlayOneShot(jamClick);
                return;
            }
        }
        
        tool = _tool;
        usingPrimary = true;
        //Shoot

        bool isReloading = ShotgunAmmoCheck(bulletFired);
        HandItemManager.Instance.DoesShotgunReload(isReloading);
        HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(shoot);
        float cooldown = isReloading ? 2.5f : 0.3f;
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, cooldown));
    }

    private bool ShotgunAmmoCheck(InventoryItemData bullet)
    {
        return PlayerInventoryHolder.Instance.FindItemInBothInventories(bullet);
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        tool = _tool;

        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if (Physics.Raycast(player.position, fwd, out hit, 6, 1 << 6))
        {
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null && structure.Interactable())
            {
                //Use Tool to interact with structure (Probably just the tool rack)
                bool success = false;
                structure.ToolInteraction(tool, out success);
                if(success) return;
            }
        }
    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            HandItemManager.Instance.StartCoroutine(ShootGun());
        }
        if (usingSecondary)
        {
            usingSecondary = false;
        }

    }

    public IEnumerator ShootGun()
    {
        PlayerInteraction.Instance.ShakeScreen(0.5f);
        if(!bulletStart)
        {
            bulletStart = HandItemManager.Instance.bulletStart;
        }
        if(bulletFired == bulletItem) ShootBullets();
        else if(bulletFired == pinexBulletItem) ShootSeedBullets();
        yield return new WaitForSeconds(1.2f);
        usingPrimary = false;
    }

    void ShootBullets()
    {
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject newBullet = ProjectilePoolManager.Instance.GrabBullet();
            newBullet.transform.position = bulletStart.position;
            newBullet.transform.rotation = Quaternion.identity;
            Vector3 dir;
            if(i == 0) dir = bulletStart.forward + new Vector3(Random.Range(-0.02f,+0.02f), Random.Range(-0.02f,0.02f), Random.Range(-0.02f,0.02f));
            else dir = bulletStart.forward + new Vector3(Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread));
            newBullet.GetComponent<Rigidbody>().AddForce(dir * speed);
 
        }
    }

    void ShootSeedBullets()
    {
        for (int i = 0; i < pinexBulletCount; i++)
        {
            GameObject newBullet = ProjectilePoolManager.Instance.GrabSeedBullet();
            newBullet.transform.position = bulletStart.position;
            newBullet.transform.rotation = Quaternion.identity;
            Vector3 dir = bulletStart.forward + new Vector3(Random.Range(-pinexBulletSpread,pinexBulletSpread), Random.Range(-pinexBulletSpread,pinexBulletSpread), Random.Range(-pinexBulletSpread,pinexBulletSpread));
            newBullet.GetComponent<Rigidbody>().AddForce(dir * speed);
 
        }
    }
}
