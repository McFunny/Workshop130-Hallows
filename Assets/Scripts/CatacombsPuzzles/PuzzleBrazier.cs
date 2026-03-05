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
    [SerializeField] public Sprite fireSprite;
    [SerializeField] public SpriteRenderer spriteRenderer;
    [SerializeField] public SpriteRenderer fireSpriteRenderer;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;
    public GameObject canvas;
    public GameObject canvasHolder;
    public Color gray;
    public Color gold;

    public InventoryItemData mandrake;
    public CropData mandrakeCropData;



    public void Start()
    {
        spriteRenderer.sprite = nutrientSprites[correctFire - 1];
        fireSpriteRenderer.sprite = fireSprite;
        canvas.SetActive(false);
        //fire.DoTypeBasedOnNumber(currentFire);
    }

    public bool isLocked = false;

    public void EndInteraction()
    {

    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (currentFire == correctFire) { isLocked = true; return; }
        if (isLocked) return;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if (currentFire == correctFire) { return; }
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

    public void ToggleHighlight(bool enable)
    {
        if (isLocked)
        {
            foreach (GameObject thing in highlight) thing.SetActive(false);

        }
            if (highlight.Count == 0) return;

        if (highlightMaterial.Count == 0)
        {
            foreach (GameObject thing in highlight)
                highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if (enable && !highlightEnabled)
        {
            highlightEnabled = true;
            canvas.SetActive(true);
            foreach (GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if (!enable && highlightEnabled)
        {
            highlightEnabled = false;
            canvas.SetActive(false);
            foreach (GameObject thing in highlight) thing.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while (highlightEnabled)
        {
            do
            {
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);

            }
            while (power > 0.7f && highlightEnabled);
            do
            {
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while (power < 1.9f && highlightEnabled);
        }
    }

    private CropData FindCropByYield(InventoryItemData item)
    {

        if (item == mandrake)
        {
            return mandrakeCropData;
        }

        // Assuming you have a central list of all CropData objects
        List<CropItem> allCrops = _database.GetAllCrops();
        Debug.Log($"Count of allCrop is {allCrops.Count}");
        foreach (var crop in allCrops)
        {
            if(crop.cropData == null)
            {
                Debug.Log($"Crop {crop.name} has no CropData assigned.");
                continue;
            }
            else if (crop.cropData.cropYield == null)
            {
                Debug.Log($"Crop {crop.name} has no cropYield assigned.");
                continue;
            }
            else if (crop.cropData.cropYield == item)
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
                fireSpriteRenderer.sprite = fireSprite;
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
        if (currentFire == correctFire)
        {
            Debug.Log("Color");
            fireSpriteRenderer.color = gold;
            canvasHolder.SetActive(false);
        }
        else if (currentFire != correctFire)
        {
            fireSpriteRenderer.color = gray;
        }



        }

        public BrazierSaveData ExportSaveData()
    {
        return new BrazierSaveData
        {
            correctFireSave = correctFire,
            currentFireSave = currentFire,
            isLockedSave = isLocked
        };
    }

    public void ImportSaveData(BrazierSaveData data)
    {
        correctFire = data.correctFireSave;
        currentFire = data.currentFireSave;
        isLocked = data.isLockedSave;

        if (currentFire == correctFire)
        {
            fireSpriteRenderer.color = gold;
            canvasHolder.SetActive(false);
        }
        else if (currentFire != correctFire)
        {
            fireSpriteRenderer.color = gray;
            canvasHolder.SetActive(true);
        }

        fire.DoTypeBasedOnNumber(currentFire);
    }
}

[System.Serializable]
public struct BrazierSaveData
{
    public int correctFireSave;
    public int currentFireSave;
    public bool isLockedSave;
}
