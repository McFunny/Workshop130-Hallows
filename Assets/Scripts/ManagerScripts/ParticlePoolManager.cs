using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticlePoolManager : MonoBehaviour
{
    public static ParticlePoolManager Instance;

    public ParticleSystem dirtParticle;

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
}
