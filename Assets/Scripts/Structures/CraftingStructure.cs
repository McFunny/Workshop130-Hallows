using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CraftingStructure : StructureBehaviorScript
{
    public List<CraftSlotData> craftSlots = new List<CraftSlotData>();
    private CraftingSystem craftingSystem;
    public int currentSlot;
    private const int CRAFTCAP = 5;
    public bool isCrafting = false;
    public Coroutine craftCoroutine;

    public AudioSource loopingSource;

    public void Start()
    {
        base.Start();
        craftingSystem = FindObjectOfType<CraftingSystem>();

    }
    public override void StructureInteraction()
    {
        //THIS IS WHERE U DO THE CODE TO BRING UP THE MENU Thank you cam very cool
        craftingSystem.SetCurrentStructure(this);
        StartCoroutine(WaitToOpenCraftingInterface());

    }

    private IEnumerator WaitToOpenCraftingInterface()
    {
        yield return new WaitForSeconds(0.1f);
        craftingSystem.OpenCraftingInterface();
    }

    public void AddCraft(CraftingEntry craft)
    {
        if (CanAddCraft())
        {
            CraftSlotData newSlot = new CraftSlotData();
            newSlot.assignedCraft = craft;
            newSlot.timeRemaining = craft.craftTimeInSeconds;
            craftSlots.Add(newSlot);
            if (!isCrafting)
            {
                craftCoroutine = StartCoroutine(PerformCraft());
            }
        }
    }

    public IEnumerator PerformCraft()
    {
        isCrafting = true;
        currentSlot = 0;

        for (int i = 0; i < craftSlots.Count; i++)
        {
            if (craftSlots[i].isComplete == false)
            {
                currentSlot = i;
                break;
            }
        }

        if (craftingSystem.currentStructure == this)
        {
            craftingSystem.UpdateTimerText(craftSlots[currentSlot].timeRemaining);
        }
        StartCoroutine(CraftTimer());

        while (craftSlots[currentSlot].timeRemaining > 0)
        {
            yield return null;
        }
        print("Craft Complete");
        //craftSlots.RemoveAt(0);
        craftSlots[currentSlot].isComplete = true;

        isCrafting = false;

        for (int i = 0; i < craftSlots.Count; i++)
        {
            if (craftSlots[i].isComplete == false)
            {
                craftCoroutine = StartCoroutine(PerformCraft());
                break;
            }
        }

        if (craftingSystem.currentStructure == this)
        {
            craftingSystem.UpdateActiveCrafts();
        }
    }

    private IEnumerator CraftTimer()
    {
        while (craftSlots[currentSlot].timeRemaining > 0)
        {
            yield return new WaitForSeconds(1f);
            craftSlots[currentSlot].timeRemaining--;
            //Debug.Log("Time Remaining: " + craftSlots[currentSlot].timeRemaining);
            if (craftingSystem.currentStructure == this)
            {
                craftingSystem.UpdateTimerText(craftSlots[currentSlot].timeRemaining);
            }
        }
    }

    public bool CanAddCraft()
    {
        if (craftSlots.Count < CRAFTCAP)
        {
            return true;
        }
        else return false;
    }

    public void StopCrafting()
    {
        if (isCrafting)
        {
            StopAllCoroutines();
            isCrafting = false;
            audioHandler.PlaySound(audioHandler.activatedSound);
            loopingSource.Stop();
        }
    }

    public void StartCrafting()
    {
        if (!isCrafting && craftSlots.Count > 0)
        {
            craftCoroutine = StartCoroutine(PerformCraft());
            StartCoroutine(LoopAudio());
        }
    }

    IEnumerator LoopAudio()
    {
        if(Random.Range(0,4) == 1) loopingSource.clip = audioHandler.miscSounds1[0];
        else loopingSource.clip = audioHandler.miscSounds1[Random.Range(0, audioHandler.miscSounds1.Length)];
        loopingSource.Play();

        float musicRuntime = loopingSource.clip.length;
        yield return new WaitForSecondsRealtime(musicRuntime);
    }
}

public class CraftSlotData
{
    public CraftingEntry assignedCraft;
    public int timeRemaining;
    public bool isComplete = false;
}

