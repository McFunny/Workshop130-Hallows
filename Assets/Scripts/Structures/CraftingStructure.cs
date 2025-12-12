using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingStructure : StructureBehaviorScript
{
    public List<CraftSlotData> craftSlots = new List<CraftSlotData>();
    private CraftingSystem craftingSystem;
    public int currentSlot;
    private const int CRAFTCAP = 5;
    public bool isCrafting = false;
    public Coroutine craftCoroutine, audioCoroutine;

    public AudioSource loopingSource;

    public Animator anim;
    public ParticleSystem fumes;

    public void Start()
    {
        base.Start();
        craftingSystem = FindObjectOfType<CraftingSystem>();
        TimeManager.OnUpdateCraftTimes += TimeSkipped;

    }
    private void OnDestroy()
    {
        base.OnDestroy();
        TimeManager.OnUpdateCraftTimes -= TimeSkipped;
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
                StartCrafting();
                Debug.Log("Craft Starting");
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
        CraftSlotData slot = null;

        for (int i = 0; i < craftSlots.Count; i++)
        {
            if (craftSlots[i].isComplete == false)
            {
                slot = craftSlots[i];
                craftCoroutine = StartCoroutine(PerformCraft());
                break;
            }
        }

        if (craftingSystem.currentStructure == this)
        {
            craftingSystem.UpdateActiveCrafts();
        }

        if(slot == null) AllCraftsFinished();
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

    public void TimeSkipped(int timePassed)
    {
        Debug.Log("Time Skipped: " + timePassed + " mins");
        StopCrafting();
        Debug.Log("Craft Stopped");
        int minsToRemove = timePassed;
        for (int i = 0; i < craftSlots.Count; i++)
        {
            if(craftSlots[i].isComplete) continue;

            if(minsToRemove >= craftSlots[i].timeRemaining)
            {
                minsToRemove -= craftSlots[i].timeRemaining;
                craftSlots[i].timeRemaining = 0;
            }
            else
            {
                craftSlots[i].timeRemaining -= minsToRemove;
                break;
            }
        }

        bool incompleteCraftFound = false;

        for (int i = 0; i < craftSlots.Count; i++)
        {
            if (craftSlots[i].timeRemaining <= 0)
            {
                craftSlots[i].isComplete = true;
            }
            else
            {
                Debug.Log("Craft " + i + " is incomplete. Starting Craft again");
                incompleteCraftFound = true;
                break;
            }
        }

        if(incompleteCraftFound == true)
        {
            StartCrafting();
            Debug.Log("Craft Starting");
        } 
        else AllCraftsFinished();
    }

    public void StopCrafting()
    {
        if (isCrafting)
        {
            StopCoroutine(craftCoroutine);
            isCrafting = false;
        }
    }

    public void StartCrafting()
    {
        if (!isCrafting && craftSlots.Count > 0)
        {
            if(audioCoroutine == null) audioCoroutine = StartCoroutine(LoopAudio());
            craftCoroutine = StartCoroutine(PerformCraft());

            fumes.Play();
            anim.SetBool("Running", true);
        }
    }

    private void AllCraftsFinished()
    {
        audioHandler.PlaySound(audioHandler.activatedSound);
        
        loopingSource.Stop();
        if(audioCoroutine != null) StopCoroutine(audioCoroutine);
        audioCoroutine = null;
        
        fumes.Stop();
        anim.SetBool("Running", false);
        print(anim.GetBool("Running"));
    }

    IEnumerator LoopAudio()
    {
        Debug.Log("We made it here");
        if(Random.Range(0,4) == 1) loopingSource.clip = audioHandler.miscSounds1[0];
        else loopingSource.clip = audioHandler.miscSounds1[Random.Range(0, audioHandler.miscSounds1.Length)];
        loopingSource.Play();

        float musicRuntime = loopingSource.clip.length;
        yield return new WaitForSecondsRealtime(musicRuntime);
    }
}

[System.Serializable]
public class CraftSlotData
{
    public CraftingEntry assignedCraft;
    public int timeRemaining;
    public bool isComplete = false;
}

