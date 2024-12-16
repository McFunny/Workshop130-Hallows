using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RotatingPillar : MonoBehaviour, IInteractable
{

    private bool coroutineRunning = false;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public void Start()
    {

    }

    public void EndInteraction()
    {
      
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if (!coroutineRunning)
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
        Quaternion targetRotation = Quaternion.Euler(transform.eulerAngles + new Vector3(0,degrees,0));

        float elapsedTime = 0f;
        float rotationDuration = 2f;

        while (elapsedTime < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, elapsedTime/rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;

        coroutineRunning = false;

        OnInteractionComplete?.Invoke(this);
    }
    
}
