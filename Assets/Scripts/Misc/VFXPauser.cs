using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXPauser : MonoBehaviour
{
    public VisualEffect[] effects;

    public Mesh hiPoly, lowPoly;

    bool paused = false;
    public bool useLowPoly = true;

    void Awake()
    {
        effects = GetComponentsInChildren<VisualEffect>();

        int x = 0;
        foreach(VisualEffect v in effects)
        {
            if(x > 0) return;
            //v.SetFloat("playRate", 0.75f);
            if(!hiPoly || !lowPoly) return;
            if(useLowPoly) v.SetMesh("SmokeMesh", lowPoly);
            else v.SetMesh("SmokeMesh", hiPoly);

            x++;
        }

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
        paused = false;
        while(effects.Length > 0)
        {
            yield return new WaitForSeconds(4);
            if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) > 150)
            {
                if(!paused)
                {
                    foreach(VisualEffect v in effects)
                    {
                        v.pause = true;
                    }
                    paused = true;
                }
                
            }
            else
            {
                if(paused)
                {
                    foreach(VisualEffect v in effects)
                    {
                        v.pause = false;
                    }
                    paused = false;
                }

            }
        }
    }
}
