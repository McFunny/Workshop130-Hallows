using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Digsite : StructureBehaviorScript
{
    public Transform itemDropTransform;

    public InventoryItemData[] possibleItems;
    public float[] itemProbabilities;

    bool isDigging = false;
    bool usingShovel = false;


    // Start is called before the first frame update
    void Awake()
    {
        audioHandler = GetComponent<StructureAudioHandler>();
    }

    void Start()
    {
        //base.Start()
    }
    
    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        if(!isDigging && usingShovel)
        {
            audioHandler.PlaySound(audioHandler.interactSound);
            isDigging = true;
            GameObject droppedItem;
            InventoryItemData newItem = DroppedItem();
            if(newItem != null)
            {
                droppedItem = ItemPoolManager.Instance.GrabItem(newItem);
                droppedItem.transform.position = transform.position;
            }

            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

            Destroy(gameObject);
        }
    }

    public InventoryItemData DroppedItem()
    {
        InventoryItemData item = null;
        bool decided = false;
        int t = 0;
        while(!decided && t < 5)
        {
            int r = Random.Range(0, possibleItems.Length);
            int p = Random.Range(0,100);
            if(itemProbabilities[r] > p)
            {
                item = possibleItems[r];
                decided = true;
            }
            t++;
        }
        return item;
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !isDigging)
        {
            StartCoroutine(DigPlant());
            success = true;
        }
    }



    IEnumerator DigPlant()
    {
        yield return new WaitForSeconds(1f);
        usingShovel = true;
        StructureInteraction();
    }

}
