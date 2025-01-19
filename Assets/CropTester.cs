using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class CropTester : MonoBehaviour, IInteractable
{
    public bool cropInserted;
    private CropData currentCrop; // Store the current crop
    private bool isProcessing; // Whether the action is in progress
    public GameObject spriteObject;
    public GameObject dome;
    private SpriteRenderer spriteRenderer;
    public FireTypeController testerBrazier;

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
        Vector3 savedPosition = dome.transform.position;
        Vector3 offset = new Vector3 ( 0, -0.75f, 0 );
        Vector3 targetPosition = dome.transform.position + offset;
        Vector3 currentPosition = dome.transform.position;

        float elapsedTime = 0;
        float waitTime = 1f;

        while (elapsedTime < waitTime)
        {
            dome.transform.position = Vector3.Lerp(currentPosition, targetPosition, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        dome.transform.position = targetPosition;

        elapsedTime = 0;
        int number = CheckForCropStats(currentCrop);
        Debug.Log(number);
        DoCropSwitchCase(number);
        yield return new WaitForSeconds(3);
        testerBrazier.DoFire();
        currentPosition = dome.transform.position;
        while (elapsedTime < waitTime)
        {
            dome.transform.position = Vector3.Lerp(currentPosition, savedPosition, (elapsedTime / waitTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        dome.transform.position = savedPosition;

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

    private int CheckForCropStats(CropData crop)
    {
        List<float> cropStats = new List<float>();
        cropStats.Add(crop.gloamIntake);
        cropStats.Add(crop.terraIntake);
        cropStats.Add(crop.ichorIntake);
        float max = cropStats.Max();
        int maxes = 0;

        if (max == crop.gloamIntake) maxes++;
        if (max == crop.terraIntake) maxes++;
        if (max == crop.ichorIntake) maxes++;

        if (maxes == 1)
        {
            if (max == crop.gloamIntake) return 1;
            if (max == crop.terraIntake) return 2;
            if (max == crop.ichorIntake) return 3;
            else return 0;
        }

        else
        {
            if (max == crop.gloamIntake && max == crop.terraIntake && max == crop.ichorIntake) return 7;
            if (max == crop.gloamIntake && max == crop.terraIntake) return 4;
            if (max == crop.gloamIntake && max == crop.ichorIntake) return 5;
            if (max == crop.terraIntake && max == crop.ichorIntake) return 6;
            else return 0;
        }
    }

    private void DoCropSwitchCase(int number)
    {
        switch (number)
        {
            case 0:
                testerBrazier.DoFire();
                break;
            case 1:
                testerBrazier.DoGloam();
                break;
            case 2:
                testerBrazier.DoTerra();
                break;
            case 3:
                testerBrazier.DoIchor();
                break;
            default:
                StartCoroutine(RareFireEffects(number));
                break;
        }

    }

    IEnumerator RareFireEffects(int number)
    {
        switch (number)
        {
            case 4:
                testerBrazier.DoGloam();
                yield return new WaitForSeconds(1.5f);
                testerBrazier.DoTerra();
                break;
            case 5:
                testerBrazier.DoGloam();
                yield return new WaitForSeconds(1.5f);
                testerBrazier.DoIchor();
                break;
            case 6:
                testerBrazier.DoTerra();
                yield return new WaitForSeconds(1.5f);
                testerBrazier.DoIchor();
                break;
            case 7:
                testerBrazier.DoGloam();
                yield return new WaitForSeconds(1f);
                testerBrazier.DoTerra();
                yield return new WaitForSeconds(1f);
                testerBrazier.DoIchor();
                break;
        }

    }
}
