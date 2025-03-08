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
    public SpriteRenderer gloamSprite;
    public SpriteRenderer terraSprite;
    public SpriteRenderer ichorSprite;

    public List<Sprite> stoneNutrientSprites = new List<Sprite>();
    public List<Sprite> regularNutrientSprites = new List<Sprite>();

    [SerializeField] private Database _database;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

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
        Vector3 offset = new Vector3(0, -0.75f, 0);
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
        gloamSprite.sprite = stoneNutrientSprites[0];
        terraSprite.sprite = stoneNutrientSprites[1];
        ichorSprite.sprite = stoneNutrientSprites[2];
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

    public void ToggleHighlight(bool enable)
    {
        if (highlight.Count == 0) return;
        if (highlightMaterial.Count == 0)
        {
            foreach (GameObject thing in highlight)
                highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if (enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach (GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if (!enable && highlightEnabled)
        {
            highlightEnabled = false;
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
                gloamSprite.sprite = regularNutrientSprites[0];
                break;
            case 2:
                testerBrazier.DoTerra();
                terraSprite.sprite = regularNutrientSprites[1];
                break;
            case 3:
                testerBrazier.DoIchor();
                ichorSprite.sprite = regularNutrientSprites[2];
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
                gloamSprite.sprite = regularNutrientSprites[0];
                yield return new WaitForSeconds(1.5f);
                testerBrazier.DoTerra();
                terraSprite.sprite = regularNutrientSprites[1];
                break;
            case 5:
                testerBrazier.DoGloam();
                gloamSprite.sprite = regularNutrientSprites[0];
                yield return new WaitForSeconds(1.5f);
                testerBrazier.DoIchor();
                ichorSprite.sprite = regularNutrientSprites[2];
                break;
            case 6:
                testerBrazier.DoTerra();
                terraSprite.sprite = regularNutrientSprites[1];
                yield return new WaitForSeconds(1.5f);
                testerBrazier.DoIchor();
                ichorSprite.sprite = regularNutrientSprites[2];
                break;
            case 7:
                testerBrazier.DoGloam();
                gloamSprite.sprite = regularNutrientSprites[0];
                yield return new WaitForSeconds(1f);
                testerBrazier.DoTerra();
                terraSprite.sprite = regularNutrientSprites[1];
                yield return new WaitForSeconds(1f);
                testerBrazier.DoIchor();
                ichorSprite.sprite = regularNutrientSprites[2];
                break;
        }

    }
}