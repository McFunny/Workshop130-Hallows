using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera mainCam;

    PlayerInventoryHolder playerInventoryHolder;
    PlayerEffectsHandler playerEffects;

    public bool isInteracting { get; private set; }
    public bool toolCooldown;

    public static PlayerInteraction Instance;

    public int currentMoney;

    public int health = 3;
    [HideInInspector] public readonly int maxHealth = 3;

    public int stamina = 100;
    [HideInInspector] public readonly int maxStamina = 100;

    public int waterHeld = 0; //for watering can
    [HideInInspector] public readonly int maxWaterHeld = 10;

    private float reach = 5;

    public LayerMask interactionLayers;

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
    
    void Start()
    {
        if(!mainCam) mainCam = FindObjectOfType<Camera>();
        playerInventoryHolder = FindObjectOfType<PlayerInventoryHolder>();
        playerEffects = FindObjectOfType<PlayerEffectsHandler>();
    }



    // Update is called once per frame
    void Update()
    {
        //LEFT CLICK USES THE ITEM CURRENTLY IN THE HAND
        if(Input.GetMouseButtonDown(0) && !PlayerMovement.accessingInventory)
        {
            UseHotBarItem();
        }

        //RIGHT CLICK USES AN ITEM ON A STRUCTURE, EX: PLANTING A SEED IN FARMLAND
        if(Input.GetMouseButtonDown(1) && !PlayerMovement.accessingInventory)
        {
            StructureInteractionWithItem();
        }

        //SPACE INTERACTS WITH A STRUCTURE WITHOUT USING AN ITEM, EX: HARVESTING A CROP
        if(Input.GetKeyDown(KeyCode.Space))
        {
            InteractWithObject();
        }

        if(Input.GetKeyDown("f"))
        {
            //TO TEST CLEARING A STRUCTURE
            DestroyStruct();
        }

        if(waterHeld > maxWaterHeld) waterHeld = maxWaterHeld;
        if(health > maxHealth) health = maxHealth;
        if(stamina > maxStamina) stamina = maxStamina;

    }

    void StartInteraction(IInteractable interactable)
    {
        //For NPC's and the Chest
        interactable.Interact(this, out bool interactSuccessful);
        isInteracting = false;
    }

    void StartInteractionWithItem(IInteractable interactable)
    {
        //For showing/giving NPC's items
        if(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData != null) interactable.InteractWithItem(this, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
        else return;
        isInteracting = false;
    }

    void EndInteraction()
    {
        isInteracting = false;
    }

    void DestroyStruct()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, interactionLayers))
        {
            Destroy(hit.collider.gameObject);
        }
    }

    void StructureInteractionWithItem()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;


        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, interactionLayers))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                StartInteractionWithItem(interactable); //Interacts with chest and npc's. I should eventually make this compatable with the structures I made - Cam
                return;
            }

            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.ItemInteraction(HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                Debug.Log("Interacted with item");
                return;
            }
        }
    }

    void InteractWithObject()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, interactionLayers))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                StartInteraction(interactable);


                if(hit.collider.GetComponent<ChestInventory>() != null) PlayerMovement.accessingInventory = true;  // Needs to check if opening a chest, else this should not be called
                Debug.Log("Opened Inventory of Interactable Object");
                return;
            }

            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.StructureInteraction();
                Debug.Log("Interacting with a structure");
                return;
            }
        }

        //if nothing, progress dialogue
        DialogueController.Instance.AdvanceDialogue();
        
    }


    void UseHotBarItem()
    {
       
        InventoryItemData item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;

        //Is it a Tool item?
        ToolItem t_item = item as ToolItem;
        if (t_item)
        {
            t_item.PrimaryUse(mainCam.transform);
        }

        //Is it a placeable item?
        PlaceableItem p_item = item as PlaceableItem;
        if (p_item)
        {
            p_item.PlaceStructure(mainCam.transform);
        }
    }

    public void PlayerTakeDamage()
    {
        playerEffects.PlayerDamage();
    }

    public IEnumerator ToolUse(ToolBehavior tool, float time, float coolDown)
    {
        if(toolCooldown) yield break;
        toolCooldown = true;
        yield return new WaitForSeconds(time);
        tool.ItemUsed();
        yield return new WaitForSeconds(coolDown - time);
        toolCooldown = false;
        //use a bool that says i am done swinging to avoid tool overlap
    }
    
}
