using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurvivalModeManager : MonoBehaviour
{

    void Start()
    {
        StartCoroutine(DelayedStart());
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(2);
        if(MainMenuScript.currentFileMode != FileMode.Survival)
        {
            gameObject.SetActive(false);
            yield break;
        }
    }

}
