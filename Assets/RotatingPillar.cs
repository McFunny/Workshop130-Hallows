using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RotatingPillar : MonoBehaviour, IInteractable
{

    private bool coroutineRunning = false;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

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
