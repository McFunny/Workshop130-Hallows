using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMeters : MonoBehaviour
{
    [SerializeField] private GameObject extraWaterObject, medLinesObject, fullLinesObject;
    public Slider waterBar, extraWaterBar, staminaBar, regenBar;
    public Image waterFill, extraWaterFill, staminaFill;
    public GameObject leftTextbox, rightTextbox;
    public Color c_stamina, c_water, c_damage;
    PlayerInteraction p;
    float currentStamina, currentWater;
    ControlManager controlManager;
    public TextMeshProUGUI leftText, rightText;
    private const float INITIALMAXWATER = 10f;
    private bool isDamagedCoroutineRunning = false;
    private bool isWaterLoweredCoroutineRunning = false;
    [SerializeField] UISpriteAnim waterAnimator, extraWaterAnimator, staminaAnimator;
    void Start()
    {
        p = PlayerInteraction.Instance;

        waterBar.minValue = 0;
        waterBar.maxValue = INITIALMAXWATER;

        currentStamina = p.stamina;
        currentWater = p.waterHeld;
        regenBar.maxValue = p.maxStamina;

        controlManager = FindFirstObjectByType<ControlManager>();

        rightTextbox.SetActive(false);
        leftTextbox.SetActive(false);
        UpdateMeters();
    }

    void Update()
    {
        
        if(controlManager.moreInfo.action.WasPressedThisFrame())
        {
            //animator.SetBool("isClockRaised", true);
            rightTextbox.SetActive(true);
            leftTextbox.SetActive(true);
        }
        if(controlManager.moreInfo.action.WasReleasedThisFrame())
        {
            //animator.SetBool("isClockRaised", false);
            rightTextbox.SetActive(false);
            leftTextbox.SetActive(false);
        }

        if(p.stamina != currentStamina || p.waterHeld != currentWater)
        {
            UpdateMeters();
        }
    }

    public void UpdateMeters()
    {
        waterBar.value = p.waterHeld;
        staminaBar.maxValue = p.maxStamina;
        regenBar.maxValue = p.maxStamina;

        staminaBar.value = p.stamina;
        regenBar.value = p.targetRegen;

        leftText.text = p.waterHeld + "/" + p.maxWaterHeld;
        rightText.text = p.stamina + "/" + p.maxStamina;

        if(p.stamina < currentStamina)
        {
            if(!isDamagedCoroutineRunning)
            {
                if(p.stamina < p.maxStamina) StartCoroutine(PlayerDamaged());
            }
            
        }

        if(p.waterHeld < currentWater)
        {
            if(!isWaterLoweredCoroutineRunning) StartCoroutine(WaterLowered());
        }

        currentStamina = p.stamina;
        currentWater = p.waterHeld;

        if(p.maxWaterHeld > INITIALMAXWATER)
        {
            extraWaterBar.maxValue = p.maxWaterHeld;
            extraWaterBar.minValue = INITIALMAXWATER;

            if(p.maxWaterHeld == 15)
            {
                medLinesObject.SetActive(true);
                fullLinesObject.SetActive(false);
            }
            else
            {
                medLinesObject.SetActive(false);
                fullLinesObject.SetActive(true);
            }
            
            extraWaterBar.value = p.waterHeld;
            extraWaterObject.SetActive(true);
        }
        else extraWaterObject.SetActive(false);
    }

    IEnumerator PlayerDamaged()
    {
        isDamagedCoroutineRunning = true;
        staminaAnimator.PlayOneShotUI();
        for(int i = 0; i < 4; i++)
        {
            staminaFill.color = c_damage;

            yield return new WaitForSeconds(.1f);
            staminaFill.color = c_stamina;
 
            yield return new WaitForSeconds(.1f);
            
        }
        isDamagedCoroutineRunning = false;
    }

    IEnumerator WaterLowered()
    {
        isWaterLoweredCoroutineRunning = true;

        if(currentWater > INITIALMAXWATER)
        {
           extraWaterAnimator.PlayOneShotUI();
        }
        else waterAnimator.PlayOneShotUI();

        for(int i = 0; i < 4; i++)
        {
            if(currentWater > INITIALMAXWATER) extraWaterFill.color = c_damage;
            else waterFill.color = c_damage;
            
            yield return new WaitForSeconds(.1f);

            if(currentWater > INITIALMAXWATER) extraWaterFill.color = c_water;
            else waterFill.color = c_water;
 
            yield return new WaitForSeconds(.1f);
            
        }

        extraWaterFill.color = c_water;
        waterFill.color = c_water;
        isWaterLoweredCoroutineRunning = false;
    }
    
}

