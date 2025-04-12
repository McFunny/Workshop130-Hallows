using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CreditsScript : MonoBehaviour
{
    public Animator animator;
    public GameObject skipContainer, skipKBM, skipController, loadingScreen;
    bool canPress = false;
    AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(Credits());
    }

    // Update is called once per frame
    void Update()
    {
        if(ControlManager.isController)
        {
            skipController.SetActive(true);
            skipKBM.SetActive(false);
        }
        else
        {
            skipController.SetActive(false);
            skipKBM.SetActive(true);
        }

        if(skipContainer.activeSelf && canPress)
        {
            if(Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
            {
                StartCoroutine(GoToMainMenu());
            }
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(GoToMainMenu());
            }
            
        }
    }

    void LateUpdate()
    {
        /*if(Input.anyKeyDown)
        {
            skipContainer.SetActive(true);
            StopCoroutine(CheckForInput());
            StartCoroutine(CheckForInput());
        }*/
    }

    IEnumerator Credits()
    {
        yield return new WaitForSeconds(2f);
        animator.SetTrigger("StartAnim");
        print("Anim Started");
    }

    IEnumerator CheckForInput() //Fix this post-alpha
    {
        print("Skipcoroutine Started");
        yield return new WaitForSeconds(3f);
        yield return new WaitForEndOfFrame();
        skipContainer.SetActive(false);
    }

    public void StartMusic()
    {
        //Start Music Here
        print("Music Starts Here");
        audioSource.PlayOneShot(audioSource.clip);
    }

    public void ShowSkipInput()
    {
        skipContainer.SetActive(true);
        canPress = true;
    }

    public IEnumerator GoToMainMenu()
    {
        canPress = false;
        AsyncOperation operation = SceneManager.LoadSceneAsync(0);

        loadingScreen.SetActive(true);
        var loadText = loadingScreen.GetComponentInChildren<TextMeshProUGUI>();

        var load1 = "Loading";
        var load2 = "Loading.";
        var load3 = "Loading..";
        var load4 = "Loading...";

        while(!operation.isDone)
        {
            loadText.text = load1;
            yield return new WaitForSecondsRealtime(.2f);
            loadText.text = load2;
            yield return new WaitForSecondsRealtime(.2f);
            loadText.text = load3;
            yield return new WaitForSecondsRealtime(.2f);
            loadText.text = load4;
            yield return new WaitForSecondsRealtime(.2f);
        }
    }

}
