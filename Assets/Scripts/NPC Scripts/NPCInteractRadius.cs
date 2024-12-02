using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractRadius : MonoBehaviour
{
    private DialogueController dialogueController;
    [SerializeField] private NPC npcScript;
    void Start()
    {
        dialogueController = DialogueController.Instance;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && dialogueController.currentTalker != null) 
        {
            if(npcScript && dialogueController.currentTalker != npcScript) return; //to make sure walking npcs dont disable another conversation
            dialogueController.currentTalker.PlayerLeftRadius();
            dialogueController.EndConversation();
            Debug.Log("dialogueEnded");
        }
    }
}
