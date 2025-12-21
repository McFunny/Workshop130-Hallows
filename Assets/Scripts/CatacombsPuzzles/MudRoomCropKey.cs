using System;
using UnityEngine;
using UnityEngine.Events;

public class MudRoomCropKey : MonoBehaviour, IInteractable
{
    [Header("Visuals")]
    public GameObject backgroundCropGameObject;
    public GameObject foregroundCropGameObject;
    private SpriteRenderer backgroundSprite;
    private SpriteRenderer foregroundSprite;

    [Header("State")]
    public CropData assignedCrop;
    public bool cropInserted;
    public int keyIndex;

    [Header("UI")]
    public GameObject highlight;
    public GameObject canvas;

    private AudioSource audioSource;

    public UnityAction<MudRoomCropKey> OnCropInserted;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    void Awake()
    {
        backgroundSprite = backgroundCropGameObject.GetComponent<SpriteRenderer>();
        foregroundSprite = foregroundCropGameObject.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        canvas.SetActive(false);
    }

    public void AssignCrop(CropData crop)
    {
        assignedCrop = crop;
        backgroundSprite.sprite = crop.cropYield.icon;
        foregroundSprite.sprite = crop.cropYield.icon;
        foregroundSprite.enabled = cropInserted;
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if (cropInserted || item != assignedCrop.cropYield)
        {
            interactSuccessful = false;
            return;
        }

        HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
        interactor.playerInventoryHolder.UpdateInventory();

        cropInserted = true;
        foregroundSprite.enabled = true;
        highlight.SetActive(false);
        canvas.SetActive(false);

        audioSource?.Play();
        OnCropInserted?.Invoke(this);

        interactSuccessful = true;
    }

    public void ToggleHighlight(bool enable)
    {
        if (cropInserted) return;
        highlight.SetActive(enable);
        canvas.SetActive(enable);
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
    }

    public MudRoomCropKeySaveData ExportSaveData()
    {
        string cropName = "";
        if(assignedCrop) cropName = assignedCrop.name;
        return new MudRoomCropKeySaveData
        {
            _keyIndex = keyIndex,
            _cropInserted = cropInserted,
            cropName = cropName
        };
    }

    public void ImportSaveData(MudRoomCropKeySaveData data)
    {
        cropInserted = data._cropInserted;
        assignedCrop = CropDatabase.Instance.GetCropByName(data.cropName);

        backgroundSprite.sprite = assignedCrop.cropYield.icon;
        foregroundSprite.sprite = assignedCrop.cropYield.icon;
        foregroundSprite.enabled = cropInserted;
    }

    public void EndInteraction()
    {
        
    }

    internal void AutoComplete()
    {
        cropInserted = true;
        foregroundSprite.enabled = true;
    }
}
