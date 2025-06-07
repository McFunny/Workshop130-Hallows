using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public Material skyMat;
    bool loading = false;
    void Start()
    {
        StartCoroutine(TimedTransition());
        skyMat.SetFloat("_BlendCubemaps", 1f);
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && !loading)
        {
            loading = true;
            SceneManager.LoadSceneAsync(1);
        }
    }

    IEnumerator TimedTransition()
    {
        yield return new WaitForSeconds(45);
        SceneManager.LoadSceneAsync(1);
    }
}
