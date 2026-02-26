using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafPile : StructureBehaviorScript
{
    public List<ItemWithAmount> items = new List<ItemWithAmount>();

    public List<ObjectWithProbability> creatures = new List<ObjectWithProbability>();

    public GameObject leafParticles;

    public GameObject spiderPrefab;

    public bool holdSpiders;

    public override void HitWithWater()
    {
        Destroy(this.gameObject);
    }


    void OnTriggerEnter(Collider other)
    {
        Destroy(this.gameObject);
    }

    public void OnDestroy()
    {
        base.OnDestroy();
        if(!gameObject.scene.isLoaded) return;

        leafParticles.SetActive(true);
        leafParticles.transform.parent = null;

        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = transform.position;

        StructureManager.Instance.PopulateDecorObject(transform.position, DecorType.Leaf);

        if(holdSpiders)
        {
            Instantiate(spiderPrefab, transform.position, Quaternion.identity);
            return;
        }

        InventoryItemData item = null;
        GameObject chosenCreature = null;
        int x = 0;

        bool spawnCreature = false;
        if(!TimeManager.Instance.isDay && Random.Range(0,20) > 12) spawnCreature = true;
        else if(Random.Range(0,20) > 16 && !TimeManager.Instance.stopTime) spawnCreature = true;

        else if(Random.Range(0,20) > 6) return;

        if(spawnCreature)
        {
            while(!chosenCreature && x < 20)
            {
                int r = Random.Range(0, creatures.Count);
                if(Random.Range(0, 100) < creatures[r]._probability) chosenCreature = creatures[r]._object;
                x++;
            }
            if(chosenCreature) Instantiate(chosenCreature, transform.position, Quaternion.identity);
        }
        else
        {
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
            }
        }
    }

    public override void LoadVariables()
    {
        holdSpiders = saveBool1;
    }

    public override void SaveVariables()
    {
        saveBool1 = holdSpiders;
    }
}
