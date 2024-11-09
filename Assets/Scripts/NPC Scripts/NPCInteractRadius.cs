using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteractRadius : MonoBehaviour
{
    [SerializeField] private DialogueController dialogueController;
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            dialogueController.EndConversation();
            if(dialogueController.currentTalker != null) dialogueController.currentTalker.PlayerLeftRadius();
            Debug.Log("dialogueEnded");
        }
    }
}
