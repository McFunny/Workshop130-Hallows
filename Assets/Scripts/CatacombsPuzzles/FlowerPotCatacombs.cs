using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlowerPotCatacombs : MonoBehaviour, IInteractable
{
    [Header("Visuals")]
    public SpriteRenderer r;
    public GameObject fogChimeLight;

    [Header("Item Setup")]
    public List<Pottable> potItems = new List<Pottable>();
    public InventoryItemData fogChime;
    public InventoryItemData requiredItem;

    [Header("State")]
    public InventoryItemData currentItem;
    public bool isLocked;
    public bool cantDigUp;
    public bool isCorrect;

    [Header("Highlight")]
    public List<GameObject> highlight = new List<GameObject>();
    private List<Material> highlightMaterial = new List<Material>();
    private Coroutine highlightCoroutine;
    public bool canShowHighlight = true;
    public GameObject structureUI;

    public FlowerTablet flowerTablet;
    public int flowerIndex;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void Awake()
    {
        fogChimeLight?.SetActive(false);
        foreach (var obj in highlight) obj.SetActive(false);
        if (structureUI) structureUI.SetActive(false);
    }

    private void Start()
    {
        UpdateVisual();
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (isLocked || cantDigUp) return;

        if (currentItem == null) return;

        bool added = PlayerInventoryHolder.Instance.AddToInventory(currentItem, 1);
        if (added)
        {
            currentItem = null;
            UpdateVisual();
            PlayerInventoryHolder.Instance.UpdateInventory();
            interactSuccessful = true;
        }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if (isLocked || currentItem != null || item == null) return;

        foreach (var p in potItems)
        {
            if (p.item == item)
            {
                currentItem = item;
                UpdateVisual();
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();

                UpdateFlower(item);
                interactSuccessful = true;
                return;
            }
        }
    }

    public void EndInteraction() { }

    public void LockPuzzle()
    {
        isLocked = true;
    }

    public void ForceInsertItem(InventoryItemData item)
    {
        currentItem = item;
        UpdateVisual();
        UpdateFlower(item);
    }

    private void UpdateVisual()
    {
        r.sprite = null;
        fogChimeLight?.SetActive(false);

        if (currentItem == null) return;

        foreach (var p in potItems)
        {
            if (p.item == currentItem)
            {
                r.sprite = p.sprite;
                break;
            }
        }

        if (currentItem == fogChime)
        {
            fogChimeLight?.SetActive(true);
        }
    }

    public void UpdateFlower(InventoryItemData flower)
    {
        isCorrect = flower == requiredItem;
        FlowerPotManager.Instance.CheckToSeeIfSolved();
    }

    public FlowerSaveData ExportSaveData()
    {
        int itemID = currentItem != null ? currentItem.ID : -1;

        return new FlowerSaveData
        {
            isCorrectData = isCorrect,
            currentItemID = itemID,
            isLockedData = isLocked,
            flowerIndexData = flowerIndex
        };
    }


    public void ImportSaveData(FlowerSaveData data)
    {
        isCorrect = data.isCorrectData;
        isLocked = data.isLockedData;
        flowerIndex = data.flowerIndexData;

        if (data.currentItemID != -1)
        {
            currentItem = Database.Instance.GetItem(data.currentItemID);
            UpdateVisual();
        }
        else
        {
            currentItem = null;
            UpdateVisual();
        }

        GetDataForTablet(flowerIndex);
    }

    private void GetDataForTablet(int flowerIndex)
    {
        flowerTablet.activePopup = FlowerPotManager.Instance.flowerAssignmentsReference[flowerIndex].popup;
        requiredItem = FlowerPotManager.Instance.flowerAssignmentsReference[flowerIndex].flower;
    }

    public void ToggleHighlight(bool enable)
    {
        if (HideUI.hideUI) return;
        if (highlight.Count == 0) return;
        if (isLocked) { DisableHighlight(); return; }

        if (!canShowHighlight)
        {
            if (structureUI) structureUI.SetActive(enable);
            DisableHighlight();
            return;
        }

        if (highlightMaterial.Count == 0 && highlight.Count > 0)
        {
            foreach (GameObject thing in highlight)
            {
                Renderer renderer = thing.GetComponentInChildren<Renderer>();
                if (renderer != null)
                    highlightMaterial.Add(renderer.material);
            }
        }

        if (enable && highlightCoroutine == null)
        {
            foreach (var h in highlight) h.SetActive(true);
            if (structureUI) structureUI.SetActive(true);
            highlightCoroutine = StartCoroutine(HightlightFlash());
        }

        if (!enable && highlightCoroutine != null)
        {
            StopCoroutine(highlightCoroutine);
            highlightCoroutine = null;
            DisableHighlight();
        }
    }

    private void DisableHighlight()
    {
        foreach (var h in highlight) h.SetActive(false);
        if (structureUI) structureUI.SetActive(false);
    }

    private IEnumerator HightlightFlash()
    {
        float power = 1f;
        while (true)
        {
            while (power > 1f)
            {
                power -= 0.1f;
                foreach (Material mat in highlightMaterial)
                    mat.SetFloat("_Fresnel_Power", power);
                yield return new WaitForSeconds(0.1f);
            }

            while (power < 2.5f)
            {
                power += 0.1f;
                foreach (Material mat in highlightMaterial)
                    mat.SetFloat("_Fresnel_Power", power);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void ReturnFocalPoint(out Transform point)
    {
        throw new System.NotImplementedException();
    }
}



[System.Serializable]
public struct FlowerSaveData
{
    public bool isCorrectData;
    public int currentItemID;
    public bool isLockedData;
    public int flowerIndexData;
}

