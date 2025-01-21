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
        if(IsTalking() == true && currentTalker) DisplayNextParagraph(currentTalker.dialogueText, currentPath, currentType);
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
                source.PlayOneShot(currentTalker.neutral);
                break;
            case Emotion.Happy:
                source.PlayOneShot(currentTalker.happy);
                break;
            case Emotion.Sad:
                source.PlayOneShot(currentTalker.sad);
                break;
            case Emotion.Angry:
                source.PlayOneShot(currentTalker.angry);
                break;
            case Emotion.Shocked:
                source.PlayOneShot(currentTalker.shocked);
                break;
            case Emotion.Confused:
                source.PlayOneShot(currentTalker.confused);
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
        switch(type)
        {
            case PathType.QuestComplete:
                for(int i = 0; i < dialogueText.questCompletePath.paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.questCompletePath.paragraphs[i]);
                    emotions.Enqueue(dialogueText.questCompletePath.emotions[i]);
                }
                break;
            case PathType.RepeatItem:
                for(int i = 0; i < dialogueText.repeatedItemPath.paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.repeatedItemPath.paragraphs[i]);
                    emotions.Enqueue(dialogueText.repeatedItemPath.emotions[i]);
                }
                break;
            case PathType.Misc:
                for(int i = 0; i < dialogueText.paths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.paths[currentTalker.currentPath].paragraphs[i]);
                    emotions.Enqueue(dialogueText.paths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.Filler:
                for(int i = 0; i < dialogueText.fillerPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.fillerPaths[currentTalker.currentPath].paragraphs[i]);
                    emotions.Enqueue(dialogueText.fillerPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.Quest:
                for(int i = 0; i < dialogueText.questPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.questPaths[currentTalker.currentPath].paragraphs[i]);
                    emotions.Enqueue(dialogueText.questPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.ItemRecieved:
                for(int i = 0; i < dialogueText.itemRecievedPaths[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.itemRecievedPaths[currentTalker.currentPath].paragraphs[i]);
                    emotions.Enqueue(dialogueText.itemRecievedPaths[currentTalker.currentPath].emotions[i]);
                }
                break;
            case PathType.ItemSpecific:
                for(int i = 0; i < dialogueText.itemSpecificRemarks[currentTalker.currentPath].paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.itemSpecificRemarks[currentTalker.currentPath].paragraphs[i]);
                    emotions.Enqueue(dialogueText.itemSpecificRemarks[currentTalker.currentPath].emotions[i]);
                }
                break;
            default:
                for(int i = 0; i < dialogueText.defaultPath.paragraphs.Length; i++)
                {
                    paragraphs.Enqueue(dialogueText.defaultPath.paragraphs[i]);
                    emotions.Enqueue(dialogueText.defaultPath.emotions[i]);
                }
                break;
        }
        
    }

    public void EndConversation()
    {
        // Clear queue
        paragraphs.Clear();
        emotions.Clear();

        conversationEnded = false;
        isTalking = false;

        if(freezePlayer)
        {
            freezePlayer = false;
            PlayerMovement.restrictMovementTokens--;
        }

        currentTalker.OnConvoEnd();

        if(dialogueBox.activeSelf)
        {
            source.PlayOneShot(end);
            dialogueBox.SetActive(false);
        }
    }

    public void PlayerSoldItem()
    {
        int itemAmount = HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize;
        InventoryItemData soldItem = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        if(!soldItem) return;
        float moneyGained = soldItem.value * soldItem.sellValueMultiplier * itemAmount;
        int moneyGainedInt = (int) moneyGained;
        PlayerInteraction.Instance.currentMoney += moneyGainedInt;
        HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(itemAmount);
        PlayerInventoryHolder.Instance.UpdateInventory();
    }

    public void PlayerBoughtItem()
    {
        var inventory = PlayerInventoryHolder.Instance;
        if (inventory.AddToInventory(currentTalker.lastInteractedStoreItem.itemData, 1))
        {
            PlayerInteraction.Instance.currentMoney -= currentTalker.lastInteractedStoreItem.cost;
            FindObjectOfType<PlayerEffectsHandler>().ItemCollectSFX();

            currentTalker.EmptyShopItem();
            //put it in inventory and remove money
        }
        
    }

    void UpdateStringVariables()
    {
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
            p = p.Replace("{storeItemValue}", $"{"<color=#E0D38F>" + currentTalker.lastInteractedStoreItem.itemData.value + "</color>"}");
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
        return isTalking;
    }

    public bool IsInterruptable()
    {
        return interruptable;
    }
}
