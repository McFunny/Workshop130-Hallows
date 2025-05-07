using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GrindStone : MonoBehaviour, IInteractable
{
   
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private Animator animator;
    private bool isCoroutineRunning;
    public void EndInteraction()
    {
       
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
        if (!isCoroutineRunning) { StartCoroutine(SpinGrindstone()); }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
    }

    public void ToggleHighlight(bool enabled)
    {
       
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    IEnumerator SpinGrindstone()
    {
        isCoroutineRunning = true;
        animator.SetTrigger("Spin");
        yield return new WaitForSeconds(1);
        isCoroutineRunning = false;

    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
    }
}
