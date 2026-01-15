using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimePillar : StructureBehaviorScript
{
    public List<GameObject> slimeObjects;

    public Transform creatureSpawn;

    public InventoryItemData rocks, slime;

    float damageThreshold = 10;

    bool dropItems = false;

    public CreatureObject slimeData;

    public int slimeValue = 3;
    int maxSlimeValue = 3;

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

        StartCoroutine(SpawnSlimes());
    }

    void UpdateModel()
    {
        int i = 0;
        foreach(GameObject slimeObject in slimeObjects)
        {
            if(i < slimeValue) slimeObject.SetActive(true);
            else slimeObject.SetActive(false);
            i++;
        }
    }

    IEnumerator SpawnSlimes()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(15, 30));
            if(slimeValue < 2 || TimeManager.Instance.isDay) continue;

            Instantiate(slimeData.objectPrefab, creatureSpawn.position, Quaternion.identity);

            if(slimeValue == 3 && Random.Range(0,2) == 1) Instantiate(slimeData.objectPrefab, creatureSpawn.position, Quaternion.identity);

            ParticlePoolManager.Instance.GrabSlimeSplashParticle().transform.position = creatureSpawn.position;

            audioHandler.PlaySound(audioHandler.activatedSound);
        }
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.isDay) return;

        slimeValue += Random.Range(0,3);
        if(slimeValue > maxSlimeValue) slimeValue = maxSlimeValue;
        UpdateModel();
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && slimeValue > 0)
        {
            ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
            PlayerInteraction.Instance.WaterChange(-1);
            success = true;
            ReduceSlime();
        }
    }

    public override void HitWithWater()
    {
        ReduceSlime();
    }

    void ReduceSlime()
    {
        if(slimeValue <= 0) return;
        slimeValue--;
        ParticlePoolManager.Instance.GrabSlimeSplashParticle().transform.position = particleCenter.position;
        UpdateModel();
        audioHandler.PlaySound(audioHandler.itemInteractSound);
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

        if(slimeValue < 1) return;

        itemsToDrop = Random.Range(1, 3);
        for(int i = 0; i < itemsToDrop; i++)
        {
            //if(Random.Range(0, 100) > 95) droppedItem = ItemPoolManager.Instance.GrabItem(gold);
            /*else */droppedItem = ItemPoolManager.Instance.GrabItem(slime);
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
}
