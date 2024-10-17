using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NPCNameText;
    [SerializeField] private TextMeshProUGUI NPCDialogueText;

    private Queue<string> paragraphs = new Queue<string>();
    private Queue<Emotion> emotions = new Queue<Emotion>();

    private bool conversationEnded;
    private string p;

    private Emotion e;

    public AudioSource source;

    
    public NPC currentTalker;
    int currentPath = -1;

    public void DisplayNextParagraph(DialogueText dialogueText, int path)
    {
        // If nothing left in queue
        if(path != currentPath)
        {
            currentPath = path;
            EndConversation();
            StartConversation(dialogueText);
            
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
        p = p.Replace("{itemValue}", $"{HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.value * HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData.sellValueMultiplier}");
        if(p.Contains("{itemSold}"))
        {
            p = p.Replace("{itemSold}", $"{""}");
            PlayerSoldItem();
        }

        NPCDialogueText.text = p;

        if (paragraphs.Count == 0)
        {
            conversationEnded = true;
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

        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    public void PlayerSoldItem()
    {
        InventoryItemData soldItem = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        if(!soldItem) return;
        float moneyGained = soldItem.value * soldItem.sellValueMultiplier;
        int moneyGainedInt = (int) moneyGained;
        PlayerInteraction.Instance.currentMoney += moneyGainedInt;
        HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
        PlayerInventoryHolder.Instance.UpdateInventory();
    }
}
