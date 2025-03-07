using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public PopupScript tillP, plantP, waterP, killP, weedP, completeP, dontDestroySeedsP;

    public static Tutorial Instance;

    public GameObject scarecrow, weed;

    public StructureObject weedData;

    public TutorialPhase phase;

    public enum TutorialPhase
    {
        Till,
        Sow,
        Water,
        Kill,
        Weed,
        Complete
    }

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        PopupHandler.Instance.AddToQueue(tillP);
        TimeManager.Instance.stopTime = true;
    }
    
    public void TilledGround()
    {
        if(phase == TutorialPhase.Till)
        {
            PopupHandler.Instance.AddToQueue(plantP);
            phase = TutorialPhase.Sow;
        }
    }

    public void PlantedSeed()
    {
        if(phase == TutorialPhase.Sow)
        {
            PopupHandler.Instance.AddToQueue(waterP);
            phase = TutorialPhase.Water;
            PopupEvents.current.PlantSeed();
        }
    }

    public void LostSeed()
    {
        if(phase == TutorialPhase.Water)
        {
            PopupHandler.Instance.AddToQueue(dontDestroySeedsP);
            PopupHandler.Instance.AddToQueue(killP);
            phase = TutorialPhase.Kill;
            PopupEvents.current.WateredCrop();

            StructureBehaviorScript guy = Instantiate(scarecrow, StructureManager.Instance.GetRandomClearTile(), Quaternion.identity).GetComponentInParent<StructureBehaviorScript>();;
            guy.health = 4;
        }
    }

    public void WateredSeed()
    {
        if(phase == TutorialPhase.Water)
        {
            PopupHandler.Instance.AddToQueue(killP);
            phase = TutorialPhase.Kill;
            PopupEvents.current.WateredCrop();
            //spawnScarecrow
            StructureBehaviorScript guy = Instantiate(scarecrow, StructureManager.Instance.GetRandomClearTile(), Quaternion.identity).GetComponentInParent<StructureBehaviorScript>();
            guy.health = 4;
        }
    }

    public void KillScarecrow()
    {
        if(phase == TutorialPhase.Kill)
        {
            PopupEvents.current.KillStructure();

            if(StructureManager.Instance.TallyStructure(weedData) == 0)
            {
                PopupHandler.Instance.AddToQueue(completeP);
                phase = TutorialPhase.Complete;
                PopupEvents.current.WeedDug();
                Destroy(gameObject);
            }
            else
            {
                PopupHandler.Instance.AddToQueue(weedP);
                phase = TutorialPhase.Weed;
            }
        }
    }

    public void WeedDug()
    {
        if(phase == TutorialPhase.Weed)
        {
            PopupHandler.Instance.AddToQueue(completeP);
            phase = TutorialPhase.Complete;
            PopupEvents.current.WeedDug();
            Destroy(gameObject);
        }
    }

    public void WeedDestroyed()
    {
        //Instantiate(weed, StructureManager.Instance.GetRandomClearTile(), Quaternion.identity);
    }

    void OnDestroy()
    {
        TimeManager.Instance.stopTime = false;
    }
}
