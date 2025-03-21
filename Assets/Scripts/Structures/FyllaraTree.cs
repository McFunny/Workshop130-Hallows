using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FyllaraTree : StructureBehaviorScript
{
    public GameObject treeNut;

    public Transform[] nutSpawns;
    public SpriteRenderer renderer;
    bool isFilled = false;
    public int treeStage = -1;

    public GameObject[] treeStages;
    public GameObject[] treeNuts;


    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        //if(treeStage == -1) RandomizeTreeStage();
        //OnDamage += TreeNutDrop;
    }

    void Update()
    {
        base.Update();

    }

    public override void StructureInteraction()
    {
        
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && !isFilled)
        {
            //FillWithWater();
            success = true;
        }
    }

    public override void HourPassed()
    {
        //
    }

    void OnDestroy()
    {
        //OnDamage -= TreeNutDrop;
        base.OnDestroy();
    }
}
