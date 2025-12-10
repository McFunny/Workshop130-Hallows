using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMeters : MonoBehaviour
{
    [SerializeField] private GameObject extraWaterObject, medLinesObject, fullLinesObject;
    public Slider waterBar, extraWaterBar, staminaBar, fatigueBar;
    public Image waterFill, extraWaterFill, staminaFill;
    public GameObject leftTextbox, rightTextbox;
    public Color c_stamina, c_water, c_damage;
    PlayerInteraction p;
    float currentStamina, currentWater;
    ControlManager controlManager;
    public TextMeshProUGUI leftText, rightText;
    private const float INITIALMAXWATER = 10f;
    void Start()
    {
        p = PlayerInteraction.Instance;

        waterBar.minValue = 0;
        waterBar.maxValue = INITIALMAXWATER;

        currentStamina = p.stamina;
        currentWater = p.waterHeld;
        fatigueBar.maxValue = p.maxStamina;

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

        if(p.stamina < currentStamina)
        {
            UpdateMeters();
            StartCoroutine(PlayerDamaged());
        }

        if(p.waterHeld < currentWater)
        {
            UpdateMeters();
            StartCoroutine(WaterLowered());
        }
    }

    public void UpdateMeters()
    {
        waterBar.value = p.waterHeld;

        staminaBar.value = p.stamina/p.maxStamina;
        fatigueBar.value = p.maxStamina + (p.fatigue - p.maxStamina); // math scares me

        leftText.text = p.waterHeld + "/" + p.maxWaterHeld;
        rightText.text = p.stamina + "/" + p.maxStamina;

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
        for(int i = 0; i < 4; i++)
        {
            staminaFill.color = c_damage;

            yield return new WaitForSeconds(.1f);
            staminaFill.color = c_stamina;
 
            yield return new WaitForSeconds(.1f);
            
        }
    }

    IEnumerator WaterLowered()
    {
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
    }
    
}

