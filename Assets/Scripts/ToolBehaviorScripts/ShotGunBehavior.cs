using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/ShotGun")]
public class ShotGunBehavior : ToolBehavior
{
    public InventoryItemData bulletItem;
    public GameObject bullet;
    public AudioClip shoot, reload;
    int bulletCount = 6;

    Transform bulletStart;

    float speed = 180;
    float bulletSpread = 0.08f;
    float damage = 25;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
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
        HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(shoot);
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, 2.8f));
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

            //To give the bullet it's damage values
            BulletScript bulletScript = newBullet.GetComponent<BulletScript>();
            bulletScript.damage = damage;
 
        }
        yield return new WaitForSeconds(1.2f);
        usingPrimary = false;
    }

  
}
