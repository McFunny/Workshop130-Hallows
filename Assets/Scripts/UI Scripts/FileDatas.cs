using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FileDatas: MonoBehaviour
{
    public Button slotButton;
    public Button defaultButton, newGameButton, loadGameButton, deleteSaveButton, backButton;
    public GameObject slotData, slotButtons;
    public TextMeshProUGUI dayNumText, mintsCurrentText, mintsTotalText, emptySlot, difficultyText, siegesClearedText, fileNameText;
    public List<Image> siegeImages = new List<Image>();
    public string difficulty;
    public bool saveDataPresent = false;

    public int dayNum;
    public int mintsCurrent;
    public int mintsTotal;
    public int completedSieges;
    private MainMenuScript mainMenuScript;

    private void Awake()
    {
        mainMenuScript = FindFirstObjectByType<MainMenuScript>();
    }

    public void OnMainButtonPress()
    {
        EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
        slotData.SetActive(false);
        slotButtons.SetActive(true);
        mainMenuScript.OnSelect();
        mainMenuScript.SetSelectedLoadSlot(this.gameObject);
    }

    public void OnNewGame()
    {
        mainMenuScript.NewGameBool(true);
        mainMenuScript.LoadGame(newGameButton);
        mainMenuScript.OnSelect();
    }

    public void OnLoadSave()
    {
        mainMenuScript.NewGameBool(false);
        mainMenuScript.LoadGame(loadGameButton);
        mainMenuScript.OnSelect();
    }

    public void OnDeleteSave()
    {
        mainMenuScript.DeleteSave(deleteSaveButton);
    }

    public void OnBackButton()
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        slotData.SetActive(true);
        slotButtons.SetActive(false);
        mainMenuScript.OnSelect();
    }

    public void OnHover()
    {
        mainMenuScript.OnHover();
    }
}
