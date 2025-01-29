using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterTimer : MonoBehaviour
{
    public float lifeTime;

    public bool destroyOnCompletion = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        StartCoroutine(LifeTime());
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(lifeTime);
        if(destroyOnCompletion) Destroy(this.gameObject);
        gameObject.SetActive(false);
    }
}
