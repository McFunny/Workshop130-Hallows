using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMeters : MonoBehaviour
{
    public Image waterBar, staminaBar;
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
    }
}
