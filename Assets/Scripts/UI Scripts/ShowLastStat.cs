using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowLastStat : MonoBehaviour
{
    [SerializeField] private PetStatsUI petStatsUI;
    [SerializeField] private CropStatsRework cropStatsRework;
    [SerializeField] private StructureStatsUI structureStatsUI;
    // Start is called before the first frame update
    void Start()
    {
        petStatsUI.OnPetStatsShown += ShowPetStats;
        cropStatsRework.OnCropStatsShown += ShowCropStats;
        structureStatsUI.OnStructureStatsShown += ShowStructureStats;
    }

    private void ShowPetStats()
    {
        petStatsUI.statsContainer.SetActive(true);
        cropStatsRework.cropStatsParent.SetActive(false);
        structureStatsUI.structureStatContainer.SetActive(false);
    }

    private void ShowCropStats()
    {
        petStatsUI.statsContainer.SetActive(false);
        cropStatsRework.cropStatsParent.SetActive(true);
        structureStatsUI.structureStatContainer.SetActive(false);
    }

    private void ShowStructureStats()
    {
        petStatsUI.statsContainer.SetActive(false);
        cropStatsRework.cropStatsParent.SetActive(false);
        structureStatsUI.structureStatContainer.SetActive(true);
    }
}
