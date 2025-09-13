using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BugPuzzleManager : ImAPuzzleManager, IInteractable
{
    public static BugPuzzleManager Instance;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;
    public InventoryItemData wantedItem;
    private int itemsDeposited;
    public int itemsNeeded;

    public GameObject structureUI;
    public TextMeshProUGUI structureUItext;
    public SpriteRenderer itemWantedSprite;

    private AudioSource audioSource;

    public Transform particlePoint;

    [Header("Gachapon Stuff")]
    public InventoryItemData gachaponReward;
    public int gachaponRewardCount;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    public void EndInteraction()
    {
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        //Add a check for if puzzle is complete
        //Add QOL for 75 items at one time
        if (puzzleSolved) return;
        if (item == wantedItem && !puzzleSolved)
        {
            if (HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize >= 5 && itemsDeposited+5 <= itemsNeeded)
            {
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(5);
                interactor.playerInventoryHolder.UpdateInventory();
                itemsDeposited = itemsDeposited + 5;
                audioSource.PlayOneShot(audioSource.clip);
               /* GameObject particle = ParticlePoolManager.Instance.GrabExtinguishParticle();
                particle.transform.position = particlePoint.position;*/
                CheckToSeeIfSolved();
                interactSuccessful = true;
            }
            else if (itemsDeposited < itemsNeeded) 
            {
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                interactor.playerInventoryHolder.UpdateInventory();
                itemsDeposited++;
                audioSource.PlayOneShot(audioSource.clip);
               /* GameObject particle = ParticlePoolManager.Instance.GrabExtinguishParticle();
                particle.transform.position = particlePoint.position;*/
                CheckToSeeIfSolved();
                interactSuccessful = true;
            }

        }
    }

        private void CheckToSeeIfSolved()
    {
        if (itemsDeposited >= itemsNeeded)
        {
            puzzleSolved = true;
            fireObject.SetActive(puzzleSolved);
            ToggleHighlight(false);
            itemWantedSprite.color = Color.white;
            Gachapon.Instance.AddToBacklog(gachaponReward, gachaponRewardCount);
            PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
        }
    }

    public void ReturnFocalPoint(out Transform point)
    {
        throw new System.NotImplementedException();
    }

    public void ToggleHighlight(bool enable)
    {
        if (highlight.Count == 0) return;
        if (highlightMaterial.Count == 0)
        {
            foreach (GameObject thing in highlight)
                highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if (enable && !highlightEnabled && !puzzleSolved)
        {
            highlightEnabled = true;
            foreach (GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
            structureUI.SetActive(true);
        }

        if (!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach (GameObject thing in highlight) thing.SetActive(false);
            structureUI.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while (highlightEnabled)
        {

            do
            {
                structureUItext.text = itemsDeposited + " / " + itemsNeeded;
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while (power > 0.7f && highlightEnabled);
            do
            {
                structureUItext.text = itemsDeposited + " / " + itemsNeeded;
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while (power < 1.9f && highlightEnabled);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        itemWantedSprite.sprite = wantedItem.icon;

        if(!puzzleSolved) itemWantedSprite.color = Color.black;
        audioSource = GetComponent<AudioSource>();
    }

    public BugPuzzleSaveData ExportSaveData()
    {
        return new BugPuzzleSaveData
        {
            itemsNeededData = itemsNeeded,
            itemsDepositedData = itemsDeposited,
            itemID = wantedItem.ID,
            isSolvedData = puzzleSolved
        };
    }

    public void ImportSaveData(BugPuzzleSaveData data)
    {
        if(data.itemsNeededData == 0) return;

        itemsNeeded = data.itemsNeededData;
        itemsDeposited = data.itemsDepositedData;
        wantedItem = Database.Instance.GetItem(data.itemID);
        puzzleSolved = data.isSolvedData;
        fireObject.SetActive(puzzleSolved);

        itemWantedSprite.sprite = wantedItem.icon;
        itemWantedSprite.color = puzzleSolved ? Color.white : Color.black;

    }

}


[System.Serializable]
public struct BugPuzzleSaveData
{
    public int itemsNeededData;
    public int itemsDepositedData;
    public int itemID;
    public bool isSolvedData;
}

