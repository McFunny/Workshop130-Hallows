using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class MainMenuScript : MonoBehaviour
{
    public InputActionReference hideUI;
    public GameObject menuObject, defaultObject, controlsDefault, controlsCanvas;
    ControlManager controlManager;
    public AudioSource source;
    public AudioClip hover, select;
    bool isTransitioning = false;
    // Start is called before the first frame update
    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        //source.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        //hideUI.action.started += HideUI;
    }

    private void OnDisable()
    {
        //controlManager.moreInfo.action.started -= HideUI;
    }

    // Update is called once per frame
    void Update()
    {
        //print(isPaused);
        //print(EventSystem.current.currentSelectedGameObject);
        //print(controlManager.playerInput.currentActionMap);
        if(EventSystem.current.currentSelectedGameObject == null && ControlManager.isGamepad)
        {
            if(controlsDefault.activeInHierarchy)EventSystem.current.SetSelectedGameObject(controlsDefault);
            else{EventSystem.current.SetSelectedGameObject(defaultObject);}
            print("Default Menu Object Selected");
        } 

        if(hideUI.action.WasPressedThisFrame())
        {
            if(!controlsCanvas.activeInHierarchy){HideUI();}
        }
    }
    void HideUI()
    {
        menuObject.SetActive(!menuObject.activeInHierarchy);
    }

    public void TestPress()
    {
        print("Test");
    }
    public void ExitGame()
    {
        if(isTransitioning) return;
        Application.Quit();
        print("Game Exited Successfully :)");
    }

    public void NewGame()
    {
        if(isTransitioning) return;
        isTransitioning = true;
        StartCoroutine(StartNewGame());
    }

    IEnumerator StartNewGame()
    {
        FadeScreen.coverScreen = true;
        yield return new WaitForSecondsRealtime(1);
        SceneManager.LoadSceneAsync(1);
    }

    public void OpenControlsScreen()
    {
        if(isTransitioning) return;
        controlsCanvas.SetActive(true);
        EventSystem.current.SetSelectedGameObject(controlsDefault);
    }

    public void OnHover()
    {
        if(isTransitioning) return;
        source.PlayOneShot(hover);
    }

    public void OnSelect()
    {
        if(isTransitioning) return;
        source.PlayOneShot(select);
    }
    
}
