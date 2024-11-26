using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forgeable : StructureBehaviorScript
{
    public InventoryItemData aloe, yarrow;
    public SpriteRenderer cropRenderer;
    public Sprite aloeSprite, yarrowSprite;
    public Transform itemDropTransform;

    public ForageType type;

    bool isDigging = false;


    // Start is called before the first frame update
    void Awake()
    {
        base.Awake();

        wealthValue = 0;
    }

    void Start()
    {
        SpriteChange();

    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        if(!isDigging)
        {
            audioHandler.PlaySound(audioHandler.interactSound);
            isDigging = true;
            GameObject droppedItem;

            if(type == ForageType.Aloe)
            {
                droppedItem = ItemPoolManager.Instance.GrabItem(aloe);
                droppedItem.transform.position = transform.position;
            }

            if(type == ForageType.Yarrow)
            {
                droppedItem = ItemPoolManager.Instance.GrabItem(yarrow);
                droppedItem.transform.position = transform.position;
            }

            SpriteChange();
        }
    }

    public void Refresh()
    {
        //
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
        
    }

    void SpriteChange()
    {
        if(type == ForageType.Aloe)
        {
            cropRenderer.sprite = aloeSprite;
        }

        if(type == ForageType.Yarrow)
        {
            cropRenderer.sprite = yarrowSprite;
        }
    }


    IEnumerator DigPlant()
    {
        isDigging = true;
        yield return new WaitForSeconds(1f);
        StructureInteraction();
    }

    void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return; 
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        base.OnDestroy();
    }
}

public enum ForageType
{
    Aloe,
    Yarrow
}
