using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandItemManager : MonoBehaviour
{
    public GameObject hoe, shovel, wateringCan, shotGun;

    ToolType currentType = ToolType.Null;

    GameObject currentHandObject;
    Animator currentAnim;
    public GameObject handSpriteTransform;
    SpriteRenderer handRenderer;

    public static HandItemManager Instance;

    public AudioSource toolSource;

    public Transform bulletStart;

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
        handRenderer = handSpriteTransform.GetComponent<SpriteRenderer>();
        StartCoroutine(DelayedStart());
        if(!bulletStart) Debug.Log("You are missing the transform for where shotgun bullets strt from, which is located on the player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwapHandModel(ToolType type)
    {
        if (MissingObject() || type == currentType) return;
        if (currentHandObject) currentHandObject.SetActive(false);
        if (handRenderer != null) handRenderer.sprite = null;
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
            case ToolType.ShotGun:
                shotGun.SetActive(true);
                currentHandObject = shotGun;
                break;
            default:
            currentHandObject = null;
                break;
        }
        if(currentHandObject) currentAnim = currentHandObject.GetComponent<Animator>();
        if(!currentAnim) currentAnim = currentHandObject.GetComponentInChildren<Animator>();
        currentType = type;
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
        if (handRenderer != null) handRenderer.sprite = null;
    }

    bool MissingObject()
    {
        if(!hoe || !shovel || !wateringCan || !shotGun)
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
        CheckSlotForTool();
    }

}
