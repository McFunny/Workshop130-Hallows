using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMeters : MonoBehaviour
{
    public Slider waterBar, staminaBar;
    public Image waterFill, staminaFill;
    public GameObject leftTextbox, rightTextbox;
    public Color c_stamina, c_damage;
    PlayerInteraction p;
    float currentStamina;
    ControlManager controlManager;
    public TextMeshProUGUI leftText, rightText;
    void Start()
    {
        p = PlayerInteraction.Instance;
        currentStamina = p.stamina;
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

        leftText.text = p.waterHeld + "/" + p.maxWaterHeld;
        rightText.text = p.stamina + "/" + p.maxStamina;

        if(p.stamina < currentStamina)
        {
            StartCoroutine(PlayerDamaged());
            currentStamina = p.stamina;
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
    
}

