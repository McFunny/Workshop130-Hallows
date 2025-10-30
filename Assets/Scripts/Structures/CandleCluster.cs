using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleCluster : StructureBehaviorScript
{
    public FireFearTrigger fireTrigger;
    public GameObject fire;

    bool burning = false;

    float chanceForDrain = 25;

    //Candles are crafted from 1 silk, 3-5 combs, and 1 nectar OR bug meat. Probably made in bulk

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        fire.SetActive(false);
        if(TimeManager.Instance.isDay) chanceForDrain = 0;
    }

    void Update()
    {
        base.Update();
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        print("Interacted");
        if(type == ToolType.Torch)
        {
            print("Torch");
            if(PlayerInteraction.Instance.torchLit && !burning)
            {
                fire.SetActive(true);
                audioHandler.PlaySound(audioHandler.activatedSound);
                success = true;
            }
            else if(burning && !PlayerInteraction.Instance.torchLit)
            {
                HandItemManager.Instance.TorchFlameToggle(true);
                success = true;
            }
            else success = false;
            return;
        }
        else if(type == ToolType.Shovel)
        {
            success = true;
        }
        else if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && burning)
        {
            PlayerInteraction.Instance.waterHeld--;
            HitWithWater();
            success = true;
        }
        else if (type == ToolType.Pyrefly && burning && !PlayerInteraction.Instance.pyreflyLit)
        {
            HandItemManager.Instance.PyreflyFlameToggle(true);
            success = true;
        }
        else success = false;
        
    }

    public override void HourPassed()
    {
        if(!burning) return;
        if(Random.Range(0,100) < chanceForDrain)
        {
            chanceForDrain = 25;
            health--;

            if(Random.Range(0,10) > 6) ExtinguishFlame();
        }
        else chanceForDrain += 25;
    }

    public override void HitWithWater()
    {
        if(!burning) return;
        ExtinguishFlame();
    }


    void ExtinguishFlame()
    {
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = fire.transform.position;
        fire.SetActive(false);
        audioHandler.PlaySound(audioHandler.miscSounds1[0]);
        burning = false;
        chanceForDrain = 25;
    }
}
