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
    public const string FileName = "DemoSaveGame.sav"; 

    //public static string saveDirectory => SaveDirectory;
    //public static string fileName => FileName;

    public static UnityAction OnSaveGame;
    public static UnityAction<SaveData> OnLoadGame;
    public static UnityAction<SaveData> OnLateLoad;

    public static bool SaveGame(SaveData data)
    {
        OnSaveGame?.Invoke();

        var dir = Application.persistentDataPath + SaveDirectory + MainMenuScript.currentSaveSlot; //check what full directory is

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

    /*public void SavePartial() //Save data when leaving to menu
    {
        // 1. Load the existing data from disk
        ///////////////
        SaveData tempData = new SaveData();

        if (File.Exists(fullPath))
        {
            string json = File.ReadAllText(fullPath);
            tempData = JsonUtility.FromJson<SaveData>(json);
        }
        else
        {
            Debug.LogError("Save file does not exist!");
        }
        /////

        // 2. Apply updates
        //
        tempData.allGameSaveData

        // 3. Save it back
        string updatedJson = JsonUtility.ToJson(tempData, true);
        File.WriteAllText(dir + FileName, updatedJson);
    }*/

    public static void LoadGame()
    {



        string fullPath = Application.persistentDataPath + SaveDirectory + MainMenuScript.currentSaveSlot + FileName;
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

        SaveLoad.OnLateLoad?.Invoke(SaveLoad.CurrentSaveData);


        }

        public static void DeleteSaveData()
        {
            string fullPath = Application.persistentDataPath + SaveDirectory + MainMenuScript.currentSaveSlot + FileName;
            if (File.Exists(fullPath))
            { 
                File.Delete(fullPath); 
            }
        }

    public static bool IsThereSaveData()
    {
        string fullPath = Application.persistentDataPath + SaveDirectory + MainMenuScript.currentSaveSlot + FileName;
        if (File.Exists(fullPath)) return true;
        else return false;
    }
}
}

