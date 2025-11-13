using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderCocoon : StructureBehaviorScript
{
    public InventoryItemData silk;
    public CreatureObject spiderData;

    public GameObject destructionObject;
    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded || onFire) return; 
        GameObject droppedItem;
        Rigidbody itemRB;
        int r = Random.Range(0,12);
        if(r > 9)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(silk);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            Vector3 dir3 = Random.onUnitSphere;
            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(dir3 * 20);
            itemRB.AddForce(Vector3.up * 50);
        }

        if((r == 1 || r == 9) && TutorialMiller.Instance == null)
        {
            Instantiate(spiderData.objectPrefab, transform.position, Quaternion.identity);
        }

        destructionObject.SetActive(true);
        destructionObject.transform.parent = null;
    }
}
