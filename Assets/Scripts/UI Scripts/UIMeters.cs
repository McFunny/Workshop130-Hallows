using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMeters : MonoBehaviour
{
    public Image waterBar, staminaBar, staminaEmptyImage;
    public GameObject waterEmptyFill, staminaEmptyFill, leftTextbox, rightTextbox;
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
        waterBar.fillAmount = p.waterHeld/p.maxWaterHeld;
        staminaBar.fillAmount = p.stamina/p.maxStamina;

        leftText.text = p.waterHeld + "/" + p.maxWaterHeld;
        rightText.text = p.stamina + "/" + p.maxStamina;

        if(p.stamina < currentStamina)
        {
            StartCoroutine(PlayerDamaged());
            currentStamina = p.stamina;
        }

        if (p.waterHeld <= 0)
        {
            waterEmptyFill.SetActive(false);
        }
        else
        {
            waterEmptyFill.SetActive(true);
        }

        if (p.stamina <= 0)
        {
            staminaEmptyFill.SetActive(false);
        }
        else
        {
            staminaEmptyFill.SetActive(true);
        }
    }

    IEnumerator PlayerDamaged()
    {
        for(int i = 0; i < 4; i++)
        {
            staminaBar.color = c_damage;
            staminaEmptyImage.color = c_damage;
            yield return new WaitForSeconds(.1f);
            staminaBar.color = c_stamina;
            staminaEmptyImage.color = c_stamina;
            yield return new WaitForSeconds(.1f);
            
        }
    }
    
}
