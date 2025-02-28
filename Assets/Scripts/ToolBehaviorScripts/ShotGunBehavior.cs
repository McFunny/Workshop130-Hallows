using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/ShotGun")]
public class ShotGunBehavior : ToolBehavior
{
    public InventoryItemData bulletItem;
    public AudioClip shoot, reload;
    int bulletCount = 6;

    Transform bulletStart;

    float speed = 240;
    float bulletSpread = 0.08f;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || TownGate.Instance.location == PlayerLocation.InTown) return;
        if (!player) player = _player;

        var inventory = PlayerInventoryHolder.Instance.PrimaryInventorySystem;
        if (inventory.ContainsItem(bulletItem, out List<InventorySlot> invSlot))
        {
            inventory.RemoveItemsFromInventory(bulletItem, 1);
        }
        else 
        {
            Debug.Log("No Bullet In Primary");
            inventory = PlayerInventoryHolder.Instance.secondaryInventorySystem;
            if (inventory.ContainsItem(bulletItem, out List<InventorySlot> invSlot2))
            {
                inventory.RemoveItemsFromInventory(bulletItem, 1);
            }
            else
            {
                Debug.Log("No Bullet In Secondary");
                return;
            }
        }
        
        tool = _tool;
        usingPrimary = true;
        //Shoot
        HandItemManager.Instance.DoesShotgunReload(ShotgunAmmoCheck());
        HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(shoot);
        float cooldown = ShotgunAmmoCheck() ? 2.8f : 0.3f;
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, cooldown));
    }

    private bool ShotgunAmmoCheck()
    {
        return PlayerInventoryHolder.Instance.FindItemInBothInventories(bulletItem);
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        //

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
        if(!bulletStart)
        {
            bulletStart = HandItemManager.Instance.bulletStart;
        }
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject newBullet = ProjectilePoolManager.Instance.GrabBullet();
            newBullet.transform.position = bulletStart.position;
            newBullet.transform.rotation = Quaternion.identity;
            Vector3 dir = bulletStart.forward + new Vector3(Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread));
            newBullet.GetComponent<Rigidbody>().AddForce(dir * speed);
 
        }
        yield return new WaitForSeconds(1.2f);
        usingPrimary = false;
    }
}
