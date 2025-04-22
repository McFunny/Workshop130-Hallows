using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(TimedTransition());
    }

    IEnumerator TimedTransition()
    {
        yield return new WaitForSeconds(45);
        SceneManager.LoadSceneAsync(1);
    }
}
