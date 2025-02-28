using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forgeable : StructureBehaviorScript
{
    //public InventoryItemData aloe, yarrow;
    public SpriteRenderer cropRenderer;
    //public Sprite aloeSprite, yarrowSprite;
    public Transform itemDropTransform;

    public ForageableItem type; //current Type
    public ForageableItem[] possibleItems;

    bool isDigging = false;
    bool usingShovel = false;


    // Start is called before the first frame update
    void Awake()
    {
        audioHandler = GetComponent<StructureAudioHandler>();
    }

    void Start()
    {
        SpriteChange();
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

            droppedItem = ItemPoolManager.Instance.GrabItem(type.item);
            droppedItem.transform.position = transform.position;

            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

            gameObject.SetActive(false);
        }
    }

    public void Refresh(bool inWilderness)
    {
        bool decided = false;
        int t = 0;
        type = possibleItems[0];
        while(!decided && t < 10)
        {
            int r = Random.Range(0, possibleItems.Length);
            int p = Random.Range(0,100);
            if(possibleItems[r].wildernessSpawnRate > p && inWilderness)
            {
                type = possibleItems[r];
                decided = true;
            }
            else if(possibleItems[r].townSpawnRate > p && !inWilderness)
            {
                type = possibleItems[r];
                decided = true;
            }
            t++;
        }
        isDigging = false;
        SpriteChange();
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

    public override void HourPassed()
    {
        if(TimeManager.Instance.currentHour == 5)
        {
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
            gameObject.SetActive(false);
        }
    }

    void SpriteChange()
    {
        /*if(type == ForageType.Aloe)
        {
            cropRenderer.sprite = aloeSprite;
        }

        if(type == ForageType.Yarrow)
        {
            cropRenderer.sprite = yarrowSprite;
        }*/

        cropRenderer.sprite = type.sprite;
    }


    IEnumerator DigPlant()
    {
        yield return new WaitForSeconds(1f);
        usingShovel = true;
        StructureInteraction();
    }

}

public enum ForageType
{
    Aloe,
    Yarrow
}

[System.Serializable]
public class ForageableItem
{
    //public ForageType type;
    public InventoryItemData item;
    public int wildernessSpawnRate;
    public int townSpawnRate;
    public Sprite sprite;
}
