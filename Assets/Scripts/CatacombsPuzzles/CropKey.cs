using System.Collections;
using System.Collections.Generic;
//using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Events;

public class CropKey : MonoBehaviour, IInteractable
{

    public GameObject backgroundCropGameObject;
    public GameObject foregroundCropGameObject;
    private SpriteRenderer backgroundSprite;
    private SpriteRenderer foregroundSprite;
    public CropData cropData;
    public bool cropInserted;
    private AudioSource audioSource;

    void Start()
    {
        backgroundSprite = backgroundCropGameObject.GetComponent<SpriteRenderer>();
        foregroundSprite = foregroundCropGameObject.GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

    }

    public void SetUpSprites()
    {
        backgroundSprite = backgroundCropGameObject.GetComponent<SpriteRenderer>();
        foregroundSprite = foregroundCropGameObject.GetComponent<SpriteRenderer>();
        backgroundSprite.sprite = cropData.cropYield.icon;
        foregroundSprite.sprite = cropData.cropYield.icon;
        foregroundSprite.enabled = false;
    }

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void EndInteraction()
    {
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
    }

    public event UnityAction<CropKey> OnCropInserted;

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if (item == cropData.cropYield && !cropInserted)
        {

            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            interactor.playerInventoryHolder.UpdateInventory();

            cropInserted = true;
            foregroundSprite.enabled = true;

            interactSuccessful = true;

            audioSource.Play();

            OnCropInserted?.Invoke(this);
        }
        else
        {
            interactSuccessful = false;
        }
    }



    public void ToggleHighlight(bool enabled)
    {
    }

    
    void Update()
    {
        
    }

    public CropKeySaveData ExportSaveData()
    {
        return new CropKeySaveData
        {
            CropInserted = cropInserted,
            CropYieldID = cropData.cropYield.ID,
            CropdataName = cropData.name
        };
    }

    public void ImportSaveData(CropKeySaveData data, InventoryItemData cropYieldItem)
    {
        cropInserted = data.CropInserted;
        cropData = CropDatabase.Instance.GetCropByName(data.CropdataName);

        if (cropYieldItem != null)
        {
            foregroundSprite.sprite = cropYieldItem.icon;
            backgroundSprite.sprite = cropYieldItem.icon;
        }
        else
        {
            Debug.LogWarning($"CropYield with ID {data.CropYieldID} not found in the database.");
        }
        Debug.Log("Crop:" + cropYieldItem);
        Debug.Log("Crop Inserted: " + cropInserted);
        foregroundSprite.enabled = cropInserted;
        Debug.Log("Foreground Inserted: " + foregroundSprite.enabled);
        if (cropInserted)
        {
            OnCropInserted?.Invoke(this);
        }
    }
}

[System.Serializable]
public struct CropKeySaveData
{
    public bool CropInserted;
    public int CropYieldID;
    public string CropdataName;
}




