using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RepairMinigame : MonoBehaviour
{
    [SerializeField] private bool debugMode = false;
    [Header("ORDER THESE BY PRIORITY TOP TO BOTTOM")]
    [SerializeField] private List<MinigameFunctionality> possibleSegments;
    [Header("IGNORE THIS FOR NOW")]
    [SerializeField] private MonoBehaviour settingsOverride;

    [Header("Default Minigame Settings")]
    [SerializeField] private float minigameSpeed;
    [SerializeField] private float slowMultiplier;
    [SerializeField] private int maxBounces;
    [SerializeField] private float nailSpeed = 1f;
    [SerializeField] private float hitAnimSpeed = 10f;
    //public int neededHits;
    //public int allowedMisses;
    //private int currentHits;
    //private int currentMisses;


    [Header("References")]
    [SerializeField] private GameObject minigameUI;
    [SerializeField] private Slider minigameSlider, progressSlider;
    [SerializeField] private TextMeshProUGUI missesAllowedText;
    [SerializeField] private TextMeshProUGUI hitsLeftText;
    [SerializeField] private Image handleImage;

    // private vars
    private bool minigameActive = false;
    private bool sliderCanMove = false;
    private bool canHit = false;
    private float sliderDirection;
    public int currentBounces = 0;
    private ControlManager controlManager;
    private MinigameFunctionality hitSegment;
    private DebrisPile debrisPile;
    private Vector3 originalPos;
    private Coroutine hitCoroutine, nailFlashCoroutine;
    private UISpriteAnim nailHitAnim;
    

    /*
        Notes: 
        0.5 is the middle of the slider, 0 is the left, 1 is the right
        The minigame slider will move back and forth between 0 and 1 and that is so flipping awesome dude
        This is the only slider that is easy to deal with, the rest are just a pain in the ass

        I let github copilot autocomplete that last comment btw it understands me
    */
    void Awake()
    {
        nailHitAnim = GetComponent<UISpriteAnim>();
        minigameSlider.value = 0f;
        controlManager = FindObjectOfType<ControlManager>();
        originalPos = minigameUI.transform.position;
        minigameUI.SetActive(false);
    }

    private void OnEnable()
    {
        controlManager.minigamePress.action.performed += MinigamePress;
        controlManager.minigameExit.action.performed += MinigameExit;
    }
    private void OnDisable()
    {
        controlManager.minigamePress.action.performed -= MinigamePress;
        controlManager.minigameExit.action.performed -= MinigameExit;
    }

    // Update is called once per frame
    void Update()
    {
        /*if (debugMode)
        {
            if (Keyboard.current.zKey.wasPressedThisFrame)
            {
                Debug.Log("Forced: Starting Minigame");
                //StartMinigame();
            }
            if (Keyboard.current.xKey.wasPressedThisFrame)
            {
                Debug.Log("Forced: Ending Minigame");
                //EndMinigame();
            }
        }*/

        if (!minigameActive) return;

        if (sliderCanMove)
        {

            if (minigameSlider.value == 0f)
            {
                sliderDirection = 1;
                currentBounces++;
            }
            else if (minigameSlider.value == 1f)
            {
                sliderDirection = -1;
                currentBounces++;
            }
            currentBounces = Mathf.Clamp(currentBounces, 0, maxBounces);

            minigameSlider.value += Time.deltaTime * (minigameSpeed - (slowMultiplier * currentBounces)) * sliderDirection;
        }

        progressSlider.value = Mathf.Lerp(progressSlider.value, debrisPile.initialRepairsNeeded - debrisPile.repairsLeft, Time.deltaTime * nailSpeed);
        Debug.Log("Progress Slider Value: " + progressSlider.value);

        // Rotate the handle 45 degrees based on misses left
        float missRatio = (float)debrisPile.missesLeft / (float)debrisPile.initialMissesAllowed;
        float handleRotation = Mathf.Lerp(0f, 45f, 1f - missRatio);
        progressSlider.handleRect.rotation = Quaternion.Euler(0f, 0f, handleRotation);
    }

    private void MinigamePress(InputAction.CallbackContext context)
    {
        if (!canHit) return;
        if (!minigameActive) return;
        if (context.canceled) return;
        if (hitCoroutine != null) return;
        hitCoroutine = StartCoroutine(AttemptHit());
    }

    private void MinigameExit(InputAction.CallbackContext context)
    {
        if (!minigameActive) return;
        if (context.canceled) return;
        if (hitCoroutine != null) StopCoroutine(hitCoroutine);
        EndMinigame();
    }

    private IEnumerator AttemptHit()
    {
        //Play Anim
        //Play Sound
        sliderCanMove = false;
        canHit = false;
        HitLoop();

        if (hitSegment != null)
        {
            debrisPile.repairsLeft -= hitSegment.hitCount;
            debrisPile.missesLeft -= hitSegment.missCount; // In case a hit segment also adds misses
            if (debrisPile.repairsLeft < 0)
            {
                debrisPile.repairsLeft = 0;
            }
            hitsLeftText.text = debrisPile.repairsLeft.ToString();
            missesAllowedText.text = debrisPile.missesLeft.ToString();

            if(hitSegment.hitCount > 0)
            {
                StartCoroutine(HitEffect());
            }
            else if(hitSegment.missCount > 0)
            {
                StartCoroutine(MissEffect());
            }

        }
        else
        {
            debrisPile.missesLeft--;
            StartCoroutine(MissEffect());
            if (debrisPile.missesLeft < 0)
            {
                debrisPile.missesLeft = 0;
            }
            hitsLeftText.text = debrisPile.repairsLeft.ToString();
            missesAllowedText.text = debrisPile.missesLeft.ToString();
        }

        Debug.Log("Minigame Value: " + minigameSlider.value);
        if(debrisPile.missesLeft <= 1 && nailFlashCoroutine == null)
        {
            nailFlashCoroutine = StartCoroutine(NailFlashing());
        }

        yield return new WaitForSeconds(0.5f);

        if (debrisPile.repairsLeft <= 0)
        {
            // End the minigame
            MinigameSuccess();
            yield break;
        }
        else if (debrisPile.missesLeft <= 0)
        {
            // End the minigame
            MinigameFail();
            yield break;
        }

        currentBounces = 1;
        canHit = true;
        sliderCanMove = true;

        hitCoroutine = null;
    }

    private bool HitLoop()
    {
        for (int i = 0; i < possibleSegments.Count; i++)
        {
            var trueSize = possibleSegments[i].size / 2;
            if (minigameSlider.value >= 0.5f - trueSize && minigameSlider.value <= 0.5f + trueSize)
            {
                print("Minimum Value: " + (0.5f - trueSize) + " Maximum Value: " + (0.5f + trueSize));
                // Call the minigame function
                possibleSegments[i].Invoke("MinigameFunction", 0f);
                hitSegment = possibleSegments[i];

                if (possibleSegments[i].hitCount > 0) // Checks if the segment counts as a hit or a miss
                {
                    return true;
                }
                else return false;
            }
        }
        hitSegment = null;
        return false;
    }

    public void StartMinigame(DebrisPile pile)
    {
        // Start the minigame
        currentBounces = 1;
        sliderDirection = 1;
        minigameSlider.value = 0.001f;
        progressSlider.maxValue = pile.initialRepairsNeeded;
        progressSlider.value = pile.repairsLeft;
        progressSlider.handleRect.rotation = Quaternion.Euler(0f, 0f, 0f);
        handleImage.color = Color.white;
        debrisPile = pile;
        hitsLeftText.text = debrisPile.repairsLeft.ToString();
        missesAllowedText.text = debrisPile.missesLeft.ToString();
        PlayerMovement.restrictMovementTokens++;
        minigameActive = true;
        sliderCanMove = true;
        StartCoroutine(CanHitDelay());
        if(debrisPile.missesLeft <= 1 && nailFlashCoroutine == null)
        {
            nailFlashCoroutine = StartCoroutine(NailFlashing());
        }
        
        minigameUI.SetActive(true);
        Debug.Log("Repairs Needed: " + debrisPile.repairsLeft);
        Debug.Log("Misses Allowed: " + debrisPile.missesLeft);
    }

    private IEnumerator CanHitDelay()
    {
        yield return new WaitForSeconds(0.2f);
        canHit = true;
        StopCoroutine(CanHitDelay());
    }


    private void MinigameSuccess()
    {
        // Do the good thing
        EndMinigame();
        Debug.Log("Minigame: Success!");
        debrisPile.RepairStructure();
    }

    private void MinigameFail()
    {
        // Do the bad thing
        EndMinigame();
        Debug.Log("Minigame: Fail!");
        debrisPile.DestroyStructure();
    }
    public void EndMinigame()
    {
        // End the minigame
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
            hitCoroutine = null;
        } 
        if(nailFlashCoroutine != null)
        {
            StopCoroutine(nailFlashCoroutine);
            nailFlashCoroutine = null;
        }
        minigameUI.SetActive(false);
        handleImage.color = Color.white;
        sliderCanMove = false;
        canHit = false;
        minigameActive = false;
        PlayerMovement.restrictMovementTokens--;
        minigameSlider.value = 0f;
    }

    public void ForceEndMinigame()
    {
        if(debrisPile.missesLeft <= 0) MinigameFail();
        if(debrisPile.repairsLeft <= 0) MinigameSuccess();
    }
    
    public bool IsMinigameActive()
    {
        //print("Minigame Active: " + minigameActive);
        return minigameActive;
    }
    private IEnumerator HitEffect()
    {
        Vector3 newPosition = originalPos + new Vector3(0f, -10f, 0f);
        nailHitAnim.PlayOneShotUI();

        while (Vector3.Distance(minigameUI.transform.position, newPosition) > 0.01f)
        {
            minigameUI.transform.position = Vector3.MoveTowards(minigameUI.transform.position, newPosition, hitAnimSpeed * Time.deltaTime);
            yield return null;
        }

        while (Vector3.Distance(minigameUI.transform.position, originalPos) > 0.01f)
        {
            minigameUI.transform.position = Vector3.MoveTowards(minigameUI.transform.position, originalPos, hitAnimSpeed * Time.deltaTime);
            yield return null;
        }
    }
    private IEnumerator MissEffect()
    {
        float shakeDuration = 0.5f;
        float shakeStrength = 5f;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            Vector3 randomOffset = new Vector3(
                Random.Range(-shakeStrength, shakeStrength),
                Random.Range(-shakeStrength, shakeStrength),
                0f
            );

            minigameUI.transform.position = originalPos + randomOffset;
            yield return null;
        }

        // Smoothly return to original position
        while (Vector3.Distance(minigameUI.transform.position, originalPos) > 0.01f)
        {
            minigameUI.transform.position = Vector3.MoveTowards(
                minigameUI.transform.position,
                originalPos,
                500f * Time.deltaTime
            );
            yield return null;
        }

        minigameUI.transform.position = originalPos;
    }

    private IEnumerator NailFlashing()
    {
        while (true)
        {
            float flashDuration = 0.5f;
            float elapsedTime = 0f;
            Color originalColor = Color.white;
            Color flashColor = Color.red;

            while (elapsedTime < flashDuration)
            {
                handleImage.color = Color.Lerp(originalColor, flashColor, Mathf.PingPong(elapsedTime * 4f, 1f));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            handleImage.color = originalColor;
            yield return new WaitForSeconds(0.5f);
        }
    }
}
