using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuriedKukri : StructureBehaviorScript
{
    public GameObject kukriPrefab;
    public void Awake()
    {
        base.Awake();
        PlayerInteraction.Instance.lostKukri = false;
    }
    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DigPlant());
            success = true;
        }
    }

    public override void DigAction()
    {
        audioHandler.PlaySoundAtPoint(audioHandler.interactSound, transform.position);

        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        //give the knife
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        PlayerInteraction.Instance.lostKukri = false;
        GameObject knife = Instantiate(kukriPrefab, new Vector3(transform.position.x, transform.position.y + 0.8f, transform.position.z), transform.rotation);
    }
}
