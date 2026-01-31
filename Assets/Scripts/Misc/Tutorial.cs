using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public PopupScript tillP, plantP, waterP, killP, weedP, completeP, dontDestroySeedsP, creatureP, corpseP, codexP, structureP;

    public static Tutorial Instance;

    public GameObject scarecrow, weed, hog;

    public StructureObject weedData;

    public TutorialPhase phase;

    public bool hasWatered;

    public enum TutorialPhase
    {
        Till,
        Sow,
        Water,
        Kill,
        Weed,
        Complete,
        Codex,
        StructurePlace
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
        PopupHandler.Instance.AddToQueue(plantP);
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
            if(!hasWatered)
            {
                PopupHandler.Instance.AddToQueue(waterP);
                phase = TutorialPhase.Water;
            }
            else
            {
                PopupHandler.Instance.AddToQueue(structureP);
                phase = TutorialPhase.StructurePlace;
            }
        }
        PopupEvents.current.PlantSeed();
    }

    public void LostSeed()
    {
        if(phase == TutorialPhase.Water)
        {
            PopupHandler.Instance.AddToQueue(dontDestroySeedsP);
            PopupHandler.Instance.AddToQueue(structureP);
            phase = TutorialPhase.StructurePlace;
        }
    }

    public void WateredSeed()
    {
        if(phase == TutorialPhase.Water)
        {
            PopupHandler.Instance.ClearQueue();
            PopupHandler.Instance.AddToQueue(structureP);
            phase = TutorialPhase.StructurePlace;
        }
        PopupEvents.current.WateredCrop();
        hasWatered = true;
    }

    public void KillScarecrow() //Unused
    {
        /*if(phase == TutorialPhase.Kill)
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
        }*/
    }

    public void WeedDug()
    {
        if(phase == TutorialPhase.Weed)
        {
            PopupHandler.Instance.AddToQueue(codexP);
            phase = TutorialPhase.Codex;
            PopupEvents.current.WeedDug();

            if(MainMenuScript.currentFileMode != FileMode.Survival) QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetMainQuest(13));
            else PopupHandler.Instance.AddToQueue(PopupHandler.Instance.bedTutorialPopup); 
        }
    }

    public void KillCreature()
    {
        PopupEvents.current.KillCreature();

        PopupHandler.Instance.AddToQueue(corpseP);
    }

    public void ClearedCorpse()
    {
        print("Cleared Corpse");
        PopupEvents.current.ClearedCorpse();

        if(StructureManager.Instance.TallyStructure(weedData) == 0) //Should never happen but just in case
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

    public void WeedDestroyed()
    {
        if(StructureManager.Instance.TallyStructure(weedData) == 0) Instantiate(weed, StructureManager.Instance.GetRandomClearTile(), Quaternion.identity);
    }

    public void OpenCodex()
    {
        if(phase == TutorialPhase.Codex)
        {
            PopupHandler.Instance.AddToQueue(completeP);
            phase = TutorialPhase.Complete;
            PopupEvents.current.OpenCodex();
            Destroy(gameObject);
        }
    }

    public void PlaceStructure()
    {
        if(phase == TutorialPhase.StructurePlace)
        {
            PopupHandler.Instance.ClearQueue();
            PopupHandler.Instance.AddToQueue(creatureP);
            phase = TutorialPhase.Kill;

            Instantiate(hog, NightSpawningManager.Instance.RandomMistPosition(), Quaternion.identity);
        }
    }

    void OnDestroy()
    {
        TimeManager.Instance.stopTime = false;
    }
}
