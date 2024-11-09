using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance;

    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;

    private Queue<string> paragraphs = new Queue<string>();
    private Queue<Emotion> emotions = new Queue<Emotion>();

    private bool conversationEnded;
    private bool isTalking = false;
    private bool interruptable = true;
    public bool restartDialogue = false;
    private string p;

    private Emotion e;

    public AudioSource source;

    
    public NPC currentTalker;
    private int currentPath = -1;

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

    public void AdvanceDialogue()
    {
        if(IsTalking() == true && currentTalker && currentPath != -1) DisplayNextParagraph(currentTalker.dialogueText, currentPath);
    }

    public void DisplayNextParagraph(DialogueText dialogueText, int path)
    {
        // If nothing left in queue
        isTalking = true;
        if(path != currentPath || restartDialogue)
        {
            currentPath = path;
            restartDialogue = false;
            EndConversation();
            DisplayNextParagraph(dialogueText, currentPath);
            return;
            
        }
        if (paragraphs.Count == 0)
        {
            if(!conversationEnded)
            {
                StartConversation(dialogueText);
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
        
    }

    private void StartConversation(DialogueText dialogueText)
    {
        // Activate the text box
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        //Update Name
        NPCNameText.text = dialogueText.speakerName;

        // Add dialogue text to queue
        if(currentTalker.currentPath == -1)
        {
            for(int i = 0; i < dialogueText.defaultPath.paragraphs.Length; i++)
            {
                paragraphs.Enqueue(dialogueText.defaultPath.paragraphs[i]);
                emotions.Enqueue(dialogueText.defaultPath.emotions[i]);
            }
        }
        else
        {
            for(int i = 0; i < dialogueText.paths[currentTalker.currentPath].paragraphs.Length; i++)
            {
                paragraphs.Enqueue(dialogueText.paths[currentTalker.currentPath].paragraphs[i]);
                emotions.Enqueue(dialogueText.paths[currentTalker.currentPath].emotions[i]);
            }
        }
        
    }

    public void EndConversation()
    {
        // Clear queue
        paragraphs.Clear();
        emotions.Clear();

        conversationEnded = false;
        isTalking = false;

        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
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
            p = p.Replace("{itemValue}", $"{HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.value * HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.sellValueMultiplier}");
            p = p.Replace("{itemTotalValue}", $"{HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.value * HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.sellValueMultiplier * HotbarDisplay.currentSlot.AssignedInventorySlot.StackSize}");
            p = p.Replace("{itemName}", $"{HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.displayName}");
            if(p.Contains("{itemSold}"))
            {
                p = p.Replace("{itemSold}", $"{""}");
                PlayerSoldItem();
            }
        } 

        if(p.Contains("{itemBought}"))
        {
            p = p.Replace("{itemBought}", $"{""}");
            PlayerBoughtItem();
        }
        
    }

    public void SetInterruptable(bool b)
    {
        interruptable = b;
    }

    public int GetPath()
    {
        return currentPath;
    }

    public bool IsTalking()
    {
        return isTalking;
        //return conversationEnded;
    }

    public bool IsInterruptable()
    {
        return interruptable;
    }
}
