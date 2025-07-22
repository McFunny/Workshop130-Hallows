using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BlackjackBettingBox : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;
    public InventoryItemData wantedItem;
    public int itemsDeposited;


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
        if (itemsDeposited > 0 && !BlackjackManager.Instance.activeGame)
        {
            if (interactor.playerInventoryHolder.IsInventoryFull() == false)
            {
                interactor.playerInventoryHolder.AddToInventory(wantedItem, itemsDeposited);
                itemsDeposited = 0;
                interactSuccessful = true;
            }
        }

    }

        public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if (item == wantedItem && !BlackjackManager.Instance.activeGame)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            interactor.playerInventoryHolder.UpdateInventory();
            itemsDeposited++;
            audioSource.PlayOneShot(audioSource.clip);
            //GameObject particle = ParticlePoolManager.Instance.GrabExtinguishParticle();
            //particle.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f); // scale it up uniformly

            //particle.transform.position = particlePoint.position;
            interactSuccessful = true;
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
        if (enable && !highlightEnabled)
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
                structureUItext.text = itemsDeposited.ToString();
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while (power > 0.7f && highlightEnabled);
            do
            {
                structureUItext.text = itemsDeposited.ToString();
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
        audioSource = GetComponent<AudioSource>();
    }

    public BettingSaveData ExportSaveData()
    {
        return new BettingSaveData
        {
          
            itemsDepositedData = itemsDeposited,
            itemID = wantedItem.ID,

        };
    }

    public void ImportSaveData(BettingSaveData data)
    {
      
        itemsDeposited = data.itemsDepositedData;
        wantedItem = Database.Instance.GetItem(data.itemID);
       

        itemWantedSprite.sprite = wantedItem.icon;
      

    }
}

[System.Serializable]

public struct BettingSaveData
{
    public int itemsNeededData;
    public int itemsDepositedData;
    public int itemID;
    public bool isSolvedData;
}
