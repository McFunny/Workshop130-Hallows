using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Well : MonoBehaviour, IInteractable
{
    public InventoryItemData waterCan, waterGun, waterCanUpgrade;
    public Transform bucket, bucketTop, ropePos, _focalPoint;
    public GameObject waterSprite;

    bool bucketFilled = false;
    public float altitude = 0; //0 meaning its at the top
    float currentRate = 0;
    float rateChange = 12;
    float riseRateMax = -3; //per second.
    float dropRateMax = 4.5f;
    float distance = 10; //progress until bucket fully risen or dropped
    bool interacting = false;
    bool autoCranking = false;
    public WellPhase phase;

    public AudioSource loopingSource;
    public AudioClip splashSound, bucketReturnedSound;
    public ParticleSystem splashParticle;
    public LineRenderer line;

    public GameObject structureUI;

    private Vector3 lineVertex1Start;
    private Vector3 lineVertex1End;
    private Vector3 bucketBottom;

    public Transform crankPivot;

    public enum WellPhase
    {
        BucketRisen,
        BucketInitialDrop,
        BucketAtBottom
    }

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    void Start()
    {
        waterSprite.SetActive(false);
        bucketFilled = false;
        altitude = 0;
        structureUI.SetActive(false);
        lineVertex1Start = line.GetPosition(1);
        lineVertex1End = new Vector3(lineVertex1Start.x, lineVertex1Start.y - (distance * 2), lineVertex1Start.z);
        bucketBottom = new Vector3(bucketTop.position.x, bucketTop.position.y - (distance * 2), bucketTop.position.z);

        StartCoroutine(CheckForAutocrank());
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;

        interacting = true;

        if(phase == WellPhase.BucketRisen)
        {
            if(bucketFilled) return;
            phase = WellPhase.BucketInitialDrop;
        }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(!bucketFilled || (item != waterCan && item != waterGun && item != waterCanUpgrade) || PlayerInteraction.Instance.waterHeld == PlayerInteraction.Instance.maxWaterHeld)
        {
            interactSuccessful = false;
            return;
        }
        interactSuccessful = true;
        PlayerInteraction.Instance.WaterChange(10);
        //PlayerInteraction.Instance.waterHeld += 10;
        //if(PlayerInteraction.Instance.maxWaterHeld < PlayerInteraction.Instance.waterHeld) PlayerInteraction.Instance.waterHeld = PlayerInteraction.Instance.maxWaterHeld;
        waterSprite.SetActive(false);
        bucketFilled = false;
        splashParticle.Play();
        AudioPoolManager.Instance.PlayClipAtPosition(splashSound, transform.position);
        
    }
    
    public void EndInteraction()
    {
       
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = _focalPoint;
    }

    void Update()
    {
        if(phase == WellPhase.BucketAtBottom && !structureUI.activeSelf && highlightEnabled) structureUI.SetActive(true);
        else if((phase != WellPhase.BucketAtBottom || !highlightEnabled) && structureUI.activeSelf) structureUI.SetActive(false);

        if(phase == WellPhase.BucketRisen) return;

        if(altitude < distance && ((!interacting || !InputManager.isHoldingInteract) || phase == WellPhase.BucketInitialDrop) && !autoCranking)
        {
            currentRate = currentRate + rateChange * Time.deltaTime;

            if(currentRate > dropRateMax) currentRate = dropRateMax;
        }
        else if((interacting && InputManager.isHoldingInteract) || autoCranking)
        {
            //altitude += riseRateMax * Time.deltaTime;
            currentRate = currentRate - rateChange * Time.deltaTime;

            float tempMaxRaiseRate = riseRateMax;
            if(autoCranking && !interacting) tempMaxRaiseRate *= 0.5f;
            if(currentRate < tempMaxRaiseRate) currentRate = tempMaxRaiseRate;
        }
        else currentRate = 0;

        altitude += currentRate * Time.deltaTime;
        
        if(altitude > distance)
        {
            if(phase == WellPhase.BucketInitialDrop)
            {
                phase = WellPhase.BucketAtBottom;
                waterSprite.SetActive(true);
            }
            AudioPoolManager.Instance.PlayClipAtPosition(splashSound, transform.position);
            altitude = distance;
            loopingSource.Stop();
            currentRate = 0;
        }

        bucket.position = Vector3.Lerp(bucketTop.position, bucketBottom, altitude/distance);
        ropePos.position = Vector3.Lerp(lineVertex1Start, lineVertex1End, altitude/distance);
        line.SetPosition(1, ropePos.position);

        if (altitude <= 0 && phase == WellPhase.BucketAtBottom)
        {
            phase = WellPhase.BucketRisen;
            bucketFilled = true;
            waterSprite.SetActive(true);
            loopingSource.Stop();
            AudioPoolManager.Instance.PlayClipAtPosition(bucketReturnedSound, transform.position);
            currentRate = 0;

            if(autoCranking)
            {
                TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.Autocrank);
                autoCranking = false;
            }
        }

        if(altitude > 0 && altitude < 10) 
        {
            if(!loopingSource.isPlaying) loopingSource.Play();
            crankPivot.Rotate(crankPivot.rotation.x + currentRate, crankPivot.rotation.y, crankPivot.rotation.z);
        }
    }

    IEnumerator CheckForAutocrank()
    {
        bool performAutoCrank = false;
        while(true)
        {
            if(performAutoCrank) yield return new WaitForSeconds(2);
            else yield return new WaitForSeconds(5);
            if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.Autocrank) && (phase == WellPhase.BucketAtBottom))
            {
                if(performAutoCrank) autoCranking = true;
                else performAutoCrank = true;
            }
            else 
            {
                autoCranking = false;
                performAutoCrank = false;
            }
        }
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
            if(InputManager.isHoldingInteract) interacting = true;
            foreach(GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if(!enable && highlightEnabled)
        {
            interacting = false;
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
}
