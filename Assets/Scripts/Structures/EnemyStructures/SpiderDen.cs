using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderDen : StructureBehaviorScript
{
    public CreatureObject spiderData;

    public int heldSpiders = 3;
    public int maxSpiders = 4;
    public int outsideSpiders = 0;

    public InventoryItemData silk;
    public GameObject destructionObject;

    void Start()
    {
        OnDamage += DenHit;
        base.Start();
    }

    void OnDestroy()
    {
        OnDamage -= DenHit;
        TimeManager.OnHourlyUpdate -= HourPassed;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded || onFire) return; 

        destructionObject.SetActive(true);
        destructionObject.transform.parent = null;

        GameObject droppedItem;
        Rigidbody itemRB;
        int r = Random.Range(3,6);
        for(int i = 0; i < r; i++)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(silk);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            Vector3 dir3 = Random.onUnitSphere;
            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(dir3 * 20);
            itemRB.AddForce(Vector3.up * 50);
        }

        PlayerMovement.Instance.RemoveSpeedMod(gameObject);
    }

    public override void HourPassed()
    {
        for(int i = 0; i < 2; ++i)
        {
            if((heldSpiders < maxSpiders || outsideSpiders < maxSpiders) && Random.Range(0,10) > 3) heldSpiders++;

            if(heldSpiders >= maxSpiders)
            {
                heldSpiders--;
                outsideSpiders++;
                Instantiate(spiderData.objectPrefab, transform.position, Quaternion.identity).GetComponent<Spider>().homeDen = this;
            }
        }
    }

    public override void HitWithWater()
    {
        DenHit();
    }

    void DenHit()
    {
        if(heldSpiders > 0)
        {
            Instantiate(spiderData.objectPrefab, transform.position, Quaternion.identity).GetComponent<Spider>().homeDen = this;
            heldSpiders--;
            outsideSpiders++; 
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            DenHit();
            PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(gameObject, 0.3f, "Webbing", false));
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            PlayerMovement.Instance.RemoveSpeedMod(gameObject);
        }
    }
}
