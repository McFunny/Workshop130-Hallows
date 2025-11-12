using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessTransitionManager : MonoBehaviour
{
    public static WildernessTransitionManager Instance;

    public GameObject transitionScene;

    public Transform playerTransitionPos, doorFocalPoint;

    public ScrollingTerrain terrainScript;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    public void EnterTransition()
    {
        transitionScene.SetActive(true);
        terrainScript.scrollTerrain = true;
        PlayerInteraction.Instance.transform.position = playerTransitionPos.position;
        PlayerCam.Instance.NewObjectOfInterest(doorFocalPoint.position);
        StartCoroutine(TransitionTimer());
    }

    IEnumerator TransitionTimer()
    {
        float timeForTransition = 15;

        TimeManager.Instance.stopTime = true;
        MouseItemData.canDropItems = false;
        AmbientAudioManager.Instance.StartCoroutine(AmbientAudioManager.Instance.FadeAudio(timeForTransition));
        yield return new WaitForSeconds(1f);
        FadeScreen.coverScreen = false;
        PlayerCam.Instance.ClearObjectOfInterest();
        PlayerMovement.restrictMovementTokens--;
        yield return new WaitForSeconds(timeForTransition);
        TimeManager.Instance.stopTime = false;
        StartCoroutine(ExitTransition());
    }

    IEnumerator ExitTransition()
    {
        //restrict movement and darken screen
        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        yield return new WaitForSeconds(3);
        WildernessManager.Instance.EnterWilderness();
        FadeScreen.coverScreen = false;
        PlayerMovement.restrictMovementTokens--;
        MouseItemData.canDropItems = true;

        transitionScene.SetActive(false);
        terrainScript.scrollTerrain = false;
    }
}
