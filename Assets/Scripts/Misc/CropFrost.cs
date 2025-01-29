using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropFrost : MonoBehaviour
{
    public FarmLand afflictedTile;

    //Audio is handled through an audio source on each particle

    float cropDamage = 10;

    void Update()
    {
        if(!afflictedTile || afflictedTile.nearbyFires.Count > 0)
        {
            gameObject.SetActive(false);
        }
    }

    void OnDisable()
    {
        if(!gameObject.scene.isLoaded) return;
        ParticlePoolManager.Instance.GrabThawParticle().transform.position = transform.position;
        if(afflictedTile)
        {
            afflictedTile.isFrosted = false;
            afflictedTile = null;
        }
    }
}
