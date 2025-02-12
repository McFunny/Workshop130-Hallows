using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Events;

namespace SaveLoadSystem
{
public static class SaveLoad
{
    public static SaveData CurrentSaveData = new SaveData();

    public const string SaveDirectory = "/SaveData/";
    public const string FileName = "SaveGame.sav";

    public static UnityAction OnSaveGame;
    public static UnityAction<SaveData> OnLoadGame;

    public static bool SaveGame(SaveData data)
    {
        OnSaveGame?.Invoke();

        var dir = Application.persistentDataPath + SaveDirectory; //check what full directory is

        if (!Directory.Exists(dir)) //if it doesnt exist create the folder
        {
            Directory.CreateDirectory(dir);
        }

        string json = JsonUtility.ToJson(CurrentSaveData, true); //writes the save file
        File.WriteAllText(dir + FileName, json);

        Debug.Log("Saving Game");

        GUIUtility.systemCopyBuffer = dir; //copies directory to clipboard

        return true;
    }

    public static void LoadGame()
    {



        string fullPath = Application.persistentDataPath + SaveDirectory + FileName;
        SaveData tempData = new SaveData();

        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            tempData = JsonUtility.FromJson<SaveData>(json);

            OnLoadGame?.Invoke(tempData);
                Debug.Log("How many times is this running");
        }
        else
        {
            Debug.LogError("Save file does not exist!");
        }

        CurrentSaveData = tempData;


    }

        public static void DeleteSaveData()
        {
            string fullPath = Application.persistentDataPath + SaveDirectory + FileName;
            if (File.Exists(fullPath))
            { 
                File.Delete(fullPath); 
            }
        }
}
}

