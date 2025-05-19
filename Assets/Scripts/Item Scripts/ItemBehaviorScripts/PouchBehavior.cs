using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item Behavior", menuName = "Item Behavior/Pouch")]
public class PouchBehavior : ItemBehavior
{
    public ItemWithAmount[] possibleItems;
    public override void UseItem(out bool consumeItem)
    {
        Vector3 itemPos = PlayerInteraction.Instance.transform.position;
        for(int i = 0; i < possibleItems.Length; i++)
        {
            if(possibleItems[i].amount > Random.Range(0, 100))
            {
                GameObject droppedItem = ItemPoolManager.Instance.GrabItem(possibleItems[i].item);
                droppedItem.transform.position = new Vector3(itemPos.x, itemPos.y + 1f, itemPos.z);

                Vector3 dir3 = Random.onUnitSphere;
                dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
                itemRB.AddForce(dir3 * 35);
                itemRB.AddForce(Vector3.up * 25);
                itemRB.AddForce(PlayerInteraction.Instance.transform.forward * 50);
            }
        }

        consumeItem = true;
    }
}
