using System;
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
    public SpriteRenderer structureUIRenderer;

    [Header("Gachapon Stuff")]
    public InventoryItemData gachaponReward;
    public int gachaponRewardCount;


    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void Awake()
    {
     
        foreach (var obj in highlight) obj.SetActive(false);
        //if (structureUI) structureUIRenderer.enabled = false;
    }

    private void Start()
    {
        structureUIRenderer.sprite = requiredItem.icon;
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
            if (structureUI) structureUI.SetActive(false);
            UpdateVisual();
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            UpdateFlower(item);
            interactSuccessful = true;
            Gachapon.Instance.AddToBacklog(gachaponReward, gachaponRewardCount);
            UpdateSieges();
        }
    }

    private void UpdateSieges()
    {
        GameSaveData.Instance.siegesCleared++;
        AchievementManager.Instance.NotifySiegeCompleted(GameSaveData.Instance.siegesCleared);
        switch (GameSaveData.Instance.siegesCleared)
        {
            case 1:
                QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[9]);
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[10]);
                GameSaveData.Instance.bot_newWares = true;
                GameSaveData.Instance.tink_newWares = true;
                break;
            case 2:
                QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[10]);
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[11]);
                GameSaveData.Instance.bot_newWares = true;
                GameSaveData.Instance.tink_newWares = true;
                break;
            case 3:
                QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[11]);
                QuestManager.Instance.AddQuest(QuestDatabase.Instance.MainQuests[12]);
                GameSaveData.Instance.bot_newWares = true;
                break;
            case 4:
                QuestManager.Instance.ForceCompleteQuest(QuestDatabase.Instance.MainQuests[12]);
                break;
            default:
                break;
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
        if(isLocked == true)
        {
            //if (structureUI) structureUI.SetActive(false);
        }

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
            //if (structureUI) structureUIRenderer.enabled = enable;
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
            //if (structureUI) structureUIRenderer.enabled = true;
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
        //if (structureUI) structureUIRenderer.enabled = false;
    }

    private IEnumerator HightlightFlash()
    {
        float power = 1f;
        while (true)
        {
            while (power > 1f)
            {
                //structureUIRenderer.enabled = true;
                power -= 0.1f;
                foreach (Material mat in highlightMaterial)
                    mat.SetFloat("_Fresnel_Power", power);
                yield return new WaitForSeconds(0.1f);
            }

            while (power < 2.5f)
            {
                //structureUIRenderer.enabled = true;
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


