using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UntilledTile : StructureBehaviorScript
{
    public GameObject firstTill, secondTill;
    public GameObject farmTile;

    bool ignoreNextHour = true;

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        clearTileOnDestroy = false;
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Hoe)
        {
            if(secondTill.activeSelf)
            {
                Instantiate(farmTile, transform.position, Quaternion.identity);
                Destroy(this.gameObject);
            }
            else
            {
                secondTill.SetActive(true);
                firstTill.SetActive(false);
            }
            success = true;
        }
    }

    public override void HourPassed()
    {
        if(ignoreNextHour)
        {
            ignoreNextHour = false;
            return;
        }
        Destroy(this.gameObject);
    }
}
