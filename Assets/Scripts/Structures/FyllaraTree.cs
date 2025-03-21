using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FyllaraTree : StructureBehaviorScript
{
    public GameObject treeNut;

    public List<Transform> nutSpawns; //possible places nuts can spawn
    List<Transform> nutSpawnsInUse; //used spots
    public SpriteRenderer renderer; //for water
    bool isFilled = false;
    public int treeStage = -1;

    public GameObject[] treeStages; //Tree objects
    public List<GameObject> currentTreeNuts; //Current nuts on the tree

    int progressUntilNextGrowth = 0;
    int maxProgress = 2;

    //When nuts drop, have the item fling to the player


    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        if(treeStage == -1) RandomizeTreeStage();
        OnDamage += TreeNutDrop;

        renderer.enabled = false;

        transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
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
            FillWithWater();
            success = true;
        }
    }

    public override void HitWithWater()
    {
        if(!isFilled) FillWithWater();
    }

    public override void HourPassed()
    {
        if(isFilled && treeStage == treeStages.Length)
        {
            progressUntilNextGrowth += Random.Range(1,3);
            if(progressUntilNextGrowth < maxProgress) return;
            progressUntilNextGrowth = 0;
            isFilled = false;
            renderer.enabled = false;
            PopulateTreeNut();
        }
    }

    void FillWithWater()
    {
        isFilled = true;
        renderer.enabled = true;
    }

    void TreeNutDrop()
    {
        for(int i = 0; i < currentTreeNuts.Count; i++)
        {
            if(currentTreeNuts[i])
            {
                currentTreeNuts[i].GetComponent<Rigidbody>().useGravity = true;
                return;
            }
        }
    }

    void PopulateTreeNut()
    {
        for(int i = 0; i < nutSpawns.Count; i++)
        {
            if(nutSpawnsInUse[i] != nutSpawns[i])
            {
                nutSpawnsInUse.Add(nutSpawns[i]);
                currentTreeNuts.Add(Instantiate(treeNut, nutSpawns[i].position, Quaternion.identity));
                return;
            }
        }
    }

    void RandomizeTreeStage()
    {
        //Right now just spawn it as a big tree
        treeStage = treeStages.Length - 1;

        for(int i = 0; i < treeStages.Length; i++)
        {
            if(i == treeStage) treeStages[i].SetActive(true);
            else treeStages[i].SetActive(false);
        }
    }

    void OnDestroy()
    {
        OnDamage -= TreeNutDrop;
        base.OnDestroy();
    }
}
