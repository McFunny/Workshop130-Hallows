using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burrow : StructureBehaviorScript
{
    bool isDigging;
    
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
        if(type == ToolType.Shovel && !isDigging)
        {
            StartCoroutine(Dig());
            success = true;
        }
    }

    IEnumerator Dig()
    {
        isDigging = true;
        yield return new WaitForSeconds(1f);
        audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);
        ParticlePoolManager.Instance.GrabPoofParticle().transform.position = transform.position;
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        Destroy(this.gameObject);
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
}
