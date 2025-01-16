using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brazier : StructureBehaviorScript
{
    public FireFearTrigger fireTrigger;
    public GameObject fire;

    public float flameLeft; //if 0, fire is gone
    float maxFlame = 10;

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
        return;
        if(flameLeft == 0)
        {
            flameLeft = maxFlame;
            fire.SetActive(true);
            audioHandler.PlaySound(audioHandler.activatedSound);
        }
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        print("Interacted");
        if(type == ToolType.Torch)
        {
            print("Torch");
            if(PlayerInteraction.Instance.torchLit && flameLeft <= 0)
            {
                flameLeft = maxFlame;
                fire.SetActive(true);
                audioHandler.PlaySound(audioHandler.activatedSound);
                success = true;
            }
            else if(flameLeft > 0 && !PlayerInteraction.Instance.torchLit)
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                success = true;
            }
            else success = false;
            return;
        }
        else success = false;
        
    }

    IEnumerator FireDrain()
    {
        int r;
        while(gameObject.activeSelf)
        {
            r = Random.Range(10, 16);
            yield return new WaitForSeconds(r);
            flameLeft -= 1;
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

    void EnemyScaredByFire(bool successful)
    {
        if(flameLeft <= 0 || !successful) return;
        flameLeft -= 2;
        if(flameLeft <= 0) ExtinguishFlame();
    }
}
