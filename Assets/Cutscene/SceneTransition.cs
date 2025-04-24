using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public Material skyMat;
    void Start()
    {
        StartCoroutine(TimedTransition());
        skyMat.SetFloat("_BlendCubemaps", 1f);
    }

    IEnumerator TimedTransition()
    {
        yield return new WaitForSeconds(45);
        SceneManager.LoadSceneAsync(1);
    }
}
