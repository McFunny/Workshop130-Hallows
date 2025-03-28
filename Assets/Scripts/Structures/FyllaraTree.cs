using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FyllaraTree : StructureBehaviorScript
{
    public GameObject treeNut;

    public List<Transform> nutSpawns; //possible places nuts can spawn
    public SpriteRenderer renderer; //for water
    bool isFilled = false;
    public int treeStage = -1;

    public GameObject[] treeStages; //Tree objects
    public GameObject[] currentTreeNuts = new GameObject[2]; //Current nuts on the tree

    int progressUntilNextGrowth = 0;
    int maxProgress = 3;

    public ParticleSystem leafBurst;


    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        if(treeStage == -1) RandomizeTreeStage();

        renderer.enabled = false;

        transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);

        OnDamage += TreeHit;
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
        if(isFilled && treeStage == (treeStages.Length - 1))
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

    void PopulateTreeNut()
    {
        for(int i = 0; i < nutSpawns.Count; i++)
        {
            if(currentTreeNuts[i] == null)
            {
                currentTreeNuts[i] = Instantiate(treeNut, nutSpawns[i].position, Quaternion.identity);
                if(Random.Range(0,2) == 1) return;
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
        OnDamage -= TreeHit;
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
        for(int i = 0; i < currentTreeNuts.Length; i++)
        {
            if(currentTreeNuts[i]) Destroy(currentTreeNuts[i]);
        }
    }

    void TreeHit()
    {
        leafBurst.Play();
    }
}
