using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WildernessTransitionManager : MonoBehaviour
{
    public static WildernessTransitionManager Instance;

    public GameObject transitionScene;

    public Transform playerSpawn;

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
        PlayerInteraction.Instance.transform.position = playerSpawn.position;
        StartCoroutine(TransitionTimer());
    }

    IEnumerator TransitionTimer()
    {
        TimeManager.Instance.stopTime = true;
        MouseItemData.canDropItems = false;
        yield return new WaitForSeconds(15);
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
