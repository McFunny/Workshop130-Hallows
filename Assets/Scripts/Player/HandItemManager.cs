using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandItemManager : MonoBehaviour
{
    public GameObject hoe, shovel, wateringCan;

    GameObject currentHandObject;
    Animator currentAnim;
    public GameObject handSpriteTransform;
    SpriteRenderer handRenderer;

    public static HandItemManager Instance;

    public AudioSource toolSource;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwapHandModel(ToolType type)
    {
        if(currentHandObject) currentHandObject.SetActive(false);
        if(MissingObject()) return;
        handRenderer.sprite = null;
        switch (type)
        {
            case ToolType.Hoe:
                hoe.SetActive(true);
                currentHandObject = hoe;
                break;
            case ToolType.Shovel:
                shovel.SetActive(true);
                currentHandObject = shovel;
                break;
            case ToolType.WateringCan:
                wateringCan.SetActive(true);
                currentHandObject = wateringCan;
                break;
            default:
            currentHandObject = null;
                break;
        }
        if(currentHandObject) currentAnim = currentHandObject.GetComponent<Animator>();
    }

    public void ShowSpriteInHand(InventoryItemData item)
    {
        handRenderer.sprite = item.icon;
    }

    public void PlayPrimaryAnimation()
    {
        if(currentAnim) currentAnim.SetTrigger("PrimaryTrigger");
    }

    public void PlaySecondaryAnimation()
    {
        if (currentAnim) currentAnim.SetTrigger("SecondaryTrigger");
    }

    public void ClearHandModel()
    {
        if(currentHandObject) currentHandObject.SetActive(false);
    }

    bool MissingObject()
    {
        if(!hoe || !shovel || !wateringCan)
        {
            Debug.Log("Missing a reference to a hand object");
            return true;
        }
        else return false;
    }

    public void CheckSlotForTool()
    {
        InventorySlot slot = HotbarDisplay.currentSlot.AssignedInventorySlot;
        if (slot != null && slot.ItemData != null)
        {           
            ToolItem t_item = slot.ItemData as ToolItem;
            if(t_item)
            {
                SwapHandModel(t_item.tool);
            }
            else SwapHandModel(ToolType.Null);
        }
        else
        {
            ClearHandModel();
        }
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        handRenderer = handSpriteTransform.GetComponent<SpriteRenderer>();
        CheckSlotForTool();
    }

}
