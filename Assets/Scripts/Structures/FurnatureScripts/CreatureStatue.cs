using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureStatue : FurnitureBehaviorScript
{
    
    public List<GameObject> models = new List<GameObject>();

    public int modelNum = -1;

    public void Awake()
    {
        base.Awake();
    }

    public void Start()
    {
        base.Start();
        FurnitureStart();
    }

    void UpdateModel()
    {
        if(modelNum == -1) modelNum = Random.Range(0, models.Count);

        foreach(GameObject decor in models) decor.SetActive(false);
        models[modelNum].SetActive(true);
    }

    public override void LoadVariables()
    {
        modelNum = saveInt1;
        UpdateModel();
    }

    public override void SaveVariables()
    {
        saveInt1 = modelNum;
    }

    
}

