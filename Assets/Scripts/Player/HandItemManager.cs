using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandItemManager : MonoBehaviour
{
    public GameObject hoe, shovel, wateringCan, shotGun, waterGun, torch, bugNet, scythe, pyrefly, hydrofly, kukri, flintlock, nutTester, seedPod;
    public GameObject wateringCanUpgrade, scytheUpgrade, torchUpgrade, hoeUpgrade;
    public GameObject torchFlame, pyreflyFlame, torchUpgradeFlame;
    public MeshRenderer pyreflyMat;
    public Material pyreflyLit, pyreflyUnlit;

    Vector3 hoePos, shovelPos, wateringCanPos, shotGunPos, waterGunPos, torchPos, bugNetPos, scythePos, pyreflyPos; //starting positions, unused
    Quaternion hoeRot, shovelRot, wateringCanRot, shotGunRot, waterGunRot, torchRot, bugNetRot, scytheRot, pyreflyRot; //starting rotations, unused

    ToolType currentType = ToolType.Null;

    GameObject currentHandObject;
    Animator currentAnim;
    public GameObject handSpriteTransform;
    SpriteRenderer handRenderer;

    public static HandItemManager Instance;

    public AudioSource toolSource, watercanSource, torchSource, watercanUpgradeSource;

    public AudioClip extinguish;

    public Transform bulletStart, waterBulletStart, waterBulletCloseStart;

    public ParticleSystem waterCanParticles, pistolParticles, waterCanUpgradeParticles, flameThrowerParticles, parryParticles;
    public TrailRenderer scytheTrail;

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
        //InitializeStartingVectors();
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
        if(currentAnim)
        {
            if(PlayerMovement.Instance.IsMoving()) currentAnim.SetBool("IsWalking", true);
            else currentAnim.SetBool("IsWalking", false);
        }
    }

    public void SwapHandModel(ToolType type, bool isUpgrade)
    {
        if (MissingObject() || type == currentType) return;
        if (currentHandObject) currentHandObject.SetActive(false);
        if (handRenderer != null) handRenderer.sprite = null;
        switch (type)
        {
            case ToolType.Hoe:
                if(isUpgrade)
                {
                    hoeUpgrade.SetActive(true);
                    currentHandObject = hoeUpgrade;
                }
                else
                {
                    hoe.SetActive(true);
                    currentHandObject = hoe;
                }
                //hoe.transform.position = hoePos;
                //hoe.transform.rotation = hoeRot;
                break;
            case ToolType.Shovel:
                shovel.SetActive(true);
                currentHandObject = shovel;
                //shovel.transform.position = shovelPos;
                //shovel.transform.rotation = shovelRot;
                break;
            case ToolType.WateringCan:
                if(isUpgrade)
                {
                    wateringCanUpgrade.SetActive(true);
                    currentHandObject = wateringCanUpgrade;
                }
                else
                {
                    wateringCan.SetActive(true);
                    currentHandObject = wateringCan;
                }
                //wateringCan.transform.position = wateringCanPos;
                //wateringCan.transform.rotation = wateringCanRot;
                break;
            case ToolType.ShotGun:
                shotGun.SetActive(true);
                currentHandObject = shotGun;
                //shotGun.transform.position = shotGunPos;
                //shotGun.transform.rotation = shotGunRot;
                break;
            case ToolType.WaterGun:
                waterGun.SetActive(true);
                currentHandObject = waterGun;
                //waterGun.transform.position = waterGunPos;
                //waterGun.transform.rotation = waterGunRot;
                break;
            case ToolType.Torch:
                if(isUpgrade)
                {
                    torchUpgrade.SetActive(true);
                    currentHandObject = torchUpgrade;
                }
                else
                {
                    torch.SetActive(true);
                    currentHandObject = torch;
                }
                //torch.transform.position = torchPos;
                //torch.transform.rotation = torchRot;
                break;
            case ToolType.BugNet:
                bugNet.SetActive(true);
                currentHandObject = bugNet;
                //bugNet.transform.position = bugNetPos;
                //bugNet.transform.rotation = bugNetRot;
                break;
            case ToolType.Scythe:
                if(isUpgrade)
                {
                    scytheUpgrade.SetActive(true);
                    currentHandObject = scytheUpgrade;
                }
                else
                {
                    scythe.SetActive(true);
                    currentHandObject = scythe;
                }
                //scythe.transform.position = scythePos;
                //scythe.transform.rotation = scytheRot;
                break;
            case ToolType.Pyrefly:
                pyrefly.SetActive(true);
                currentHandObject = pyrefly;
                //pyrefly.transform.position = pyreflyPos;
                //pyrefly.transform.rotation = pyreflyRot;
                break;
            case ToolType.Hydrofly:
                hydrofly.SetActive(true);
                currentHandObject = hydrofly;
                break;
            case ToolType.Kukri:
                kukri.SetActive(true);
                currentHandObject = kukri;
                break;
            case ToolType.Flintlock:
                flintlock.SetActive(true);
                currentHandObject = flintlock;
                break;
            case ToolType.NutTester:
                nutTester.SetActive(true);
                currentHandObject = nutTester;
                break;
            case ToolType.SeedPod:
                seedPod.SetActive(true);
                currentHandObject = seedPod;
                break;
            default:
                currentHandObject = null;
                break;
        }
        if(currentHandObject) currentAnim = currentHandObject.GetComponent<Animator>();
        if(!currentAnim && currentHandObject) currentAnim = currentHandObject.GetComponentInChildren<Animator>();
        currentType = type;
    }

    public bool IsPlayerHoldingTorch()
    {
        if(currentType == ToolType.Torch) return true;
        else return false;
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
        if(!hoe || !shovel || !wateringCan || !shotGun || !torch || !bugNet || !scythe)
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
                SwapHandModel(t_item.tool, t_item.isUpgrade);
            }
            else SwapHandModel(ToolType.Null, false);
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

    public void TorchFlameToggle(bool ignite, bool inventoryExtinguish = false)
    {
        if((PlayerInteraction.Instance.torchLit && ignite) || (!PlayerInteraction.Instance.torchLit && !ignite)) return;

        if(ignite)
        {
            PlayerInteraction.Instance.torchLit = true;
            torchFlame.SetActive(true);
            torchUpgradeFlame.SetActive(true);
        }
        else
        {
            if(/*currentHandObject == torch &&*/ inventoryExtinguish)
            {
                ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = torchFlame.transform.position;
                toolSource.PlayOneShot(extinguish);
            } 
            PlayerInteraction.Instance.torchLit = false;
            torchFlame.SetActive(false);
            torchUpgradeFlame.SetActive(false);
        }
    }

    public void PyreflyFlameToggle(bool ignite)
    {
        if((PlayerInteraction.Instance.pyreflyLit && ignite) || (!PlayerInteraction.Instance.pyreflyLit && !ignite)) return;

        if(ignite)
        {
            PlayerInteraction.Instance.pyreflyLit = true;
            pyreflyFlame.SetActive(true);
            if(pyreflyMat) pyreflyMat.material = pyreflyLit;
        }
        else
        {
            PlayerInteraction.Instance.pyreflyLit = false;
            pyreflyFlame.SetActive(false);
            if(pyreflyMat) pyreflyMat.material = pyreflyUnlit;
        }
    }

    public ToolType GetCurrentType()
    {
        return currentType;
    }

    public ParticleSystem GetWaterCanParticles(bool isUpgrade)
    {
        if(isUpgrade) return waterCanUpgradeParticles;
        else return waterCanParticles;
    }

    void InitializeStartingVectors() //no worky
    {
        hoePos = hoe.transform.position;
        hoeRot = hoe.transform.rotation;

        shovelPos = shovel.transform.position;
        shovelRot = shovel.transform.rotation;

        wateringCanPos = wateringCan.transform.position;
        wateringCanRot = wateringCan.transform.rotation;

        shotGunPos = shotGun.transform.position;
        shotGunRot = shotGun.transform.rotation;

        waterGunPos = waterGun.transform.position;
        waterGunRot = waterGun.transform.rotation;

        torchPos = torch.transform.position;
        torchRot = torch.transform.rotation;

        bugNetPos = bugNet.transform.position;
        bugNetRot = bugNet.transform.rotation;

        scythePos = scythe.transform.position;
        scytheRot = scythe.transform.rotation;

        pyreflyPos = pyrefly.transform.position;
        pyreflyRot = pyrefly.transform.rotation;
    }
}
