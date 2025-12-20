using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MudRoomDoor : MonoBehaviour
{
    [Header("Door Visuals")]
    public GameObject doorObj;
    public GameObject blackObj;

    [Header("Crop Keys")]
    public List<MudRoomCropKey> cropKeys;
    public List<CropData> potentialCrops;

    [Header("Puzzle State")]
    public bool puzzleSolved;


    AudioSource audioSource;
    private void Awake()
    {
        for (int i = 0; i < cropKeys.Count; i++)
        {
            cropKeys[i].keyIndex = i;
            cropKeys[i].OnCropInserted += HandleCropInserted;
        }
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void AssignCrops()
    {
        if (potentialCrops == null || potentialCrops.Count == 0)
        {
            Debug.LogError("MudRoomDoor: No potential crops assigned.");
            return;
        }

        foreach (var key in cropKeys)
        {
            int randomIndex = Random.Range(0, potentialCrops.Count);
            key.AssignCrop(potentialCrops[randomIndex]);
        }
    }

    void HandleCropInserted(MudRoomCropKey key)
    {
        CheckSolved();
    }

    void CheckSolved()
    {
        foreach (var key in cropKeys)
        {
            if (!key.cropInserted)
                return;
        }

        SolvePuzzle();
    }

    void SolvePuzzle()
    {
        puzzleSolved = true;
        StartCoroutine(HideDoor());
        Gachapon.Instance.AddToBacklog(Gachapon.Instance.siegePaper, 1);
    }
    IEnumerator HideDoor()
    {
        Vector3 startPos = doorObj.transform.position;
        Vector3 endPos = startPos + Vector3.down * 9f;
        audioSource.Play();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 3f;
            doorObj.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        doorObj.SetActive(false);

        Vector3 startScale = blackObj.transform.localScale;
        Vector3 endScale = new Vector3(startScale.x, startScale.y, 0f);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 2f;
            blackObj.transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        audioSource.Stop();
        blackObj.SetActive(false);
    }

    void ForceOpen()
    {
        doorObj.SetActive(false);
        blackObj.SetActive(false);
    }


    public MudRoomDoorSaveData ExportSaveData()
    {
        MudRoomDoorSaveData data = new MudRoomDoorSaveData
        {
            puzzleSolved = puzzleSolved,
            cropKeys = new List<MudRoomCropKeySaveData>()
        };

        foreach (var key in cropKeys)
            data.cropKeys.Add(key.ExportSaveData());

        return data;
    }

    public void ImportSaveData(MudRoomDoorSaveData data)
    {
        if (data.cropKeys == null || 0 == data.cropKeys.Count)
        {
            AssignCrops();
            return;
        }

        puzzleSolved = data.puzzleSolved;

        foreach (var keyData in data.cropKeys)
            cropKeys[keyData.keyIndex].ImportSaveData(keyData);

        if (puzzleSolved)
            ForceOpen();
    }

}

[System.Serializable]
public struct MudRoomCropKeySaveData
{
    public int keyIndex;
    public bool cropInserted;
    public string cropName;
}

[System.Serializable]
public struct MudRoomDoorSaveData
{
    public bool puzzleSolved;
    public List<MudRoomCropKeySaveData> cropKeys;
}

