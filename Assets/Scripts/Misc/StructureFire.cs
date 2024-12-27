using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StructureFire : MonoBehaviour
{
    public StructureBehaviorScript burningStruct;
    public Transform flameBase;

    // Update is called once per frame
    void Update()
    {
        if(!burningStruct || !burningStruct.onFire)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        if(!gameObject.scene.isLoaded) return;
        ParticlePoolManager.Instance.GrabExtinguishParticle().transform.position = flameBase.position;
    }
}
