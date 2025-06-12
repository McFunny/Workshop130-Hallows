using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance;

    public GameObject dialogueBox;

    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;

    private Queue<string> paragraphs = new Queue<string>();
    private Queue<Emotion> emotions = new Queue<Emotion>();

    private bool conversationEnded; //playing last piece of dialogue
    private bool isTalking = false; //no more dialogue
    private bool interruptable = true;
    public bool restartDialogue = false;
    private bool freezePlayer = false;
    private bool canAdvanceDialogue = true; //Pauses dialogue at the start so the player cant mash through it

    private string p;

    private Emotion e;

    public AudioSource source;
    public AudioClip start, end;

    
    public NPC currentTalker;
    private int currentPath = -1;
    private PathType currentType;
    private InventoryItemData currentItemToGive;
    private int amountToGive;

    PlayerEffectsHandler playerEffects;

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

        playerEffects = FindObjectOfType<PlayerEffectsHandler>();
    }

    public void AdvanceDialogue()
    {
        if(IsTalking() == true && currentTalker && canAdvanceDialogue) DisplayNextParagraph(currentTalker.dialogueText, currentPath, currentType);
    }

    IEnumerator StartDialogueCooldown() //So the player does not accidentally skip over the start
    {
        canAdvanceDialogue = false;
        yield return new WaitForSeconds(0.5f);
        canAdvanceDialogue = true;
    }

    public void DisplayNextParagraph(DialogueText dialogueText, int path, PathType type)
    {
        // If nothing left in queue
        isTalking = true;
        if(path != currentPath || type != currentType || restartDialogue)
        {
            currentPath = path;
            currentType = type;
            restartDialogue = false;

            //EndConversation();
            paragraphs.Clear();
            emotions.Clear();

            conversationEnded = false;
            isTalking = false;

            DisplayNextParagraph(dialogueText, currentPath, currentType);
            if(!interruptable) print("You just interrupted dialogue");
            return;
            
        }
        source.Stop();
        if (paragraphs.Count == 0)
        {
            if(!conversationEnded)
            {
                StartConversation(dialogueText, type);
            }
            else
            {
                EndConversation();
                return;
            }
        }

        // If something in queue
        p = paragraphs.Dequeue();

        e = emotions.Dequeue();

        switch(e)
        {
            case Emotion.Neutral:
                source.PlayOneShot(currentTalker.neutral[Random.Range(0, currentTalker.neutral.Length)]);
                break;
            case Emotion.Happy:
                source.PlayOneShot(currentTalker.happy[Random.Range(0, currentTalker.happy.Length)]);
                break;
            case Emotion.Sad:
                source.PlayOneShot(currentTalker.sad[Random.Range(0, currentTalker.sad.Length)]);
                break;
            case Emotion.Angry:
                source.PlayOneShot(currentTalker.angry[Random.Range(0, currentTalker.angry.Length)]);
                break;
            case Emotion.Shocked:
                source.PlayOneShot(currentTalker.shocked[Random.Range(0, currentTalker.shocked.Length)]);
                break;
            case Emotion.Confused:
                source.PlayOneShot(currentTalker.confused[Random.Range(0, currentTalker.confused.Length)]);
                break;
            default:
                break;
        }

        // Update convo text
        UpdateStringVariables();
        

        NPCDialogueText.text = p;

        if (paragraphs.Count == 0)
        {
            conversationEnded = true;
            interruptable = true;
            //isTalking = false;
        }
        else interruptable = false;
        
    }

    private void StartConversation(DialogueText dialogueText, PathType type)
    {
        StartCoroutine(StartDialogueCooldown());
        // Activate the text box
        if (!dialogueBox.activeSelf)
        {
            dialogueBox.SetActive(true);
            source.PlayOneShot(start);
        }

        print(currentPath);
        print(type);

        //Update Name
        NPCNameText.text = dialogueText.speakerName;

        // Add dialogue text to queue
        switch (type)
        {
            case PathType.QuestComplete:
                for (int i = 0; i < dialogueText.questCompletePaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.questCompletePaths[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.questCompletePaths[currentTalker.currentPath].emotions.Count <= i) dialogueText.questCompletePaths[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.questCompletePaths[currentTalker.currentPath].emotions[i]);
                }

                break;
            case PathType.RepeatItem:
                for (int i = 0; i < dialogueText.repeatedItemPath.paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.repeatedItemPath.paragraphs[i]);
                    if(dialogueText.repeatedItemPath.emotions.Count <= i) dialogueText.repeatedItemPath.emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.repeatedItemPath.emotions[i]);
                }
                break;
            case PathType.Misc:
                for (int i = 0; i < dialogueText.paths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.paths[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.paths[currentTalker.currentPath].emotions.Count <= i) dialogueText.paths[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.paths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.Filler:
                for (int i = 0; i < dialogueText.fillerPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.fillerPaths[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.fillerPaths[currentTalker.currentPath].emotions.Count <= i) dialogueText.fillerPaths[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.fillerPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.Quest:
                for (int i = 0; i < dialogueText.questPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.questPaths[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.questPaths[currentTalker.currentPath].emotions.Count <= i) dialogueText.questPaths[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.questPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.ItemRecieved:
                for (int i = 0; i < dialogueText.itemRecievedPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.itemRecievedPaths[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.itemRecievedPaths[currentTalker.currentPath].emotions.Count <= i) dialogueText.itemRecievedPaths[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.itemRecievedPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.ItemSpecific:
                for (int i = 0; i < dialogueText.itemSpecificRemarks[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.itemSpecificRemarks[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.itemSpecificRemarks[currentTalker.currentPath].emotions.Count <= i) dialogueText.itemSpecificRemarks[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.itemSpecificRemarks[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.AlreadySpoken:
                for (int i = 0; i < dialogueText.alreadySpoken[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.alreadySpoken[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.alreadySpoken[currentTalker.currentPath].emotions.Count <= i) dialogueText.alreadySpoken[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.alreadySpoken[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.BranchingPaths:
                for (int i = 0; i < dialogueText.branchingPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.branchingPaths[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.branchingPaths[currentTalker.currentPath].emotions.Count <= i) dialogueText.branchingPaths[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.branchingPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.GivingDaily:
                for (int i = 0; i < dialogueText.dailyQuestPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.dailyQuestPaths[currentTalker.currentPath].paragraphs[i]);
                    if(dialogueText.dailyQuestPaths[currentTalker.currentPath].emotions.Count <= i) dialogueText.dailyQuestPaths[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.dailyQuestPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            default:
                for(int i = 0; i < dialogueText.defaultPath.paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.defaultPath.paragraphs[i]);
                    //if(dialogueText.defaultPath[currentTalker.currentPath].emotions.Count <= i) dialogueText.defaultPath[currentTalker.currentPath].emotions.Add(Emotion.Null);
                    emotions.Enqueue(dialogueText.defaultPath.emotions[i]);
                }
                break;
        }

        if(freezePlayer && currentTalker && currentTalker.eyeLine) PlayerCam.Instance.NewObjectOfInterest(currentTalker.eyeLine.position);
        
    }

    public void EndConversation()
    {
        // Clear queue
        print("ConvoEnded");
        paragraphs.Clear();
        emotions.Clear();

        conversationEnded = false;
        isTalking = false;

        if(freezePlayer)
        {
            freezePlayer = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
        }

        currentTalker.OnConvoEnd();
        currentTalker = null;

        if(dialogueBox.activeSelf)
        {
            source.PlayOneShot(end);
            dialogueBox.SetActive(false);
        }

        interruptable = true;
    }

    public void PlayerSoldItem()
    {
        int itemAmount = HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize;
        InventoryItemData soldItem = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        if(!soldItem) return;
        float moneyGained = soldItem.value * soldItem.sellValueMultiplier * itemAmount;
        int moneyGainedInt = (int) moneyGained;
        PlayerInteraction.Instance.currentMoney += moneyGainedInt;
        PlayerInteraction.Instance.totalMoneyEarned += moneyGainedInt;
        HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(itemAmount);
        PlayerInventoryHolder.Instance.UpdateInventory();
    }

    public void PlayerBoughtItem()
    {
        InventoryItemData item = currentTalker.lastInteractedStoreItem.itemData;
        if(item.cannotEnterInventory)
        {
            if(item.itemBehavior) item.itemBehavior.OnRecieve();
            PlayerInteraction.Instance.currentMoney -= currentTalker.lastInteractedStoreItem.cost;
            FindObjectOfType<PlayerEffectsHandler>().ItemCollectSFX();

            currentTalker.EmptyShopItem();
            return;
        }

        var inventory = PlayerInventoryHolder.Instance;
        if (inventory.AddToInventory(item, 1))
        {
            PlayerInteraction.Instance.currentMoney -= currentTalker.lastInteractedStoreItem.cost;
            FindObjectOfType<PlayerEffectsHandler>().ItemCollectSFX();

            currentTalker.EmptyShopItem();
            //put it in inventory and remove money
        }
        
    }

    void UpdateStringVariables()
    {
        p = p.Replace("{replacementString1}", currentTalker.ReplacementString1());
        p = p.Replace("{replacementString2}", currentTalker.ReplacementString2());
        p = p.Replace("{replacementString3}", currentTalker.ReplacementString3());

        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData)
        {
            float value = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.value * HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.sellValueMultiplier;
            value = Mathf.Floor(value);
            p = p.Replace("{itemValue}", $"{"<color=#E0D38F>" + value + "</color>"}");
            p = p.Replace("{itemTotalValue}", $"{"<color=#E0D38F>" + (value * HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize) + "</color>"}");
            p = p.Replace("{itemName}", $"{"<color=#81C6DE>" + HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.displayName + "</color>"}");

            if(p.Contains("{itemSold}"))
            {
                p = p.Replace("{itemSold}", $"{""}");
                PlayerSoldItem();
            }
            if(p.Contains("{itemGiven}"))
            {
                p = p.Replace("{itemGiven}", $"{""}");
                HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                PlayerInventoryHolder.Instance.UpdateInventory();
            }
        } 

        if(currentTalker.lastInteractedStoreItem)
        {
            p = p.Replace("{storeItemName}", $"{"<color=#81C6DE>" + currentTalker.lastInteractedStoreItem.itemData.displayName + "</color>"}");
            p = p.Replace("{storeItemValue}", $"{"<color=#E0D38F>" + currentTalker.lastInteractedStoreItem.cost + "</color>"}");
        }

        if(p.Contains("{itemBought}"))
        {
            p = p.Replace("{itemBought}", $"{""}");
            PlayerBoughtItem();
        }

        if(p.Contains("{freezePlayer}"))
        {
            p = p.Replace("{freezePlayer}", $"{""}");
            if(!freezePlayer)
            {
                freezePlayer = true;
                PlayerMovement.restrictMovementTokens++;
                playerEffects.StartCoroutine(playerEffects.Focus());
            }
        }

        if(p.Contains("{giveGift}"))
        {
            p = p.Replace("{giveGift}", $"{""}");
            for(int i = 0; i < amountToGive; i++)
            {
                if(PlayerInventoryHolder.Instance.AddToInventory(currentItemToGive, 1) == false)
                {
                    if(currentItemToGive.isKeyItem)
                    {
                        //put this in a mailbox or smth
                    }
                    GameObject droppedItem = ItemPoolManager.Instance.GrabItem(currentItemToGive);
                    droppedItem.transform.position = transform.position;
                }
            }
        }

        if(p.Contains("{givePlayerItems}"))
        {
            p = p.Replace("{givePlayerItems}", $"{""}");
            foreach(ItemWithAmount x in currentTalker.itemsToGive)
            {
                PlayerInventoryHolder.Instance.AddToInventory(x.item, x.amount);
            }
            currentTalker.itemsToGive.Clear();
        }
        
    }

    public void SetInterruptable(bool b)
    {
        //pointless function now, text is interruptable only when finished talking
        interruptable = b;
    }

    public void SetItem(InventoryItemData item, int amount)
    {
        currentItemToGive = item;
        amountToGive = amount;
    }

    public int GetPath()
    {
        return currentPath;
    }

    public bool IsTalking()
    {
        if(currentTalker) return true;
        return isTalking;
    }

    public bool IsInterruptable()
    {
        return interruptable;
    }

    public bool FreeToSpeak(NPC talker)
    {
        if(currentTalker == null || currentTalker == talker)
        {
            print("It's my turn to talk");
            return true;
        }
        else
        {
            print("It's not my turn to talk");
            return false;
        }
    }
}
