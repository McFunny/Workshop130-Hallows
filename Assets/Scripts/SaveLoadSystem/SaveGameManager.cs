using SaveLoadSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveGameManager : MonoBehaviour
{
    public static SaveData data;

    private void Awake()
    {
        data = new SaveData();
        SaveLoad.OnLoadGame += LoadData;
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadData;
    }

    public void DeleteData()
    {
        SaveLoad.DeleteSaveData();
    }

    public static void SaveData()
    {
       
        SaveLoad.CurrentSaveData = data;

        SaveLoad.SaveGame(data);

    }

    public static void LoadData(SaveData _data)
    {
        data = _data;
    }

    public static void TryLoadData()
    {
        SaveLoad.LoadGame();
    }


}
