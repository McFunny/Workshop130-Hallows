using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmDecor : StructureBehaviorScript
{
    public List<GameObject> variations;

    public List<int> rockVariations, leafVariations;

    int variationNum = -1;

    public DecorType type;


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
            if(type == DecorType.Rock) variationNum = rockVariations[Random.Range(0, rockVariations.Count)];
            else if(type == DecorType.Leaf) variationNum = leafVariations[Random.Range(0, leafVariations.Count)];
            else variationNum = Random.Range(0, variations.Count);
        }
        foreach(GameObject decor in variations) decor.SetActive(false);
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

public enum DecorType
{
    Any,
    Rock,
    Leaf
}
