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

    void Start()
    {
        backgroundSprite = backgroundCropGameObject.GetComponent<SpriteRenderer>();
        foregroundSprite = foregroundCropGameObject.GetComponent<SpriteRenderer>();

    }

    public void SetUpSprites()
    {
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
            CropYieldID = cropData?.cropYield?.ID ?? -1
        };
    }


    public void ImportSaveData(CropKeySaveData data, InventoryItemData cropYieldItem)
    {
        cropInserted = data.CropInserted;

        if (cropYieldItem != null)
        {
            foregroundSprite.sprite = cropYieldItem.icon;
            backgroundSprite.sprite = cropYieldItem.icon;
        }
        else
        {
            Debug.LogWarning($"CropYield with ID {data.CropYieldID} not found in the database.");
        }

        foregroundSprite.enabled = cropInserted;
    }



}

[System.Serializable]
public struct CropKeySaveData
{
    public bool CropInserted;
    public int CropYieldID; 
}




