using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiningHelmet : ArmorBehavior
{


    public override void OnEquip()
    {
        base.OnEquip();
        ArmorManager.Instance.miningLight.SetActive(true);
        Debug.Log("Im mining itIm mining itIm mining itIm mining itIm mining itIm mining itIm mining itIm mining itIm mining itIm mining itIm mining itIm mining it");
    }

    public override void OnUnequip()
    {
        base.OnUnequip();
        ArmorManager.Instance.miningLight.SetActive(false);
    }
}
