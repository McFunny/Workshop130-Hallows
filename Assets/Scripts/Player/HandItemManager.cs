using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandItemManager : MonoBehaviour
{
    public GameObject hoe, shovel, wateringCan, shotGun, waterGun, torch;
    public GameObject torchFlame;

    ToolType currentType = ToolType.Null;

    GameObject currentHandObject;
    Animator currentAnim;
    public GameObject handSpriteTransform;
    SpriteRenderer handRenderer;

    public static HandItemManager Instance;

    public AudioSource toolSource;

    public AudioClip extinguish;

    public Transform bulletStart, waterBulletStart, waterBulletCloseStart;

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
            case ToolType.WaterGun:
                waterGun.SetActive(true);
                currentHandObject = waterGun;
                break;
            case ToolType.Torch:
                torch.SetActive(true);
                currentHandObject = torch;
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

    public Animator AccessCurrentAnimator()
    {
        if(currentAnim) return currentAnim;
        else return null;
    }

    public void ClearHandModel()
    {
        if(currentHandObject) currentHandObject.SetActive(false);
        if (handRenderer != null) handRenderer.sprite = null;
        currentType = ToolType.Null;
    }

    bool MissingObject()
    {
        if(!hoe || !shovel || !wateringCan || !shotGun || !torch)
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

    public void DoesShotgunReload(bool hasShotgunAmmoLeft)
    {
        if (currentAnim)
        {
            if (hasShotgunAmmoLeft == false)
            {
                currentAnim.SetBool("HasAmmoLeft", false);
            }
            else
            {
                currentAnim.SetBool("HasAmmoLeft", true);
            }
        }
    }

    public void TorchFlameToggle(bool ignite)
    {
        if((PlayerInteraction.Instance.torchLit && ignite) || (!PlayerInteraction.Instance.torchLit && !ignite)) return;

        if(ignite)
        {
            PlayerInteraction.Instance.torchLit = true;
            torchFlame.SetActive(true);
        }
        else
        {
            toolSource.PlayOneShot(extinguish);
            if(currentHandObject == torch) ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = torchFlame.transform.position;
            PlayerInteraction.Instance.torchLit = false;
            torchFlame.SetActive(false);
        }
    }
}
