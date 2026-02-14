using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WagonHPUI : MonoBehaviour
{
    [SerializeField] private GameObject mainUIContainer;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image sliderFill;
    [SerializeField] private Color defaultColor, damagedColor;
    [SerializeField] private UISpriteAnim wagonAnimator;
    private float currentHealth;
    private bool isDamagedCoroutineRunning;


    private void Start()
    {
        WagonManager.Instance.onWagonHPChanged += OnWagonHPChange;
        WildernessManager.OnWildernessEnter += OnWildernessEnter;
        WildernessManager.OnWildernessLeave += OnWildernessLeave;

        currentHealth = healthSlider.value;
    }

    private void OnDisable()
    {
        WagonManager.Instance.onWagonHPChanged -= OnWagonHPChange;
        WildernessManager.OnWildernessEnter -= OnWildernessEnter;
        WildernessManager.OnWildernessLeave -= OnWildernessLeave;
    }

    private void OnWagonHPChange()
    {
        healthSlider.maxValue = WagonManager.Instance.maxWagonHealth;
        healthSlider.value = WagonManager.Instance.wagonHealth;

        if(healthSlider.value < currentHealth)
        {
            if(!isDamagedCoroutineRunning) StartCoroutine(WagonDamaged());
        }

        currentHealth = healthSlider.value;
    }

    private void OnWildernessEnter()
    {
        OnWagonHPChange();
        mainUIContainer.SetActive(true);
    }

    private void OnWildernessLeave()
    {
        mainUIContainer.SetActive(false);
    }

    IEnumerator WagonDamaged()
    {
        isDamagedCoroutineRunning = true;
        wagonAnimator.PlayOneShotUI();
        for(int i = 0; i < 4; i++)
        {
            sliderFill.color = damagedColor;

            yield return new WaitForSeconds(.1f);
            sliderFill.color = defaultColor;
 
            yield return new WaitForSeconds(.1f);
            
        }
        isDamagedCoroutineRunning = false;
    }
}
