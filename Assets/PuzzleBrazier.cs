using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PuzzleBrazier : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [SerializeField] private Database _database;
    [SerializeField] private FireTypeController fire;

    [SerializeField] public int correctFire;
    [SerializeField] public int currentFire;

    [SerializeField] public List<Sprite> nutrientSprites = new List<Sprite>();
    [SerializeField] public SpriteRenderer spriteRenderer;

    public void Start()
    {
        spriteRenderer.sprite = nutrientSprites[correctFire - 1];
    }

    public bool isLocked = false;

    public void EndInteraction()
    {

    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (isLocked) return;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if (isLocked) return;
        if (item != null)
        {
            CropData matchingCrop = FindCropByYield(item);
            if (matchingCrop != null)
            {
                int number = CheckForCropStats(matchingCrop);
                DoCropSwitchCase(number);
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                interactor.playerInventoryHolder.UpdateInventory();

                OnInteractionComplete?.Invoke(this);
                interactSuccessful = true;

            }
            else
            {
                Debug.Log($"The item {item.displayName} is not a valid crop.");
            }
        }
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
                fire.DoFire();
                currentFire = 0;
                break;
            case 1:
                fire.DoGloam();
                currentFire = 1;
                break;
            case 2:
                fire.DoTerra();
                currentFire = 2;
                break;
            case 3:
                fire.DoIchor();
                currentFire = 3;
                break;
            case 4:
                if (correctFire == 1)
                {
                    fire.DoGloam();
                    currentFire = 1;
                }
                else
                {
                    fire.DoTerra();
                    currentFire = 2;
                }
                break;
            case 5:
                if (correctFire == 1)
                {
                    fire.DoGloam();
                    currentFire = 1;
                }
                else
                {
                    fire.DoIchor();
                    currentFire = 3;
                }
                break;
            case 6:
                if (correctFire == 2)
                {
                    fire.DoTerra();
                    currentFire = 2;
                }
                else
                {
                    fire.DoIchor();
                    currentFire = 3;
                }
                break;
            case 7:
                if (correctFire == 1)
                {
                    fire.DoGloam();
                    currentFire = 1;
                }
                else if (correctFire == 2)
                {
                    fire.DoTerra();
                    currentFire = 2;
                }
                else if (correctFire == 3)
                {
                    fire.DoIchor();
                    currentFire = 3;
                }
                break;

                //add in do specific effect for brazier

        }

    }
}