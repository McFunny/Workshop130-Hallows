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
            ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

            gameObject.SetActive(false);
        }
    }

    public void Refresh()
    {
        int r = Random.Range(0,2);
        if(r == 0)
        {
            type = ForageType.Aloe;
        }
        else
        {
            type = ForageType.Yarrow;
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
        yield return new WaitForSeconds(1f);
        StructureInteraction();
    }

}

public enum ForageType
{
    Aloe,
    Yarrow
}
