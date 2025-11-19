using UnityEngine;
using UnityEngine.UI;

public class WagonHPUI : MonoBehaviour
{
    [SerializeField] private GameObject mainUIContainer;
    [SerializeField] private Slider healthSlider;


    private void Start()
    {
        WagonManager.Instance.onWagonHPChanged += OnWagonHPChange;
        WildernessManager.OnWildernessEnter += OnWildernessEnter;
        WildernessManager.OnWildernessLeave += OnWildernessLeave;
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
}
