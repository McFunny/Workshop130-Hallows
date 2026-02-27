using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingMist : MonoBehaviour
{
    bool triggered;


    void OnTriggerEnter(Collider other)
    {
        if(triggered) return;
        triggered = true;
        StartCoroutine(EndGame());
    }

    IEnumerator EndGame()
    {
        FadeScreen.coverScreen = true;
        PlayerMovement.restrictMovementTokens++;
        yield return new WaitForSeconds(10);
        SceneManager.LoadSceneAsync(2);
    }
}
