using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


public class InteractionButton : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public MonoBehaviour buttonControlledObj;
    public IButtonable buttonable;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;
    bool coroutineRunning = false;
    Animator animator;
    AudioSource audiosource;
    public AudioClip creakSound, clickSound;

    public void EndInteraction()
    {
       
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if (!coroutineRunning)
        {
            StartCoroutine(InteractCoroutine());
        }

        interactSuccessful = false;
    }

    IEnumerator InteractCoroutine()
    {
        if (animator != null)
        {
            coroutineRunning = true;
            animator.SetTrigger("PressButton");
            yield return null;
            float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;
            print(animator.GetCurrentAnimatorClipInfo(0).Length);
            if(audiosource) audiosource.PlayOneShot(creakSound);
            yield return new WaitForSeconds(animationLength / 2);
            if (audiosource) audiosource.PlayOneShot(creakSound);
            yield return new WaitForSeconds(animationLength / 2);
            coroutineRunning = false;
        }
        else
        {
            coroutineRunning = true;
            if (buttonable != null) buttonable.OnButtonPress();
            yield return new WaitForSeconds(0.5f);
            coroutineRunning = false;
        }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
    }
    public void ToggleHighlight(bool enabled)
    {
        if (highlight.Count == 0) return;
        if (highlightMaterial.Count == 0)
        {
            foreach (GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if (enabled && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach (GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if (!enabled && highlightEnabled)
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

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        audiosource = GetComponent<AudioSource>();
        buttonable = buttonControlledObj.GetComponent<IButtonable>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayAudioSound()
    {
        if(audiosource)
        {
            audiosource.PlayOneShot(clickSound);
        }
    }

    public void OnButtonAnimationEvent()
    {
        if (buttonable != null)
            buttonable.OnButtonPress();
    }

}

public interface IButtonable
{
    public abstract void OnButtonPress();
}
