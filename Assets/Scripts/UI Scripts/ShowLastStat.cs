using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowLastStat : MonoBehaviour
{
    [SerializeField] private PetStatsUI petStatsUI;
    [SerializeField] private CropStatsRework cropStatsRework;
    // Start is called before the first frame update
    void Start()
    {
        petStatsUI.OnPetStatsShown += ShowPetStats;
        cropStatsRework.OnCropStatsShown += ShowCropStats;
    }

    private void ShowPetStats()
    {
        petStatsUI.statsContainer.SetActive(true);
        cropStatsRework.cropStatsParent.SetActive(false);
    }

    private void ShowCropStats()
    {
        petStatsUI.statsContainer.SetActive(false);
        cropStatsRework.cropStatsParent.SetActive(true);
    }
}
