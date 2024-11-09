using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager Instance;

    public ParticleSystem dirtParticle;

    public VisualEffect hitEffect;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else Instance = this;

    }

    public void MoveAndPlayParticle(Vector3 pos, ParticleSystem p)
    {
        p.transform.position = pos;
        p.Play();
    }

    public void MoveAndPlayVFX(Vector3 pos, VisualEffect v)
    {
        v.transform.position = pos;
        v.Play();
    }
}
