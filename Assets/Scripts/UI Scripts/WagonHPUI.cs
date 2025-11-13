using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WagonHPUI : MonoBehaviour
{
    [SerializeField] private Slider healthSlider;


    private void OnEnable()
    {
        WagonManager.Instance.onWagonHPChanged += OnWagonHPChange;
    }

    private void OnDisable()
    {
        WagonManager.Instance.onWagonHPChanged -= OnWagonHPChange;
    }

    private void OnWagonHPChange()
    {
        healthSlider.maxValue = WagonManager.Instance.maxWagonHealth;
        healthSlider.value = WagonManager.Instance.wagonHealth;
    }
}
