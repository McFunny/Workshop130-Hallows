using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boulder : StructureBehaviorScript
{
    public List<GameObject> rockVariations;

    public InventoryItemData rocks, gold;

    float damageThreshold = 10;

    bool dropItems = false;

    int rockNum = -1;

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
        if(rockNum == -1) rockNum = Random.Range(0, rockVariations.Count);
        foreach(GameObject rock in rockVariations) rock.SetActive(false);
        rockVariations[rockNum].SetActive(true);
    }

    void OnDestroy()
    {
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
    }

    void Damaged(float damage)
    {
        if(damage >= damageThreshold)
        {
            dropItems = true;
            health = 0;
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
