using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Gachapon : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;



    public bool currentlyOfferingPrize = false;
    public static Gachapon Instance;
    public SpriteRenderer ballSprite;
    private Animator animator;
    public List<ParticleSystem> steamParticles = new List<ParticleSystem>();

    public List<int> itemBacklog = new List<int>();
    public List<int> itemNumberBacklog = new List<int>();

    private bool coroutineRunning = false;

    public InventoryItemData siegePaper;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
 
    }

    private void Start()
    {
        animator = GetComponent<Animator>();

        if(!MainMenuScript.loadingData)
        {
            AddToBacklog(siegePaper, 1);
        }

        if(!currentlyOfferingPrize) ballSprite.enabled = false;
        if (itemBacklog.Count > 0) PlayParticles(true);
        else PlayParticles(false);

    }

    public void EndInteraction()
    {
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (itemBacklog.Count > 0 && currentlyOfferingPrize && !coroutineRunning)
        {
            InventoryItemData item = Database.Instance.GetItem(itemBacklog[0]);
            if (PlayerInventoryHolder.Instance.AddToInventory(item, itemNumberBacklog[0]))
            {
                StartCoroutine(CloseGachapon());
                interactSuccessful = true;
            }
        }
        else if (itemBacklog.Count > 0 && !currentlyOfferingPrize && !coroutineRunning)
        {
            StartCoroutine(OpenGachapon());
            interactSuccessful = true;
        }

    }


    IEnumerator OpenGachapon()
    {
        coroutineRunning = true;
        animator.SetTrigger("Open");
        ballSprite.enabled = true;
        currentlyOfferingPrize = true;
        yield return new WaitForSeconds(0.75f);
        coroutineRunning = false;
    }

    IEnumerator CloseGachapon()
    {
        coroutineRunning = true;
        animator.SetTrigger("Close");
        itemBacklog.RemoveAt(0);
        itemNumberBacklog.RemoveAt(0);
        ballSprite.enabled = false;
        currentlyOfferingPrize = false;
        if (itemBacklog.Count > 0) PlayParticles(true);
        else PlayParticles(false);
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

    public void AddToBacklog(InventoryItemData item, int numberOfItems)
    {
        itemBacklog.Add(item.ID);
        itemNumberBacklog.Add(numberOfItems);
        if (itemBacklog.Count > 0) PlayParticles(true);
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
       
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

    // Start is called before the first frame update

    public GachaponSaveData ExportSaveData()
    {
        return new GachaponSaveData
        {
            backlogData = itemBacklog,
            backlogNumberData = itemNumberBacklog,
            currentlyOfferingPrizeData = currentlyOfferingPrize
        };
    }

    public void ImportSaveData(GachaponSaveData data)
    {
        if(data.backlogData == null) return;
        itemBacklog = data.backlogData;
        itemNumberBacklog = data.backlogNumberData;
        currentlyOfferingPrize = data.currentlyOfferingPrizeData;

        if(currentlyOfferingPrize)
        {
            animator.SetTrigger("Open");
        }

        if (itemBacklog.Count > 0)
        {
           PlayParticles(true);
        }

    }

    }


    [System.Serializable]

public struct GachaponSaveData
{
    public List<int> backlogData;
    public List<int> backlogNumberData;
    public bool currentlyOfferingPrizeData;
}
