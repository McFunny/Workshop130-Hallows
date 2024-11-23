using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public Camera mainCam;

    PlayerInventoryHolder playerInventoryHolder;
    PlayerEffectsHandler playerEffects;

    ControlManager controlManager;

    Rigidbody rb;

    public bool isInteracting { get; private set; }
    public bool toolCooldown;

    public static PlayerInteraction Instance;

    public int currentMoney;

    public float stamina = 200;
    [HideInInspector] public readonly float maxStamina = 100;

    public float waterHeld = 0; //for watering can
    [HideInInspector] public readonly float maxWaterHeld = 10;

    private float reach = 5;

    public LayerMask interactionLayers;
    private bool ltCanPress = false;

    bool gameOver;

    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        stamina = maxStamina;
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
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        //LEFT CLICK USES THE ITEM CURRENTLY IN THE HAND
        controlManager.useHeldItem.action.started += UseHeldItem; 
        //RIGHT CLICK USES AN ITEM ON A STRUCTURE, EX: PLANTING A SEED IN FARMLAND
        controlManager.interactWithItem.action.started += OnInteractWithItem;
        //SPACE INTERACTS WITH A STRUCTURE WITHOUT USING AN ITEM, EX: HARVESTING A CROP
        controlManager.interactWithoutItem.action.started += InteractWithoutItem;
    }

    private void OnDisable()
    {
        controlManager.useHeldItem.action.started -= UseHeldItem;
        controlManager.interactWithItem.action.started -= OnInteractWithItem;
        controlManager.interactWithoutItem.action.started -= InteractWithoutItem;
    }

    // Update is called once per frame
    void Update()
    {
        if(waterHeld > maxWaterHeld) waterHeld = maxWaterHeld;
        if(stamina > maxStamina) stamina = maxStamina;

        DisplayHologramCheck();

        if(stamina <= 0 && !gameOver)
        {
            gameOver = true;
            StartCoroutine(GameOver());
        }

        if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory) return;

        if(Input.GetKeyDown("o"))
        {
            stamina = 0;
        }
    }

    private void UseHeldItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory) return;
        UseHotBarItem();
    }

    private void OnInteractWithItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory) return;
        if(ltCanPress == true) { StructureInteractionWithItem(); ltCanPress = false; }
        else ltCanPress = true;
    }


    private void InteractWithoutItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory) return;
        InteractWithObject();
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
        InventoryItemData item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;

        //Is it a Tool item?
        ToolItem t_item = item as ToolItem;
        if (t_item) 
        {
            t_item.SecondaryUse(mainCam.transform);
            return;
        }

        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;


        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach + 8, interactionLayers))
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
                //Debug.Log("Interacted with item");
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
                //Debug.Log("Opened Inventory of Interactable Object");
                return;
            }

            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.StructureInteraction();
                //Debug.Log("Interacting with a structure");
                return;
            }
        }

        //if nothing, progress dialogue
        if(DialogueController.Instance) DialogueController.Instance.AdvanceDialogue();
        
    }


    void UseHotBarItem()
    {
       Debug.Log("UsingHandItem");
        InventoryItemData item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        if(item == null) return;

        //Is it a Tool item?
        ToolItem t_item = item as ToolItem;
        if (t_item)
        {
            t_item.PrimaryUse(mainCam.transform);
            return;
        }
        

        //Is it a placeable item?
        PlaceableItem p_item = item as PlaceableItem;
        if (p_item)
        {
            p_item.PlaceStructure(mainCam.transform);
            playerInventoryHolder.UpdateInventory();
            return;
        }

        if(item.staminaValue > 0 && stamina < maxStamina)
        {
            //eat it
            StaminaChange(item.staminaValue);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
            return;
        }
    }

    public void StaminaChange(float amount)
    {
        stamina += amount;
        if(amount < -5) playerEffects.PlayerDamage();
    }

    public IEnumerator ToolUse(ToolBehavior tool, float time, float coolDown)
    {
        rb.velocity = new Vector3(0,0,0);
        if(toolCooldown) yield break;
        toolCooldown = true;
        yield return new WaitForSeconds(time);
        tool.ItemUsed();
        yield return new WaitForSeconds(coolDown - time);
        toolCooldown = false;
        //use a bool that says i am done swinging to avoid tool overlap
    }

    void DisplayHologramCheck()
    {
        InventoryItemData item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        if(!item) return;
        PlaceableItem p_item = item as PlaceableItem;
        if(!p_item || !p_item.hologramPrefab) return;
        p_item.DisplayHologram(mainCam.transform);

        if(Input.GetKeyDown("r"))
        {
            p_item.RotateHologram();
        }
    }

    IEnumerator GameOver()
    {
        //work on a transition, maybe with the vignette
        yield return new WaitForSeconds(1.5f);
        TimeManager.Instance.GameOver();
        if(currentMoney > 0) currentMoney = currentMoney/2;
        transform.position = TimeManager.Instance.playerRespawn.position;
        gameOver = false;
        stamina = 100;

    }
    
}
