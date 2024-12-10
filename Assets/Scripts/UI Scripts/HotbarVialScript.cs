using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarVialScript : MonoBehaviour
{
    public Image waterBar, staminaBar;
    float waterVal, staminaVal, waterMax, staminaMax;
    PlayerInteraction playerInteraction;
    // Update is called once per frame
    void Start()
    {
        playerInteraction = PlayerInteraction.FindAnyObjectByType<PlayerInteraction>();
        waterMax = playerInteraction.maxWaterHeld;
        staminaMax = playerInteraction.maxStamina;
    }
    void Update()
    {
        UpdateStamina();
        UpdateWater();
    }

    void UpdateStamina()
    {
        waterVal = playerInteraction.waterHeld;
        waterBar.fillAmount = waterVal / waterMax;
    }

    void UpdateWater()
    {
        staminaVal = playerInteraction.stamina;
        staminaBar.fillAmount = staminaVal / staminaMax;
    }
}
