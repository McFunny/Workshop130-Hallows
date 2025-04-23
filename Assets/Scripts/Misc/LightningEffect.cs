using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningEffect : MonoBehaviour
{
    public Light light;
    public IEnumerator PlayLightning()
    {
        int times = Random.Range(2,5);
        int x = 0;
        yield return new WaitForSeconds(0.5f);
        while(x < times)
        {
            light.enabled = true;
            yield return new WaitForSeconds(Random.Range(0.07f, 0.11f));
            light.enabled = false;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.15f));
            x++;
        }
    }
}
