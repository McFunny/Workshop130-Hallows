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

    //CHANGE THIS LATER, ITS MESSY
    public TestNPC currentTalker;

    public void DisplayNextParagraph(DialogueText dialogueText)
    {
        // If nothing left in queue
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
        for(int i = 0; i < dialogueText.paragraphs.Length; i++)
        {
            paragraphs.Enqueue(dialogueText.paragraphs[i]);
            emotions.Enqueue(dialogueText.emotions[i]);
        }
    }

    public void EndConversation()
    {
        // Clear queue
        paragraphs.Clear();

        conversationEnded = false;

        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
