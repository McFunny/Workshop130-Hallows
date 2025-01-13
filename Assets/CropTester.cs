using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CropTester : MonoBehaviour, IInteractable
{
    public bool cropInserted;
    private CropData currentCrop; // Store the current crop
    public float actionDuration = 5f; // Duration of the "thing"
    private bool isProcessing; // Whether the action is in progress
    public GameObject spriteObject;
    private SpriteRenderer spriteRenderer;

    [SerializeField] private Database _database;

    private void Start()
    {
        spriteRenderer = spriteObject.GetComponent<SpriteRenderer>();
    }

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void EndInteraction()
    {
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
       interactSuccessful = false;

        if (isProcessing)
        {
            Debug.Log("Action in progress.");
            return;
        }

        if (cropInserted)
        {
            
            InventoryItemData cropYield = currentCrop.cropYield;

            if (cropYield != null)
            {
                interactor.playerInventoryHolder.AddToInventory(cropYield, 1);
                Debug.Log($"Returned {cropYield.displayName} to the player.");
            }
            else
            {
                
            }

            
            cropInserted = false;
            currentCrop = null;
            spriteRenderer.sprite = null;
            interactSuccessful = true;
        }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;

        if (isProcessing)
        {
            Debug.Log("Already processing or crop inserted. Cannot insert another crop.");
            interactSuccessful = false;
            return;
        }

        if (cropInserted)
        {

            InventoryItemData cropYield = currentCrop.cropYield;

            if (cropYield != null)
            {
                interactor.playerInventoryHolder.AddToInventory(cropYield, 1);
                Debug.Log($"Returned {cropYield.displayName} to the player.");
            }
            else
            {

            }


            cropInserted = false;
            currentCrop = null;
            spriteRenderer.sprite = null;
            interactSuccessful = true;
            return;
        }


        if (item != null)
        {
            CropData matchingCrop = FindCropByYield(item);
            if (matchingCrop != null)
            {
                currentCrop = matchingCrop;
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                interactor.playerInventoryHolder.UpdateInventory();
                spriteRenderer.sprite = item.icon;

                cropInserted = true;
                interactSuccessful = true;

                // Start the processing coroutine
                StartCoroutine(ProcessCrop());
            }
            else
            {
                Debug.Log($"The item {item.displayName} is not a valid crop.");
            }
        }
    }


        private IEnumerator ProcessCrop()
    {
        isProcessing = true;
        Debug.Log($"Processing crop {currentCrop.name}...");

       
        yield return new WaitForSeconds(actionDuration);

        Debug.Log($"Action completed for crop {currentCrop.name}.");
        isProcessing = false;
    }

    public void ToggleHighlight(bool enabled)
    {
      
    }

    private CropData FindCropByYield(InventoryItemData item)
    {
        // Assuming you have a central list of all CropData objects
        foreach (var crop in _database.GetAllCrops())
        {
            if (crop.cropData.cropYield == item)
            {
                return crop.cropData;
            }
        }
        return null; // No matching crop found
    }

}
