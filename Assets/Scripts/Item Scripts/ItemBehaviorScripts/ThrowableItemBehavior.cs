using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/Throwable")]
public class ThrowableItemBehavior : ItemBehavior
{
    public GameObject prefab;

    public float force = 100;

    public override void UseItem(out bool consumeItem)
    {
        Vector3 itemPos = PlayerInteraction.Instance.transform.position;

        ThrowItem();

        consumeItem = true;
    }

    void ThrowItem()
    {
        Transform bulletStart = HandItemManager.Instance.bulletStart;

        GameObject projectile = Instantiate(prefab, bulletStart.position, Quaternion.identity);
        projectile.transform.position = bulletStart.position;
        projectile.transform.rotation = Quaternion.identity;
        Vector3 dir = bulletStart.forward;
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.AddForce(dir * force);
        rb.AddForce(Vector3.up * 50);
    }
}
