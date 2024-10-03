using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestNPC : NPC, ITalkable
{
    [SerializeField] private DialogueText dialogueText;
    [SerializeField] private DialogueController dialogueController;
    public AudioClip happy, sad, neutral, angry, confused, shocked;
    public override void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        Talk(dialogueText);
        interactSuccessful = true;
        Debug.Log("NPC Interact Successful");
    }

    public void Talk(DialogueText dialogueText)
    {
        dialogueController.currentTalker = this;
        dialogueController.DisplayNextParagraph(dialogueText);
    }
}
