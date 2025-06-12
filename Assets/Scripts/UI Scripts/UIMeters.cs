using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMeters : MonoBehaviour
{
    public Slider waterBar, staminaBar, fatigueBar;
    public Image waterFill, staminaFill;
    public GameObject leftTextbox, rightTextbox;
    public Color c_stamina, c_water, c_damage;
    PlayerInteraction p;
    float currentStamina, currentWater;
    ControlManager controlManager;
    public TextMeshProUGUI leftText, rightText;
    void Start()
    {
        p = PlayerInteraction.Instance;
        currentStamina = p.stamina;
        currentWater = p.waterHeld;
        fatigueBar.maxValue = p.maxStamina;
        controlManager = FindFirstObjectByType<ControlManager>();

        rightTextbox.SetActive(false);
        leftTextbox.SetActive(false);
    }

    void Update()
    {
        UpdateMeters();
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
    }

    public void UpdateMeters()
    {
        waterBar.value = p.waterHeld/p.maxWaterHeld;
        staminaBar.value = p.stamina/p.maxStamina;
        fatigueBar.value = p.maxStamina + (p.fatigue - p.maxStamina); // math scares me

        leftText.text = p.waterHeld + "/" + p.maxWaterHeld;
        rightText.text = p.stamina + "/" + p.maxStamina;

        if(p.stamina < currentStamina)
        {
            StartCoroutine(PlayerDamaged());
            currentStamina = p.stamina;
        }

        if(p.waterHeld < currentWater)
        {
            StartCoroutine(WaterLowered());
            currentWater = p.waterHeld;
        }
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
            waterFill.color = c_damage;

            yield return new WaitForSeconds(.1f);
            waterFill.color = c_water;
 
            yield return new WaitForSeconds(.1f);
            
        }
    }
    
}

