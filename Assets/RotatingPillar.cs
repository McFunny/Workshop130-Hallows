using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RotatingPillar : MonoBehaviour, IInteractable
{
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    //
    //              WHEN CURRENTPILLARROTATION = 0
    //              Puzzle is solved
    //
    //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    private bool coroutineRunning = false;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;
    private Sprite[] growthSprites;

    public List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    public CropData specifiedCrop;
    [SerializeField] private int pillarNumber;
    public float currentPillarRotation;
    public bool correctlyOrientated;
    public int[] spriteSaveData = new int[4];
    private bool isLocked = false;
    private AudioSource audioSource;


    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void SetUpSprites(CropData crop)
    {
        specifiedCrop = crop;
        growthSprites = crop.cropSprites;

        if (growthSprites == null || growthSprites.Length == 0)
        {
            Debug.LogError("GrowthSprites is null or empty!");
            return;
        }

       
        List<int> validIndices = new List<int>();
        for (int j = 0; j < growthSprites.Length; j++)
        {
            validIndices.Add(j);
        }

        
        spriteRenderers[0].sprite = growthSprites[pillarNumber];
        spriteSaveData[0] = pillarNumber;
        validIndices.Remove(pillarNumber); 

       
        for (int i = 1; i < spriteRenderers.Count; i++)
        {
            if (validIndices.Count == 0)
            {
                Debug.LogWarning("Not enough sprites to assign!");
                return;
            }

           
            int randomIndex = Random.Range(0, validIndices.Count);
            int selectedSpriteIndex = validIndices[randomIndex];

            spriteRenderers[i].sprite = growthSprites[selectedSpriteIndex];
            spriteSaveData[i] = selectedSpriteIndex;

           
            validIndices.RemoveAt(randomIndex);
        }

       
        int rA = Random.Range(0, 4);
        float rotationAmount = rA * 90f;
        SetRotation(rotationAmount);
    }


    public void EndInteraction()
    {
      
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if (!coroutineRunning && !isLocked)
        {
            StartCoroutine(RotatePillar(90f));
            interactSuccessful = true;
        }
        else
        {
            interactSuccessful = false;
        }
        
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
    }

    public void ToggleHighlight(bool enable)
    {
        if(highlight.Count == 0) return;
        if(highlightMaterial.Count == 0)
        {
            foreach(GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if(enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach(GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if(!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach(GameObject thing in highlight) thing.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while(highlightEnabled)
        {
            do
            {
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power > 0.7f && highlightEnabled);
            do
            {
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power < 1.9f && highlightEnabled);
        }
    }

    IEnumerator RotatePillar(float degrees)
    {
        coroutineRunning = true;
        Quaternion currentRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, degrees, 0));

        float elapsedTime = 0f;
        float rotationDuration = 2f;

        audioSource.Play();

        currentPillarRotation += 90f;

        if (currentPillarRotation > 270) currentPillarRotation = 0f;

        correctlyOrientated = currentPillarRotation == 0f;

        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

       

        coroutineRunning = false;

        OnInteractionComplete?.Invoke(this);
    }


    private void SetRotation(float rotationAmount)
    {
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0, rotationAmount, 0));
        transform.rotation = targetRotation;
        currentPillarRotation = rotationAmount;

        if (currentPillarRotation == 0f) correctlyOrientated = true;
        else correctlyOrientated = false;

    }

    public void LockPuzzle()
    {
        isLocked = true;
    }

    public RotatingPillarSaveData ExportSaveData()
    {
        return new RotatingPillarSaveData
        {
            CurrentRotation = currentPillarRotation,
            IsCorrectlyOriented = correctlyOrientated,
            IsLocked = isLocked,
            SpriteIndices = spriteSaveData
        };
    }

    public void ImportSaveData(RotatingPillarSaveData data)
    {
        
        currentPillarRotation = data.CurrentRotation;
        correctlyOrientated = data.IsCorrectlyOriented;
        isLocked = data.IsLocked;
        spriteSaveData = data.SpriteIndices;

       
        SetRotation(currentPillarRotation);


        for (int i = 0; i < spriteRenderers.Count; i++)
        {
            if (spriteSaveData.Length > i && specifiedCrop != null)
            {
                spriteRenderers[i].sprite = specifiedCrop.cropSprites[spriteSaveData[i]];
            }
        }
    }

}


[System.Serializable]
public struct RotatingPillarSaveData
{
    public float CurrentRotation;
    public bool IsCorrectlyOriented;
    public bool IsLocked;
    public int[] SpriteIndices;
}