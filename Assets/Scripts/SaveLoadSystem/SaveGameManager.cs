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

    void Start()
    {
        //if(MainMenuScript.loadingData) TryLoadData();
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(0.2f);
        if(MainMenuScript.loadingData) TryLoadData();
    }

    private void OnDisable()
    {
        SaveLoad.OnLoadGame -= LoadData;
    }

    public void DeleteData()
    {
        SaveLoad.DeleteSaveData();
    }

    private void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.O))
        {
            SaveData();
        }

        if (Input.GetKeyUp(KeyCode.P))
        {
            TryLoadData();
            
        }*/
    }

    public static void SaveData()
    {
       
        data = SaveLoad.CurrentSaveData;

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
