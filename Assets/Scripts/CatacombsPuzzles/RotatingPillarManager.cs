using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using SaveLoadSystem;

public class RotatingPillarManager : ImAPuzzleManager
{
    [System.Serializable]
    public class PuzzleSetEntry
    {
        public List<RotatingPillar> pillars;
        public CropKey cropKey;
        [HideInInspector] public bool isSolved = false;
    }

    public List<PuzzleSetEntry> puzzleSets = new List<PuzzleSetEntry>();
    public List<CropData> cropData = new List<CropData>();

    [SerializeField] private Database _database;
    private AudioSource audioSource;
    private int puzzlesSolved = 0;

    public static RotatingPillarManager Instance;

    [Header("Gachapon Stuff")]
    public InventoryItemData gachaponReward;
    public int gachaponRewardCount;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        AssignCropsToPuzzles();
    }

    private void AssignCropsToPuzzles()
    {
        var cropDataGroups = GroupCropDataByLength();

        foreach (var entry in puzzleSets)
        {
            AssignCropToPuzzleSet(entry.pillars, entry.cropKey, cropDataGroups);
        }
    }

    private Dictionary<int, List<CropData>> GroupCropDataByLength()
    {
        var cropDataGroups = new Dictionary<int, List<CropData>>();
        foreach (var crop in cropData)
        {
            int cropLength = crop.cropSprites.Length;
            if (!cropDataGroups.ContainsKey(cropLength))
                cropDataGroups[cropLength] = new List<CropData>();

            cropDataGroups[cropLength].Add(crop);
        }
        return cropDataGroups;
    }

    private void AssignCropToPuzzleSet(List<RotatingPillar> puzzleSet, CropKey cropKey, Dictionary<int, List<CropData>> cropDataGroups)
    {
        int puzzleSetLength = puzzleSet.Count;
        if (cropDataGroups.TryGetValue(puzzleSetLength, out var matchingCrops) && matchingCrops.Count > 0)
        {
            int randomIndex = Random.Range(0, matchingCrops.Count);
            CropData specifiedCrop = matchingCrops[randomIndex];
            matchingCrops.RemoveAt(randomIndex);

            cropKey.cropData = specifiedCrop;
            cropKey.SetUpSprites();

            foreach (var pillar in puzzleSet)
            {
                pillar.SetUpSprites(specifiedCrop);
                pillar.OnInteractionComplete += (completedPillar) => CheckPuzzleCompletion(puzzleSet);
                pillar.SetCropInsertionListener(cropKey);
            }
        }
        else
        {
            Debug.LogWarning($"No matching crops found for puzzle set with {puzzleSetLength} pillars.");
        }
    }

    private void CheckPuzzleCompletion(List<RotatingPillar> puzzleSet)
    {
        var entry = puzzleSets.Find(e => e.pillars == puzzleSet);
        if (entry == null || entry.isSolved) return;

        foreach (var pillar in puzzleSet)
        {
            if (!pillar.correctlyOrientated)
                return;
        }

        entry.isSolved = true;
        OnPuzzleSolved(puzzleSet);
    }

    private void OnPuzzleSolved(List<RotatingPillar> puzzleSet)
    {
        foreach (var pillar in puzzleSet)
        {
            pillar.LockPuzzle();
        }

        puzzlesSolved++;
        audioSource.Play();

        if (puzzleSets.All(e => e.isSolved))
        {
            Debug.Log("All puzzles solved! Great job!");
            puzzleSolved = true;
            PuzzleManager.Instance.totalPuzzlesSolved++;
            PuzzleManager.Instance.CheckToSeeIfPuzzlesAreComplete();
            Gachapon.Instance.AddToBacklog(gachaponReward, gachaponRewardCount);
        }
    }

    public RotatingPuzzleSaveData ExportSaveData()
    {
        List<PuzzleSetEntrySaveData> saveEntries = new List<PuzzleSetEntrySaveData>();

        foreach (var entry in puzzleSets)
        {
            saveEntries.Add(new PuzzleSetEntrySaveData
            {
                PillarData = ExportPuzzleSet(entry.pillars),
                CropKeyData = entry.cropKey.ExportSaveData(),
                IsSolved = entry.isSolved
            });
        }

        return new RotatingPuzzleSaveData
        {
            PuzzleSetEntries = saveEntries,
            PuzzlesSolved = puzzlesSolved
        };
    }

    public void ImportSaveData(RotatingPuzzleSaveData data)
    {
        puzzlesSolved = data.PuzzlesSolved;
        puzzleSolved = (puzzlesSolved == puzzleSets.Count);

        for (int i = 0; i < puzzleSets.Count; i++)
        {
            //The catch for old saves
            if(i >= data.PuzzleSetEntries.Count)
            {
                puzzleSets[i].isSolved = false;
                continue;
            }

            puzzleSets[i].isSolved = data.PuzzleSetEntries[i].IsSolved;
            puzzleSets[i].cropKey.ImportSaveData(data.PuzzleSetEntries[i].CropKeyData,
                Database.Instance.GetItem(data.PuzzleSetEntries[i].CropKeyData.CropYieldID));
            ImportPuzzleSet(puzzleSets[i].pillars, data.PuzzleSetEntries[i].PillarData);
        }
    }

    private List<RotatingPillarSaveData> ExportPuzzleSet(List<RotatingPillar> puzzleSet)
    {
        List<RotatingPillarSaveData> saveData = new List<RotatingPillarSaveData>();
        foreach (var pillar in puzzleSet)
        {
            saveData.Add(pillar.ExportSaveData());
        }
        return saveData;
    }

    private void ImportPuzzleSet(List<RotatingPillar> puzzleSet, List<RotatingPillarSaveData> saveData)
    {
        for (int i = 0; i < puzzleSet.Count; i++)
        {
            puzzleSet[i].ImportSaveData(saveData[i]);
        }
    }
}

[System.Serializable]
public struct RotatingPuzzleSaveData
{
    public List<PuzzleSetEntrySaveData> PuzzleSetEntries;
    public int PuzzlesSolved;
}

[System.Serializable]
public struct PuzzleSetEntrySaveData
{
    public List<RotatingPillarSaveData> PillarData;
    public CropKeySaveData CropKeyData;
    public bool IsSolved;
}
