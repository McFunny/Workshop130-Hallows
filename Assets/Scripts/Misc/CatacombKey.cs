using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatacombKey : MonoBehaviour
{
    void Awake()
    {
        if(GameSaveData.Instance.keyCollected) Destroy(this.gameObject);
    }

    void OnDisable()
    {
        if (!gameObject.scene.isLoaded) return;
        GameSaveData.Instance.keyCollected = true;
    }
}
