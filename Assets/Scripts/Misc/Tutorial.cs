using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public PopupScript tillP, plantP, waterP, killP, weedP, completeP, dontDestroySeedsP;

    public static Tutorial Instance;

    public GameObject scarecrow, weed;

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
        }
    }

    public void LostSeed()
    {
        if(phase == TutorialPhase.Water)
        {
            PopupHandler.Instance.AddToQueue(dontDestroySeedsP);
            PopupHandler.Instance.AddToQueue(killP);
            phase = TutorialPhase.Kill;

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
            //spawnScarecrow
            StructureBehaviorScript guy = Instantiate(scarecrow, StructureManager.Instance.GetRandomClearTile(), Quaternion.identity).GetComponentInParent<StructureBehaviorScript>();
            guy.health = 4;
        }
    }

    public void KillScarecrow()
    {
        if(phase == TutorialPhase.Kill)
        {
            PopupHandler.Instance.AddToQueue(weedP);
            phase = TutorialPhase.Weed;
        }
    }

    public void WeedDug()
    {
        if(phase == TutorialPhase.Weed)
        {
            PopupHandler.Instance.AddToQueue(completeP);
            phase = TutorialPhase.Complete;
            Destroy(gameObject);
        }
    }

    public void WeedDestroyed()
    {
        Instantiate(weed, StructureManager.Instance.GetRandomClearTile(), Quaternion.identity);
    }
}
