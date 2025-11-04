using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WagonTrackingParticles : MonoBehaviour
{
    public ParticleSystem system;
    ParticleSystem.Particle[] particles;

    void Update()
    {
        if(WagonManager.Instance == null || TownGate.Instance.location != PlayerLocation.InWilderness) return;
        PushParticles();
    }

    void PushParticles()
    {
        particles = new ParticleSystem.Particle[system.main.maxParticles];
        int numParticlesAlive = system.GetParticles(particles);

        Vector3 dir = (transform.position - WagonManager.Instance.wildernessWagon.transform.position).normalized;

        for (int i = 0; i < numParticlesAlive; i++)
        {
            particles[i].velocity -= dir * 0.05f;
        }
        system.SetParticles(particles, numParticlesAlive);
    }
}
