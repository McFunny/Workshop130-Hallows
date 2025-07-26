using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burrow : StructureBehaviorScript
{
    bool isDigging;

    public BugObject termite;
    
    void Awake()
    {
        base.Awake();
    }
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

        OnDamage += Damaged;
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(Dig());
            success = true;
        }
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0)
        {
            ParticlePoolManager.Instance.GrabSplashParticle().transform.position = transform.position;
            PlayerInteraction.Instance.waterHeld--;
            success = true;
            Destroy(gameObject);
        }
    }

    public override void DigAction()
    {
        audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(this.gameObject);
    }

    public override void HourPassed()
    {
        if(Random.Range(0,20) > 17) StartCoroutine(SpawnBug());
    }

    IEnumerator SpawnBug()
    {
        yield return new WaitForSeconds(Random.Range(2, 15));
        BugSpawningManager.Instance.SpawnBug(transform.position, termite);
    }

    void OnDestroy()
    {
        OnDamage -= Damaged;
        base.OnDestroy();
    }

    void Damaged()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }

    public override void HitWithWater()
    {
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(gameObject);
    }
}
