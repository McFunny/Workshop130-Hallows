using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RecipeMachine : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public bool currentlyOfferingPrize = false;
    public SpriteRenderer ballSprite;
    private Animator animator;
    public List<ParticleSystem> steamParticles = new List<ParticleSystem>();


    private bool coroutineRunning = false;

    public InventoryItemData ticket;

    public PopupScript recipeUnlockedP;

    public AudioClip grabSFX;

    public AudioClip gachaponOpenSound;
    //public AudioClip gachaponGearsTurning;
    public AudioClip gachaponCloseSound;

    public AudioSource source;
    

    private void Start()
    {
        animator = GetComponent<Animator>();

        if(!currentlyOfferingPrize) ballSprite.enabled = false;
        PlayParticles(true);

    }

    public void EndInteraction()
    {
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;

        if (currentlyOfferingPrize && !coroutineRunning)
        {
            StartCoroutine(CloseGachapon());
            interactSuccessful = true;
        }

    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;

        if(item != ticket) return;

        if (!currentlyOfferingPrize && !coroutineRunning)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();

            GameSaveData.Instance.tTicketsHeld--;

            StartCoroutine(OpenGachapon());
            interactSuccessful = true;
        }
       
    }

    void GiveRecipe()
    {
        CraftingDatabase.Instance.UnlockRandomLockedRecipeInTier();
        PopupHandler.Instance.AddToQueue(recipeUnlockedP);
        AudioPoolManager.Instance.PlayClip(grabSFX, 0.3f);
    }


    IEnumerator OpenGachapon()
    {
        coroutineRunning = true;
        animator.SetTrigger("Open");
        ballSprite.enabled = true;
        currentlyOfferingPrize = true;

        source.Stop();
        source.PlayOneShot(gachaponOpenSound);
        yield return new WaitForSeconds(0.75f);
        coroutineRunning = false;
    }

    IEnumerator CloseGachapon()
    {
        coroutineRunning = true;
        animator.SetTrigger("Close");
        ParticlePoolManager.Instance.GrabSparkParticle().transform.position = ballSprite.transform.position;
        ballSprite.enabled = false;
        currentlyOfferingPrize = false;
        GiveRecipe();

        source.Stop();
        source.PlayOneShot(gachaponCloseSound);
        yield return new WaitForSeconds(0.75f);
        coroutineRunning = false;
    }

    private void PlayParticles(bool enable)
    {
        foreach (ParticleSystem p in steamParticles)
        {
            var emission = p.emission;
            emission.enabled = enable;
            AudioSource pAudio = p.gameObject.GetComponent<AudioSource>();
            pAudio.enabled = enable;
        }
    }

   

    public void ReturnFocalPoint(out Transform point)
    {
        throw new System.NotImplementedException();
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
}
