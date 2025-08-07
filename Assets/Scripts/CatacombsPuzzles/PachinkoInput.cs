using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class PachinkoInput : MonoBehaviour, IInteractable
{
    public bool isInput = false;
    public bool isOutput = false;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;
    public InventoryItemData inputItem;
    public InventoryItemData outputItem;
    private int itemsDeposited;
    public bool isSolved = false;

    public GameObject structureUI;
    public TextMeshProUGUI structureUItext;

    private AudioSource audioSource;

    public Transform particlePoint;


    public void EndInteraction()
    {
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (isOutput)
        {
            Debug.Log($"Trying to output. totalWinnings: {PachinkoManager.Instance.totalWinnings}");

            if (PachinkoManager.Instance.totalWinnings > 0 && interactor.playerInventoryHolder.IsInventoryFull() == false)
            {
                Debug.Log("Depositing winnings into inventory");
                bool didItHappen = interactor.playerInventoryHolder.AddToInventory(outputItem, PachinkoManager.Instance.totalWinnings);
                Debug.Log($"Did it happens: {didItHappen}");
                PachinkoManager.Instance.totalWinnings = 0;
                PachinkoManager.Instance.UpdateText();
                interactSuccessful = true;
            }
        }
    }


    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if (item == inputItem && isInput)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            interactor.playerInventoryHolder.UpdateInventory();
            audioSource.PlayOneShot(audioSource.clip);
            GameObject particle = ParticlePoolManager.Instance.GrabPoofParticle();
            particle.transform.position = particlePoint.position;
            interactSuccessful = true;

            if (PachinkoManager.Instance.activeBug == null)
            {
                PachinkoManager.Instance.bugsInserted++;
                PachinkoManager.Instance.SpawnBug();
            }
            else
            {
                PachinkoManager.Instance.bugsInserted++;
                PachinkoManager.Instance.UpdateText();
            }
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
            if (structureUI) structureUI.SetActive(true);
        }

        if (!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach (GameObject thing in highlight) thing.SetActive(false);
            if (structureUI) structureUI.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while (highlightEnabled)
        {

            do
            {
                //structureUItext.text = itemsDeposited + " / " + itemsNeeded;
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while (power > 0.7f && highlightEnabled);
            do
            {
                //structureUItext.text = itemsDeposited + " / " + itemsNeeded;
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
        
        audioSource = GetComponent<AudioSource>();
    }
}
