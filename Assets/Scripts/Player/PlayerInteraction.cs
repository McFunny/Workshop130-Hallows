using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;
using UnityEngine.Events;

public class PlayerInteraction : MonoBehaviour
{
    public delegate void AttackedCreature(CreatureBehaviorScript c);
    public static event AttackedCreature OnPlayerAttack; //Unity Event that will listeners when the player physically attacks an enemy

    public Camera mainCam;

    public Transform playerFeet, cameraPos, trippedFocalPoint;

    public PlayerInventoryHolder playerInventoryHolder { get; private set; }

    public PlayerUpgrades playerUpgrades;

    PlayerEffectsHandler playerEffects;

    [HideInInspector] public ControlManager controlManager;

    [HideInInspector] public Rigidbody rb;

    public bool isInteracting { get; private set; } //Obsolete I think
    public bool toolCooldown;
    bool itemUseCooldown, isTripped;

    public static PlayerInteraction Instance;

    public int currentMoney;
    public int totalMoneyEarned;
    public int daysSinceDeath = 0;
    public delegate void PlayerDeathEvent();
    public static event PlayerDeathEvent OnPlayerDeath;

    public float stamina = 200;
    [HideInInspector] public readonly float maxStamina = 200;
    public float fatigue = 0;
    [HideInInspector] public readonly float maxFatigue = 150;
    bool sentLowStaminaMessage = false;
    public bool invincible = false;

    public float waterHeld = 10; //for watering can //USED TO BE 15, TRYING 10
    [HideInInspector] public float maxWaterHeld = 10;

    public bool torchLit = false; //For the tool item
    public bool pyreflyLit = false; //For the tool item

    private float reach = 8;

    public List<StatusEffect> currentEffects = new List<StatusEffect>();


    public LayerMask interactionLayers;
    private bool ltCanPress = false;

    [HideInInspector] public bool gameOver;

    public PopupScript lowStaminaWarning;

    StructureBehaviorScript lastSeenStruct;
    IInteractable lastSeenInteractable;
    private RepairMinigame repairMinigame;
    public delegate void FoodConsumedEvent(InventoryItemData item);
    public static event FoodConsumedEvent onFoodConsumed;



    void Awake()
    {
        controlManager = FindFirstObjectByType<ControlManager>();
        repairMinigame = FindFirstObjectByType<RepairMinigame>();
        stamina = maxStamina;
        waterHeld = maxWaterHeld;
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
        playerInventoryHolder = GetComponent<PlayerInventoryHolder>();
        playerEffects = GetComponent<PlayerEffectsHandler>();
        rb = GetComponent<Rigidbody>();

        StartCoroutine(WakeUp());
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
        if(fatigue > maxFatigue) fatigue = maxFatigue;

        //if(stamina > maxStamina - fatigue) stamina = maxStamina - fatigue;

        DisplayHologramCheck();

        DisplayHighlightCheck();

        if(stamina <= 0 && !gameOver)
        {
            gameOver = true;
            StartCoroutine(GameOver());
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            if (Input.GetKeyDown(KeyCode.L) && StructureManager.Instance.enableCheats)
            {
                currentMoney += 200;
                totalMoneyEarned += 200;
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (Input.GetKeyDown(KeyCode.O) && StructureManager.Instance.enableCheats)
            {
                print(InputManager.isCharging);
                InputManager.isCharging = false;
            }
        }

        if (StructureManager.Instance.enableCheats && Input.GetKeyDown(KeyCode.Y) && !toolCooldown && PlayerMovement.restrictMovementTokens == 0) StartCoroutine(WaterPropulsion());

        //if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory) return;


    }

    private void UseHeldItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory || PlayerMovement.isCodexOpen) return;
        UseHotBarItem();
    }

    private void OnInteractWithItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory|| PlayerMovement.isCodexOpen) return;
        if(!ControlManager.isController) StructureInteractionWithItem();
        else
        {
            if(ltCanPress == true)
            { 
                StructureInteractionWithItem();
                ltCanPress = false; 
            }
            else ltCanPress = true;
        }
    }


    private void InteractWithoutItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory|| PlayerMovement.isCodexOpen)
        {
            if(DialogueController.Instance) DialogueController.Instance.AdvanceDialogue();
            return;
        }
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
        else interactable.Interact(this, out bool interactSuccessful);
        isInteracting = false;
    }

    void EndInteraction()
    {
        isInteracting = false;
    }

    void StructureInteractionWithItem()
    {
        InventoryItemData item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;

        if(item == null) return;

        //Is it a Tool item?
        ToolItem t_item = item as ToolItem;
        if (t_item) 
        {
            t_item.SecondaryUse(mainCam.transform);
            return;
        }

        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;


        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach + 4, interactionLayers))
        {
            if(hit.collider.gameObject.layer == 19) return;
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                StartInteractionWithItem(interactable); //Interacts with chest and npc's. I should eventually make this compatable with the structures I made - Cam
                return;
            }

            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.ItemInteraction(item);
                //Debug.Log("Interacted with item");
                return;
            }

            var critter = hit.collider.GetComponentInParent<ICritter>();
            if (critter != null)
            {
                critter.InteractWithItem(this, out bool interactSuccessful, item);
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
            if(hit.collider.gameObject.layer == 19) return;
            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                StartInteraction(interactable);


                if(hit.collider.GetComponent<ChestInventory>() != null) PlayerMovement.accessingInventory = true;  // Needs to check if opening a chest, else this should not be called
                //Debug.Log("Opened Inventory of Interactable Object");
                return;
            }

            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.StructureInteraction();
                //Debug.Log("Interacting with a structure");
                return;
            }

            var critter = hit.collider.GetComponentInParent<ICritter>();
            if (critter != null)
            {
                critter.Interact(this, out bool interactSuccessful);
                return;
            }
        }

        //if nothing, progress dialogue
        if(DialogueController.Instance) DialogueController.Instance.AdvanceDialogue();
        
    }


    void UseHotBarItem()
    {
        //Debug.Log("UsingHandItem");
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

        if(itemUseCooldown) return;

        bool itemUsed = false;

        if(item.staminaValue > 0 && stamina < maxStamina)
        {
            //eat it
            StaminaChange(item.staminaValue);
            itemUsed = true;
        }

        if(item.gainedEffects.Count > 0)
        {
            foreach(StatusEffect s in item.gainedEffects)
            {
                ApplyStatusEffect(s.effect, s.remainingDuration);
            }

            itemUsed = true;
        }

        if(item.itemBehavior)
        {
            item.itemBehavior.UseItem(out bool consumedOnUse);
            if(consumedOnUse) itemUsed = true;
            else 
            {
                if(item.useSound) playerEffects.PlayClip(item.useSound);
                if(item.useCooldown > 0) StartCoroutine(ItemUseCooldown(item.useCooldown));
                return;
            }
        }

        if(itemUsed)
        {
            if(item.useCooldown > 0) StartCoroutine(ItemUseCooldown(item.useCooldown));
            onFoodConsumed?.Invoke(item);

            if (item.useSound) playerEffects.PlayClip(item.useSound);
            else if (item.staminaValue > 0) playerEffects.PlayClip(playerEffects.itemEat);
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            playerInventoryHolder.UpdateInventory();
        }
    }

    public void GainMints(int amount, bool countForTotal)
    {
        currentMoney += amount;
        if (countForTotal) totalMoneyEarned += amount;
    }

    public void StaminaChange(float amount)
    {
        if (DialogueController.Instance.IsTalking() && amount < 0 || Tutorial.Instance || invincible || isTripped)
        {
            print("Damage negated! Stamina is : " + stamina);
            return;
        }
        if(stamina + amount <= 50 && stamina > 50 && amount >= -4 && amount < 0)
        {
            print("Damage negated to not go under threshold");
            return;
        }

        if(MainMenuScript.currentFileMode == FileMode.Cozy && amount < 0) amount *= 0.75f;

        if (StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Dare) && amount < 0) amount *= 1.5f;

        //if(amount > 6) fatigue += Mathf.Round(amount * 0.1f);
        
        if(repairMinigame.IsMinigameActive())
        {
            repairMinigame.EndMinigame();
        }

        stamina +=  Mathf.Round(amount);
        if(amount <= -5) playerEffects.PlayerDamage();
        if(!sentLowStaminaMessage && stamina <= 50)
        {
            sentLowStaminaMessage = true;
            PopupHandler.Instance.AddToQueue(lowStaminaWarning);
        }
        else if(stamina > 50) sentLowStaminaMessage = false;
    }

    public void ApplyStatusEffect(StatusEffectObject status, int duration)
    {
        if(currentEffects.Count == 0)
        {
            currentEffects.Add(new StatusEffect(status, duration));
            currentEffects[currentEffects.Count - 1].effect.OnEffectApplied();
            GameObject vfx = StatusEffectManager.Instance.GrabStatusVFX(status.name);
            if(vfx == null) return;
            vfx.GetComponent<VFXStatusObject>().followTransform = playerFeet;
            //vfx.transform.position = new Vector3(playerFeet.position.x, playerFeet.position.y + 0.5f, playerFeet.position.z);
            //vfx.transform.parent = playerFeet;
            return;
        }
        for(int x = 0; x < currentEffects.Count; x++)
        {
            //do the effects referencing the scriptable object here
            if(currentEffects[x].effect.name == status.name)
            {
                if(currentEffects[x].remainingDuration < duration) currentEffects[x].remainingDuration = duration;
                return;
            }
        }
    }

    public IEnumerator ToolUse(ToolBehavior tool, float time, float coolDown)
    {
        if(time > 0) rb.velocity = new Vector3(0,0,0);
        if(toolCooldown) yield break;
        toolCooldown = true;
        yield return new WaitForSeconds(time);
        tool.ItemUsed();
        yield return new WaitForSeconds(coolDown - time);
        toolCooldown = false;
    }

    public IEnumerator ToolUseWithoutMovementReset(ToolBehavior tool, float time, float coolDown)
    {
        //if(time > 0) rb.velocity = new Vector3(0,0,0);
        if(toolCooldown) yield break;
        toolCooldown = true;
        yield return new WaitForSeconds(time);
        tool.ItemUsed();
        yield return new WaitForSeconds(coolDown - time);
        toolCooldown = false;
    }

    public void ToolUseToggle(bool x)
    {
        toolCooldown = x;
    }

    void DisplayHologramCheck()
    {
        if(!HotbarDisplay.currentSlot) return;
        InventoryItemData item = HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData;
        if(!item) return;
        PlaceableItem p_item = item as PlaceableItem;
        if(!p_item || !p_item.hologramPrefab) return;
        p_item.DisplayHologram(mainCam.transform);

        if(controlManager.rotateStructure.action.WasPressedThisFrame())
        {
            p_item.RotateHologram();
        }
    }

    void DisplayHighlightCheck()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, interactionLayers))
        {
            if(hit.collider.gameObject.layer == 19) return;
            var structure = hit.collider.GetComponentInParent<StructureBehaviorScript>();
            if (structure != null)
            {
                if(structure == lastSeenStruct) return;
                structure.ToggleHighlight(true);
                if(lastSeenStruct) lastSeenStruct.ToggleHighlight(false);
                if(lastSeenInteractable != null) lastSeenInteractable.ToggleHighlight(false);
                lastSeenStruct = structure;
                return;
            }

            var interactable = hit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                if(interactable == lastSeenInteractable) return;
                interactable.ToggleHighlight(true);
                if(lastSeenStruct) lastSeenStruct.ToggleHighlight(false);
                if(lastSeenInteractable != null) lastSeenInteractable.ToggleHighlight(false);
                lastSeenInteractable = interactable;
                return;
            }
        }
        if(lastSeenStruct)
        {
            lastSeenStruct.ToggleHighlight(false);
            lastSeenStruct = null;
        }
        if(lastSeenInteractable != null)
        {
            lastSeenInteractable.ToggleHighlight(false);
            lastSeenInteractable = null;
        }
    }

    IEnumerator GameOver()
    {
        //maybe pause time? also make sure no issues arise when dying while talking to someone

        //Remove Status Effects
        foreach(StatusEffect e in currentEffects)
        {
            e.remainingDuration = 0;
        }
        PlayerTrip();

        PlayerMovement.restrictMovementTokens++;
        FadeScreen.coverScreen = true;
        daysSinceDeath = -1;
        InvokePlayerDeathEvent();
        AmbientAudioManager.Instance.ChangeMusic();
        yield return new WaitForSeconds(0.5f);
        playerEffects.PlayClip(playerEffects.playerDie, 0.8f);
        yield return new WaitForSeconds(1.5f);
        NightSpawningManager.Instance.GameOver();
        print("Night GameOver Complete");
        StructureManager.Instance.GameOver();
        print("Structure GameOver Complete");
        WildernessManager.Instance.GameOver();
        print("Wilderness GameOver Complete");

        TownGate.Instance.Transition(PlayerLocation.InFarm);

        stamina = 100;
        if(currentMoney > 0 && MainMenuScript.currentFileMode != FileMode.Cozy) currentMoney = (currentMoney/5) * 4; //I have no idea if this will work
        TimeManager.Instance.GameOver(); //Has to be last, this is where it saves
        print("Time GameOver Complete");

        yield return new WaitForSeconds(1f);
        print("GameOver Complete");
        PlayerMovement.restrictMovementTokens--;
        PlayerMovement.ignoreMovementInputs = false;
        FadeScreen.coverScreen = false;
        transform.position = TimeManager.Instance.playerRespawn.position;
        gameOver = false;
        //StartCoroutine(WakeUp());

    }

    IEnumerator ItemUseCooldown(float duration)
    {
        itemUseCooldown = true;
        yield return new WaitForSeconds(duration);
        itemUseCooldown = false;
    }

    IEnumerator WakeUp() //Cant get this working smoothly.
    {
        print("Waking up");
        PlayerMovement.restrictMovementTokens++;
        //yield return new WaitForSeconds(1f);
        PlayerCam.Instance.NewObjectOfInterest(TimeManager.Instance.respawnFocus.position);
        int i = 0;
        while(i < 10)
        {
            yield return new WaitForSeconds(0.2f);
            TimeManager.Instance.respawnFocus.position = new Vector3(TimeManager.Instance.respawnFocus.position.x, TimeManager.Instance.respawnFocus.position.y + 0.4f, TimeManager.Instance.respawnFocus.position.z);
            PlayerCam.Instance.NewObjectOfInterest(TimeManager.Instance.respawnFocus.position);
            i++;
        }
        //yield return new WaitForSeconds(2);
        PlayerMovement.restrictMovementTokens--;
        PlayerCam.Instance.ClearObjectOfInterest();
        print("Done");
        while(i > 0)
        {
            yield return new WaitForSeconds(0.2f);
            TimeManager.Instance.respawnFocus.position = new Vector3(TimeManager.Instance.respawnFocus.position.x, TimeManager.Instance.respawnFocus.position.y - 0.4f, TimeManager.Instance.respawnFocus.position.z);
            i++;
        }
    }

    public IEnumerator WaterPropulsion()
    {
        if(toolCooldown || waterHeld < 1) yield break;
        waterHeld -= 1;
        playerEffects.PlayClip(playerEffects.waterJet);
        toolCooldown = true;
        PlayerMovement.limitMaxVelocity = false;
        PlayerMovement.ignoreMovementInputs = true;
        GetComponent<PlayerMovement>().ApplyForceToPlayer(3000, PlayerInteraction.Instance.mainCam.transform.TransformDirection(Vector3.forward));
        yield return new WaitForSeconds(0.2f);
        PlayerMovement.limitMaxVelocity = true;
        PlayerMovement.ignoreMovementInputs = false;
        yield return new WaitForSeconds(0.4f);
        toolCooldown = false;
    }

    public void InvokePlayerDeathEvent()
    {
        OnPlayerDeath?.Invoke();
    }

    public void InvokeEnemyHitEvent(CreatureBehaviorScript c)
    {
        OnPlayerAttack?.Invoke(c);
    }

    public void ShakeScreen(float intensity)
    {
        playerEffects.damageImpulse.GenerateImpulseWithForce(intensity);
    }

    public void PlayerTrip()
    {
        if(PlayerMovement.restrictMovementTokens > 0 || isTripped) return;
        StartCoroutine(PlayerTripRoutine(true));
    }

    public void PlayerTripNoKnockback()
    {
        if(PlayerMovement.restrictMovementTokens > 0 || isTripped) return;
        StartCoroutine(PlayerTripRoutine(false));
    }

    IEnumerator PlayerTripRoutine(bool addKnockback) //for recoiling purposes
    {
        isTripped = true;
        PlayerMovement.restrictMovementTokens++;
        PlayerMovement.limitMaxVelocity = false;
        if(addKnockback) GetComponent<PlayerMovement>().ApplyForceToPlayer(2000, PlayerInteraction.Instance.mainCam.transform.TransformDirection(-Vector3.forward));
        cameraPos.DOMoveY(cameraPos.position.y + 1, 0.15f); //Move up

        if(addKnockback) PlayerCam.Instance.NewObjectOfInterest(trippedFocalPoint.position);
        yield return new WaitForSeconds(.15f);

        cameraPos.DOMoveY(cameraPos.position.y - 2.5f, 0.25f); //Move Down
        yield return new WaitForSeconds(.25f);
        playerEffects.PlayClip(playerEffects.trip);
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        yield return new WaitForSeconds(.50f);
        PlayerMovement.limitMaxVelocity = true;
        if(stamina <= 0) yield return new WaitForSeconds(3f); //Death extra time

        cameraPos.DOMoveY(cameraPos.position.y + 1.5f, 0.75f); //Stand back up
        yield return new WaitForSeconds(0.75f);
        PlayerCam.Instance.ClearObjectOfInterest();
        isTripped = false;
        PlayerMovement.restrictMovementTokens--;
    }


}
