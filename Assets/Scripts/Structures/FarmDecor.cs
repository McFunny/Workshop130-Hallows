using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmDecor : StructureBehaviorScript
{
    public List<GameObject> variations;

    int variationNum = -1;


    void Start()
    {
        StructureManager.Instance.allStructs.Add(this);
        clearTileOnDestroy = false;
        //base.Start();

        UpdateModel();
    }

    void UpdateModel()
    {
        if(variationNum == -1)
        {
            variationNum = Random.Range(0, variations.Count);
        }
        foreach(GameObject rock in variations) rock.SetActive(false);
        variations[variationNum].SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        Destroy(this.gameObject);
    }

    void OnDestroy()
    {
        base.OnDestroy();

        if (!gameObject.scene.isLoaded) return; 

        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
    }

    public override void LoadVariables()
    {
        variationNum = saveInt1;
        UpdateModel();
    }

    public override void SaveVariables()
    {
        saveInt1 = variationNum;
    }
}
