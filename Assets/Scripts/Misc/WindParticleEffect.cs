using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindParticleEffect : MonoBehaviour
{
    public ParticleSystem system;
    ParticleSystem.Particle[] particles;

    void OnEnable()
    {
        AmbientAudioManager.OnWindBlow += WindPush;
    }

    void OnDisable()
    {
        AmbientAudioManager.OnWindBlow -= WindPush;
    }

    void WindPush(Vector3 dir)
    {
        StopAllCoroutines();
        StartCoroutine(ApplyForce(8, dir));
    }

    IEnumerator ApplyForce(int repeats, Vector3 dir)
    {
        int t = 0;
        int numParticlesAlive;
        while(t < repeats)
        {
            particles = new ParticleSystem.Particle[system.main.maxParticles];
            numParticlesAlive = system.GetParticles(particles);

            for (int i = 0; i < numParticlesAlive; i++)
            {
                particles[i].velocity += dir * 5.05f;
            }
            system.SetParticles(particles, numParticlesAlive);
            yield return new WaitForSeconds(2);
            t++;
        }
    }

}
