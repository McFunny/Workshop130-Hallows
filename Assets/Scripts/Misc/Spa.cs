using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Spa : MonoBehaviour, IInteractable
{
    bool playerInSpa, activated;

    public InventoryItemData waterCan, waterGun;

    public InventoryItemData bathBomb;

    public ParticleSystem splashVFX;

    public Transform focalPoint;

    public GameObject activatedEffects;

    public AudioSource source;

    public SpriteRenderer renderer;
    public Sprite[] waterSprites;

    void Start()
    {
        StartCoroutine(Heal());
        StartCoroutine(AnimateWater());
    }

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if((item == waterCan || item == waterGun) && PlayerInteraction.Instance.waterHeld < PlayerInteraction.Instance.maxWaterHeld)
        {
            interactSuccessful = true;
            PlayerInteraction.Instance.waterHeld = PlayerInteraction.Instance.maxWaterHeld;
            splashVFX.Play();
            source.Play();
            return;
        }
        if(item == bathBomb && !activated)
        {
            interactSuccessful = true;
            activated = true;
            activatedEffects.SetActive(true);
            StartCoroutine(ActivationTimer());
            splashVFX.Play();
            source.Play();
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            return;
        }

        interactSuccessful = false;
        
    }
    
    public void EndInteraction()
    {
       
    }

    IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(60);
        activated = false;
        activatedEffects.SetActive(false);
    }

    public void ReturnFocalPoint(out Transform _focalPoint)
    {
        _focalPoint = focalPoint;
    }

    public void ToggleHighlight(bool enable)
    {
        //
    }

    IEnumerator HightlightFlash()
    {
        yield break;
    }

    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            playerInSpa = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 10)
        {
            playerInSpa = false;
        }
    }

    IEnumerator Heal()
    {
        do
        {
            yield return new WaitForSeconds(0.5f);
            if(playerInSpa && activated)
            {
                if(PlayerInteraction.Instance.stamina < PlayerInteraction.Instance.maxStamina) PlayerInteraction.Instance.stamina += 10;
                if(PlayerInteraction.Instance.fatigue > 0)  PlayerInteraction.Instance.fatigue -= 5;
            }
        }
        while(gameObject.activeSelf);
    }

    IEnumerator AnimateWater()
    {
        int currentSprite = 0;
        do
        {
            currentSprite++;
            if(currentSprite >= waterSprites.Length) currentSprite = 0;
            yield return new WaitForSeconds(0.5f);
            renderer.sprite = waterSprites[currentSprite];
        }
        while(gameObject.activeSelf);
    }
}
