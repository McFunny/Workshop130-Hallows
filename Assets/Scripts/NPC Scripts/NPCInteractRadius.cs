using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractRadius : MonoBehaviour
{
    [SerializeField] private DialogueController dialogueController;
    [SerializeField] private NPC npcScript;
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player" && dialogueController.currentTalker != null) 
        {
            if(npcScript && dialogueController.currentTalker != npcScript) return; //to make sure walking npcs dont disable another conversation
            dialogueController.EndConversation();
            dialogueController.currentTalker.PlayerLeftRadius();
            dialogueController.currentTalker = null;
            Debug.Log("dialogueEnded");
        }
    }
}
