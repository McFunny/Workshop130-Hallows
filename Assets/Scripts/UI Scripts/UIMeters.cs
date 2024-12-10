using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMeters : MonoBehaviour
{
    public Image waterBar, staminaBar;
    public GameObject waterEmptyFill, staminaEmptyFill;
    PlayerInteraction p;
    void Start()
    {
        p = PlayerInteraction.Instance;
    }

    void Update()
    {
        UpdateMeters();
    }

    public void UpdateMeters()
    {
        waterBar.fillAmount = p.waterHeld/p.maxWaterHeld;
        staminaBar.fillAmount = p.stamina/p.maxStamina;

        if (p.waterHeld == 0)
        {
            waterEmptyFill.SetActive(false);
        }
        else
        {
            waterEmptyFill.SetActive(true);
        }

        if (p.stamina == 0)
        {
            staminaEmptyFill.SetActive(false);
        }
        else
        {
            staminaEmptyFill.SetActive(true);
        }
    }
}
