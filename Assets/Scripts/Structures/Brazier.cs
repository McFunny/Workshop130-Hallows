using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brazier : StructureBehaviorScript
{
    public FireFearTrigger fireTrigger;
    public GameObject fire;

    public float flameLeft; //if 0, fire is gone
    float maxFlame = 5;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        fireTrigger.OnScare += EnemyScaredByFire;
        StartCoroutine(FireDrain());
        flameLeft = 0;
        fire.SetActive(false);
    }

    void Update()
    {
        base.Update();
    }

    public override void StructureInteraction()
    {
        if(flameLeft == 0)
        {
            flameLeft = maxFlame;
            fire.SetActive(true);
            audioHandler.PlaySound(audioHandler.activatedSound);
        }
    }

    IEnumerator FireDrain()
    {
        float r;
        while(gameObject.activeSelf)
        {
            r = Random.Range(10, 15);
            yield return new WaitForSeconds(r);
            r = Random.Range(0,2);
            flameLeft -= r;
            if(flameLeft < 0) flameLeft = 0;
            if(flameLeft == 0 && fire.activeSelf)
            {
                ExtinguishFlame();
            }
        }
    }

    void ExtinguishFlame()
    {
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        audioHandler.PlaySound(audioHandler.miscSounds1[0]);
    }

    public override void HitWithWater()
    {
        if(flameLeft > 0)
        {
            flameLeft = 0;
            ExtinguishFlame();
        }
    }

    void OnDestroy()
    {
        fireTrigger.OnScare -= EnemyScaredByFire;
        base.OnDestroy();
        //if (!gameObject.scene.isLoaded) return; 
    }

    void EnemyScaredByFire()
    {
        if(flameLeft <= 0) return;
        float r = Random.Range(0,10);
        if(r < 4) flameLeft -= r;
        if(flameLeft <= 0) ExtinguishFlame();
    }
}
