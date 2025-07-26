using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RustlingDirt : MonoBehaviour
{
    public List<ItemWithAmount> items = new List<ItemWithAmount>();

    void Start()
    {
        TimeManager.OnHourlyUpdate += OnHour;
    }

    void OnHour()
    {
        if(TimeManager.Instance.currentHour == 7)
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= OnHour;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.GetComponentInParent<RustlingDirt>() != null) Destroy(this.gameObject);

        StructureBehaviorScript s = other.GetComponentInParent<StructureBehaviorScript>();
        if(s == null) return;

        UntilledTile tile = s as UntilledTile;
        if(tile)
        {
            InventoryItemData item = null;
            int x = 0;
            while(!item && x < 20)
            {
                int r = Random.Range(0, items.Count);
                if(Random.Range(0, 100) < items[r].amount) item = items[r].item;
                x++;
            }
            if(item)
            {
                GameObject droppedItem = ItemPoolManager.Instance.GrabItem(item);
                droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

                Vector3 dir3 = Random.onUnitSphere;
                dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
                Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
                itemRB.AddForce(dir3 * 20);
                itemRB.AddForce(Vector3.up * 50);

                ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
                ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
            }
        }

        else Destroy(this.gameObject);
    }
}
