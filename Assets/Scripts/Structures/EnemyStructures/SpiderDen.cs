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

    public GameObject stage1, stage2;
    public bool isLarge = false;
    float chanceToGrow = 0;

    void Start()
    {
        OnDamage += DenHit;
        base.Start();

        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(1);
        if(absentFromGrid) UpdateStage(true);
    }

    void UpdateStage(bool grown)
    {
        if(isLarge == grown) return;

        isLarge = grown;

        if(isLarge)
        {
            heldSpiders = 2;
            health = maxHealth;
            stage1.SetActive(false);
            stage2.SetActive(true);
        }
        else
        {
            stage1.SetActive(true);
            stage2.SetActive(false);
        }
    }

    void OnDestroy()
    {
        OnDamage -= DenHit;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded || onFire) return; 

        destructionObject.SetActive(true);
        destructionObject.transform.parent = null;

        GameObject droppedItem;
        Rigidbody itemRB;
        int r = Random.Range(3,6);
        if(!isLarge) r = Random.Range(-1, 2);
        for(int i = 0; i < r; ++i)
        {
            droppedItem = ItemPoolManager.Instance.GrabItem(silk);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            Vector3 dir3 = Random.onUnitSphere;
            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(dir3 * 20);
            itemRB.AddForce(Vector3.up * 50);
        }

        for(int x = 0; x < heldSpiders; ++x)
        {
            Spider newSpider = Instantiate(spiderData.objectPrefab, transform.position, Quaternion.identity).GetComponent<Spider>();
            if(absentFromGrid)
            {
                newSpider.persistAfterNewDay = false;
                newSpider.inWilderness = true;
            }
        }

        PlayerMovement.Instance.RemoveSpeedMod(gameObject);
    }

    public override void HourPassed()
    {
        if(!isLarge)
        {
            if(Random.Range(0, 100) < chanceToGrow) UpdateStage(true);
            else chanceToGrow += 23;
            return;
        }

        for(int i = 0; i < 2; ++i)
        {
            if((heldSpiders < maxSpiders || outsideSpiders < maxSpiders) && Random.Range(0,10) > 3) heldSpiders++;

            if(heldSpiders >= maxSpiders)
            {
                heldSpiders--;
                outsideSpiders++;
                Spider newSpider = Instantiate(spiderData.objectPrefab, transform.position, Quaternion.identity).GetComponent<Spider>();
                if(absentFromGrid)
                {
                    newSpider.persistAfterNewDay = false;
                    newSpider.inWilderness = true;
                }
            }
        }
    }

    public override void HitWithWater()
    {
        DenHit();
    }

    void DenHit()
    {
        if(heldSpiders > 0 && isLarge)
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

    public override void LoadVariables()
    {
        UpdateStage(saveBool1);
    }

    public override void SaveVariables()
    {
        saveBool1 = isLarge;
    }
}
