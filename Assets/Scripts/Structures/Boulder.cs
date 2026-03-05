using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : StructureBehaviorScript
{
    public List<GameObject> rockVariations;

    public InventoryItemData rocks, gold;

    float damageThreshold = 10;

    bool dropItems = false;

    public int rockNum = -1;

    public StructureObject geyserData;

    void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

        OnDamageWithValue += Damaged;

        UpdateModel();
    }

    void UpdateModel()
    {
        if(rockNum == -1)
        {
            rockNum = Random.Range(0, 3);
            //if(Random.Range(0,30) == 1) rockNum = 3;
        }
        foreach(GameObject rock in rockVariations) rock.SetActive(false);
        rockVariations[rockNum].SetActive(true);
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.currentHour == 8 && Random.Range(0,150) == 1 && StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm)) //Turn into a plugged geyser
        {
            rockNum = 3;
            UpdateModel();
        }
    }

    void OnDestroy()
    {
        if(rockNum == 3) clearTileOnDestroy = false;

        OnDamageWithValue -= Damaged;
        base.OnDestroy();

        if (!gameObject.scene.isLoaded || !dropItems) return; 

        GameObject droppedItem;
        int itemsToDrop = Random.Range(1, 5);
        for(int i = 0; i < itemsToDrop; i++)
        {
            //if(Random.Range(0, 100) > 95) droppedItem = ItemPoolManager.Instance.GrabItem(gold);
            /*else */droppedItem = ItemPoolManager.Instance.GrabItem(rocks);
            droppedItem.transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);

            Vector3 dir3 = Random.onUnitSphere;
            dir3 = new Vector3(dir3.x, droppedItem.transform.position.y, dir3.z);
            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(dir3 * 35);
            itemRB.AddForce(Vector3.up * 50);
        }

        if(rockNum == 3)
        {
            Instantiate(geyserData.objectPrefab, transform.position, Quaternion.identity);
        }
        else StructureManager.Instance.PopulateDecorObject(transform.position, DecorType.Rock);
    }

    void Damaged(float damage)
    {
        if(damage >= damageThreshold)
        {
            dropItems = true;
            health = 0;
            AchievementManager.Instance.AddProgressWithEnum(ACHKey.Hundred_Rocks);
            Destroy(gameObject);
        }
        else
        {
            health = maxHealth;
        }
    }

    public override void LoadVariables()
    {
        rockNum = saveInt1;
        UpdateModel();
    }

    public override void SaveVariables()
    {
        saveInt1 = rockNum;
    }
}
