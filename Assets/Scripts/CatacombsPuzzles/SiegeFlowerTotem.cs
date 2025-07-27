using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SiegeFlowerTotem : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;
    public InventoryItemData wantedItem;
    private int itemsDeposited;
    public int itemsNeeded;
    public bool isSolved = false;

    public GameObject structureUI;
    public TextMeshProUGUI structureUItext;
    public SpriteRenderer itemWantedSprite;

    private AudioSource audioSource;

    public Transform particlePoint;


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
        if (item == wantedItem)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            interactor.playerInventoryHolder.UpdateInventory();
            itemsDeposited++;
            audioSource.PlayOneShot(audioSource.clip);
            GameObject particle = ParticlePoolManager.Instance.GrabExtinguishParticle();
            particle.transform.position = particlePoint.position;
            CheckToSeeIfSolved();
            interactSuccessful = true;
        }
    }

    private void CheckToSeeIfSolved()
    {
        if (itemsDeposited == itemsNeeded)
        {
            isSolved = true;
            ToggleHighlight(false);
            itemWantedSprite.color = Color.white;
            ShrineBoxManager.Instance.CheckToSeeIfSolved();
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
        if (enable && !highlightEnabled && !isSolved)
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
        itemWantedSprite.color = Color.black;
        audioSource = GetComponent<AudioSource>();
    }

    public SiegeFlowerSaveData ExportSaveData()
    {
        return new SiegeFlowerSaveData
        {
            itemsNeededData = itemsNeeded,
            itemsDepositedData = itemsDeposited,
            itemID = wantedItem.ID,
            isSolvedData = isSolved
        };
    }

    public void ImportSaveData(SiegeFlowerSaveData data)
    {
        itemsNeeded = data.itemsNeededData;
        itemsDeposited = data.itemsDepositedData;
        wantedItem = Database.Instance.GetItem(data.itemID);
        isSolved = data.isSolvedData;

        itemWantedSprite.sprite = wantedItem.icon;
        itemWantedSprite.color = isSolved ? Color.white : Color.black;

    }

}


[System.Serializable]
public struct SiegeFlowerSaveData
{
    public int itemsNeededData;
    public int itemsDepositedData;
    public int itemID;
    public bool isSolvedData;
}

