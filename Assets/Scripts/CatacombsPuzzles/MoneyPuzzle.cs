using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MoneyPuzzle : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public bool donationComplete = false;
    private AudioSource audioSource;
    private Animator animator;

    public void EndInteraction()
    {
      
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (!donationComplete)
        {
            if (PlayerInteraction.Instance.currentMoney >= 50)
            {
                PlayerInteraction.Instance.currentMoney -= 50;
                donationComplete = true;
                audioSource.Play();
                animator.SetTrigger("OnInsert");
            }
            else interactSuccessful = false;
        }
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
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
