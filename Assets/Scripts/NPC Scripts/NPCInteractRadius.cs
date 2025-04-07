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
        if(!npcScript) npcScript = GetComponentInParent<NPC>();
    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player") 
        {
            if(npcScript && dialogueController.currentTalker != null && dialogueController.currentTalker == npcScript) //to make sure walking npcs dont disable another conversation
            {
                dialogueController.currentTalker.PlayerLeftRadius();
                dialogueController.EndConversation();
                Debug.Log("dialogueEnded");
            }
            else if(npcScript)
            {
                npcScript.PlayerLeftRadius();
            }
        }
    }
}
