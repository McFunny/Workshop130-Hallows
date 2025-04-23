using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXPauser : MonoBehaviour
{
    public VisualEffect[] effects;

    void Awake()
    {
        effects = GetComponentsInChildren<VisualEffect>();
    }

    void OnEnable()
    {
        //StopCoroutine(DistanceCheck());
        StartCoroutine(DistanceCheck());
    }

    void OnDisable()
    {
        StopCoroutine(DistanceCheck());
    }

    IEnumerator DistanceCheck()
    {
        if(effects == null || effects.Length == 0 || PlayerInteraction.Instance == null) yield break;
        foreach(VisualEffect v in effects)
        {
            v.pause = false;
        }
        while(effects.Length > 0)
        {
            yield return new WaitForSeconds(4);
            if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) > 150)
            {
                foreach(VisualEffect v in effects)
                {
                    v.pause = true;
                }
            }
            else
            {
                foreach(VisualEffect v in effects)
                {
                    v.pause = false;
                }
            }
        }
    }
}
