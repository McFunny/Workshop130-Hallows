using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SiegeFlowerTotem : MonoBehaviour, IInteractable
{
    [Header("Visuals")]
    public SpriteRenderer r;

    [Header("Item Setup")]
    public Pottable flowerRequired;
    public InventoryItemData requiredItem;

    [Header("State")]
    public InventoryItemData currentItem;
    public bool isLocked;
    public bool cantDigUp;

    [Header("Highlight")]
    public List<GameObject> highlight = new List<GameObject>();
    private List<Material> highlightMaterial = new List<Material>();
    private Coroutine highlightCoroutine;
    public bool canShowHighlight = true;
    public GameObject structureUI;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void Awake()
    {
     
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

        if (flowerRequired.item == item)
        {
            currentItem = item;
            isLocked = true;
            UpdateVisual();
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            UpdateFlower(item);
            interactSuccessful = true;
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
        isLocked = true;
        UpdateVisual();
        UpdateFlower(item);
    }


    private void UpdateVisual()
    {
        r.sprite = null;

        if (currentItem == null) return;

        if (flowerRequired.item == currentItem)
        {
            r.sprite = flowerRequired.sprite;
        }
    }


    public void UpdateFlower(InventoryItemData flower)
    {
        SiegeFlowerPuzzleManager.Instance.CheckToSeeIfSolved();
    }

    public SiegeFlowerSaveData ExportSaveData()
    {
        return new SiegeFlowerSaveData
        {
            currentItemID = currentItem != null ? currentItem.ID : -1,
            isLockedData = isLocked
        };
    }



    public void ImportSaveData(SiegeFlowerSaveData data)
    {
        isLocked = data.isLockedData;

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
public struct SiegeFlowerSaveData
{
    public int currentItemID;
    public bool isLockedData;
}


