using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
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
    //public int neededHits;
    //public int allowedMisses;
    //private int currentHits;
    //private int currentMisses;


    [Header("References")]
    [SerializeField] private GameObject minigameUI;
    [SerializeField] private Slider minigameSlider;
    [SerializeField] private TextMeshProUGUI missesAllowedText;
    [SerializeField] private TextMeshProUGUI hitsLeftText;

    // private vars
    private bool minigameActive = false;
    private bool sliderCanMove = false;
    private bool canHit = false;
    private float sliderDirection;
    public int currentBounces = 0;
    private ControlManager controlManager;
    private MinigameFunctionality hitSegment;
    private DebrisPile debrisPile;

    /*
        Notes: 
        0.5 is the middle of the slider, 0 is the left, 1 is the right
        The minigame slider will move back and forth between 0 and 1 and that is so flipping awesome dude
        This is the only slider that is easy to deal with, the rest are just a pain in the ass

        I let github copilot autocomplete that last comment btw it understands me
    */
    void Awake()
    {
        minigameSlider.value = 0f;
        controlManager = FindObjectOfType<ControlManager>();
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
    }

    private void MinigamePress(InputAction.CallbackContext context)
    {
        if (!canHit) return;
        if (!minigameActive) return;
        if (context.canceled) return;
        StartCoroutine(AttemptHit());
    }

    private void MinigameExit(InputAction.CallbackContext context)
    {
        if (!minigameActive) return;
        if (context.canceled) return;
        EndMinigame();
    }

    private IEnumerator AttemptHit()
    {
        //Play Anim
        //Play Sound
        sliderCanMove = false;
        canHit = false;

        if (HitLoop() == true && hitSegment != null)
        {
            debrisPile.repairsLeft = debrisPile.repairsLeft - hitSegment.hitCount;
            if (debrisPile.repairsLeft < 0)
            {
                debrisPile.repairsLeft = 0;
            }
            hitsLeftText.text = debrisPile.repairsLeft.ToString();
        }
        else
        {
            debrisPile.missesLeft--;
            if (debrisPile.missesLeft < 0)
            {
                debrisPile.missesLeft = 0;
            }
            missesAllowedText.text = debrisPile.missesLeft.ToString();
        }

        Debug.Log("Minigame Value: " + minigameSlider.value);

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

        StopCoroutine(AttemptHit());

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

                if (possibleSegments[i].isHit) // Checks if the segment counts as a hit or a miss
                {
                    return true;
                }
                else return false;
            }
        }
        return false;
    }

    public void StartMinigame(DebrisPile pile)
    {
        // Start the minigame
        currentBounces = 1;
        sliderDirection = 1;
        minigameSlider.value = 0.001f;
        debrisPile = pile;
        hitsLeftText.text = debrisPile.repairsLeft.ToString();
        missesAllowedText.text = debrisPile.missesLeft.ToString();
        PlayerMovement.restrictMovementTokens++;
        minigameActive = true;
        sliderCanMove = true;
        minigameUI.SetActive(true);
        StartCoroutine(CanHitDelay());

        Debug.Log("Repairs Needed: " + debrisPile.repairsLeft);
        Debug.Log("Misses Allowed: " + debrisPile.missesLeft);
    }

    private IEnumerator CanHitDelay()
    {
        yield return new WaitForSeconds(1f);
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
        StopCoroutine(AttemptHit());
        minigameUI.SetActive(false);
        sliderCanMove = false;
        canHit = false;
        minigameActive = false;
        PlayerMovement.restrictMovementTokens--;
        minigameSlider.value = 0f;
    }
    
    public bool IsMinigameActive()
    {
        //print("Minigame Active: " + minigameActive);
        return minigameActive;
    }
}
