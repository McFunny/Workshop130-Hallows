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
    public static event AttackedCreature OnPlayerAttack; //Unity Event that will tell listeners when the player physically attacks an enemy

    public delegate void UseTool();
    public static event UseTool OnToolUse; //Unity Event that will tell listeners when the player TRIES to use primary of a tool
    
    public delegate void TakenDamage(float damage);
    public static event TakenDamage OnPlayerDamaged; //Unity Event that will tell listeners when the player physically attacks an enemy

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

    ///Stamina and Water///
    public float stamina = 200;
    [HideInInspector] public readonly float maxStamina = 200;
    public float fatigue = 0;
    [HideInInspector] public readonly float maxFatigue = 150;
    public float regenRate = 4; //Every .5 seconds
    public float targetRegen = 0; // Target stamin value at end of regen

    bool sentLowStaminaMessage = false;
    [HideInInspector] public bool overrideDamagePulse; //Makes the damage effects not happen
    public bool invincible = false;

    public float waterHeld = 10; //for watering can //USED TO BE 15, TRYING 10
    [HideInInspector] public float maxWaterHeld = 10;
    //////

    public bool torchLit = false; //For the tool item
    public bool pyreflyLit = false; //For the tool item
    //public bool droppedKukri = false; //For when the player has thrown their knife
    public bool lostKukri = false; //For when the player no longer has their knife
    public bool isParrying, parrySuccess;

    private float reach = 8;

    public List<StatusEffect> currentEffects = new List<StatusEffect>();


    public LayerMask interactionLayers;
    private bool ltCanPress = false;
    private bool interactWithEmptyHand = false;

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
        UpdateSettings();
        StartCoroutine(WakeUp());
        StartCoroutine(RegenRoutine());
    }

    private void OnEnable()
    {
        //LEFT CLICK USES THE ITEM CURRENTLY IN THE HAND
        controlManager.useHeldItem.action.started += UseHeldItem; 
        //RIGHT CLICK USES AN ITEM ON A STRUCTURE, EX: PLANTING A SEED IN FARMLAND
        controlManager.interactWithItem.action.started += OnInteractWithItem;
        //SPACE INTERACTS WITH A STRUCTURE WITHOUT USING AN ITEM, EX: HARVESTING A CROP
        controlManager.interactWithoutItem.action.started += InteractWithoutItem;
        SettingsValueManager.OnSettingsChanged += UpdateSettings;
    }

    private void OnDisable()
    {
        controlManager.useHeldItem.action.started -= UseHeldItem;
        controlManager.interactWithItem.action.started -= OnInteractWithItem;
        controlManager.interactWithoutItem.action.started -= InteractWithoutItem;
        SettingsValueManager.OnSettingsChanged -= UpdateSettings;
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

        if ((StructureManager.Instance.enableCheats || playerUpgrades.gainedWaterPack) && controlManager.waterJet.action.WasPressedThisFrame() && !toolCooldown 
        && PlayerMovement.restrictMovementTokens == 0) StartCoroutine(WaterPropulsion());

        //if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory) return;


    }

    private void UseHeldItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 /*|| toolCooldown*/ || PlayerMovement.accessingInventory || PlayerMovement.isCodexOpen) return;
        UseHotBarItem();
    }

    private void OnInteractWithItem(InputAction.CallbackContext obj)
    {
        if(PlayerMovement.restrictMovementTokens > 0 /*|| toolCooldown */|| PlayerMovement.accessingInventory|| PlayerMovement.isCodexOpen) return;
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

        if(item == null)
        {
            //return; // Replace this with the setting bool check
            
            /////THIS IS TO TEST HAVING LEFT CLICK FUNCTION AS SPACE IF HAND IS EMPTY///////////
            if(PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory|| PlayerMovement.isCodexOpen)
            {
                if(DialogueController.Instance) DialogueController.Instance.AdvanceDialogue();
                return;
            }
            InteractWithObject();
            ////////////////////////////////////////////////////////////////////////////////////
            return;
        }

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
            if (structure != null && structure.Interactable())
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
            if (structure != null && structure.Interactable())
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
        if(item == null)
        {
            if (interactWithEmptyHand && !ControlManager.isController)
            {
                /////THIS IS TO TEST HAVING LEFT CLICK FUNCTION AS SPACE IF HAND IS EMPTY///////////
                if (PlayerMovement.restrictMovementTokens > 0 || toolCooldown || PlayerMovement.accessingInventory || PlayerMovement.isCodexOpen)
                {
                    if (DialogueController.Instance) DialogueController.Instance.AdvanceDialogue();
                    return;
                }
                InteractWithObject();
                ////////////////////////////////////////////////////////////////////////////////////
            }  
            return;
        }

        //Is it a Tool item?
        ToolItem t_item = item as ToolItem;
        if (t_item)
        {
            OnToolUse?.Invoke();
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
            //StaminaChange(item.staminaValue);
            EatFood(item.staminaValue);
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

    public void StaminaChange(float amount, bool ignoreArmor = false)
    {
        if (DialogueController.Instance.IsTalking() && amount < 0 || Tutorial.Instance || invincible || isTripped)
        {
            print("Damage negated! Stamina is : " + stamina);
            overrideDamagePulse = false;
            return;
        }
        if(stamina + amount <= 50 && stamina > 50 && amount >= -4 && amount < 0) //To prevent tools from putting player below 50
        {
            print("Damage negated to not go under threshold");
            overrideDamagePulse = false;
            return;
        }

        if(MainMenuScript.currentFileMode == FileMode.Cozy && amount < 0) amount *= 0.75f; // Damage refuction from Cosy mode

        if (StatusEffectManager.Instance.FindStatusOnPlayer(StatusEffectName.Dare) && amount <= -5) amount *= 1.5f; //Damage Modifier from Dare

        //if(amount > 6) fatigue += Mathf.Round(amount * 0.1f);

        if(amount > 0) playerEffects.PlayClip(playerEffects.playerHeal, 1.3f); //Play Heal Effects
        
        if(repairMinigame.IsMinigameActive()) repairMinigame.ForceEndMinigame();

        if(amount <= -5 && !overrideDamagePulse) 
        {
            if(!ignoreArmor) //Apply Damage Reduction from Trinkets
            {
                amount = TrinketInventoryHandler.Instance.ApplyTrinketDamageModifiers(amount);
                if(amount > -5) amount = -5;
            }

            if(waterHeld > 0 && TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.WaterGuard)) //Reduce water instead of damage
            {
                int waterLoss = 0;
                while(amount <= -5 && waterLoss < waterHeld)
                {
                    amount += 5;
                    if(amount > -5) amount = 0;
                    ++waterLoss;
                }

                if(waterLoss > 0)
                {
                    WaterChange(-waterLoss);
                    TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.WaterGuard, waterLoss);
                    playerEffects.PlayerDamage();
                }
            }

            if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.MimicNose) && UnityEngine.Random.Range(0, 10) == 1) ApplyStatusEffect(StatusDatabase.Instance.GetStatus(StatusEffectName.MimicScent), 10);

            if(stamina + amount <= 0 && TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.RoachRegen)) //Prevents death if roach trinket is equipped
            {
                amount = 0;
                TrinketInventoryHandler.Instance.ForceBreakTrinket(TrinketKey.RoachRegen);
            }
        }

        stamina += Mathf.Round(amount); //Apply the new stamina

        if(amount <= -5 && !overrideDamagePulse)
        {
            playerEffects.PlayerDamage();
            ScreenSplatSpawner.Instance.SpawnSplats(SplatType.Blood, new Color(1,1,1,0.4f), Mathf.Clamp(-amount / 3, 1, 8));
            if(stamina <= 0 || MainMenuScript.currentFileMode != FileMode.Cozy) targetRegen = 0;

            StartCoroutine(DamageSlowDown());

            //TrinketInventoryHandler.Instance.DamageArmorTrinkets(-amount);
        }

        if(amount <= -10) OnPlayerDamaged?.Invoke(amount);

        if(!sentLowStaminaMessage && stamina <= 50)
        {
            sentLowStaminaMessage = true;
            PopupHandler.Instance.AddToQueue(lowStaminaWarning);
        }
        else if(stamina > 50) sentLowStaminaMessage = false;

        overrideDamagePulse = false;
    }

    public void StaminaChange(float amount, Vector3 sourcePos) //Mostly just for parrying
    {
        //Dont forget to check the Dot
        if(isParrying && amount <= 5)
        {
            Vector3 dir = (sourcePos - transform.position).normalized;
            float dot = Vector3.Dot(dir, mainCam.transform.forward);

            if(dot >= 0.65f)
            {
                isParrying = false;
                parrySuccess = true;
                if(HandItemManager.Instance.parryParticles) HandItemManager.Instance.parryParticles.Play();
                TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.Parry);
                return;
            }
        }


        StaminaChange(amount);
    }

    public void WaterChange(float amount)
    {
        if(amount + waterHeld > maxWaterHeld) amount -= amount + waterHeld - maxWaterHeld;
        else if(waterHeld + amount < 0) amount = waterHeld;

        if(amount == 0) return;

        waterHeld += amount;

        if(amount > 0) playerEffects.PlayClip(playerEffects.waterGain, 1.3f);


        //if using hareflask trinket, subtract 1 for each water gained over maxWater - 5
        if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.WaterFlask) && waterHeld > (maxWaterHeld - 5))
        {
            float tempValue = waterHeld;
            float x = 0;
            while(tempValue > (maxWaterHeld - 5) && x < amount)
            {
                tempValue--;
                x++;
                //Damage Trinket
                TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.WaterFlask);
            }
        }
    }

    public void EatFood(float staminaGain)
    {
        if(staminaGain <= 10)
        {
            StaminaChange(10);
            return;
        }
        
        StaminaChange(Mathf.Floor(staminaGain/4));
        if(targetRegen == 0) targetRegen = stamina;
        targetRegen += Mathf.Floor(staminaGain * .75f);
        if(targetRegen > maxStamina) targetRegen = maxStamina;
    }

    IEnumerator RegenRoutine()
    {
        bool skipNext = true;
        float currentRegenRate;
        while(true)
        {
            yield return new WaitForSeconds(1f);

            if(stamina < 50 && targetRegen == 0 && TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.RoachRegen))
            {
                StaminaChange(1);
                TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.RoachRegen);
                continue;
            }

            if(targetRegen <= stamina || stamina <= 0) 
            {
                targetRegen = 0;
                skipNext = true;
                continue;
            }

            if(skipNext) //To make sure heal isnt always immediate
            {
                skipNext = false;
                continue;
            }

            currentRegenRate = regenRate;
            
            if(TrinketInventoryHandler.Instance.CheckForTrinket(TrinketKey.TickRegen))
            {
                currentRegenRate *= 2;
                TrinketInventoryHandler.Instance.ApplyTrinketDamage(TrinketKey.TickRegen);
            }

            if(stamina + currentRegenRate > targetRegen) currentRegenRate = targetRegen - stamina;

            StaminaChange(currentRegenRate);
        }
    }

    IEnumerator DamageSlowDown()
    {
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(gameObject, 0.6f, "Damage", false));
        yield return new WaitForSeconds(0.5f);
        PlayerMovement.Instance.RemoveSpeedMod("Damage");
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
        //if(toolCooldown) yield break;
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
            if (structure != null && structure.Interactable())
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

        if(MainMenuScript.currentFileMode == FileMode.Survival)
        {
            NightSpawningManager.Instance.GameOver();
            TimeManager.Instance.stopTime = true;
            yield return new WaitForSeconds(1f);
            SurvivalModeManager.Instance.StartCoroutine(SurvivalModeManager.Instance.GameOver(true));
            yield break;
        }

        NightSpawningManager.Instance.GameOver();
        print("Night GameOver Complete");
        StructureManager.Instance.GameOver();
        print("Structure GameOver Complete");
        WildernessManager.Instance.GameOver();
        print("Wilderness GameOver Complete");

        TownGate.Instance.Transition(PlayerLocation.InFarm);

        stamina = 150;
        if(currentMoney > 0 && MainMenuScript.currentFileMode != FileMode.Cozy) currentMoney = currentMoney - (currentMoney/5); //I have no idea if this will work
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
        if (addKnockback) GetComponent<PlayerMovement>().ApplyForceToPlayer(2000, PlayerInteraction.Instance.mainCam.transform.TransformDirection(-Vector3.forward));
        cameraPos.DOMoveY(cameraPos.position.y + 1, 0.15f); //Move up

        if (addKnockback) PlayerCam.Instance.NewObjectOfInterest(trippedFocalPoint.position);
        yield return new WaitForSeconds(.15f);

        cameraPos.DOMoveY(cameraPos.position.y - 2.5f, 0.25f); //Move Down
        yield return new WaitForSeconds(.25f);
        playerEffects.PlayClip(playerEffects.trip);
        playerEffects.CallScreenShake(0.7f);
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        ScreenSplatSpawner.Instance.SpawnSplats(SplatType.Dirt, new Color(1,1,1,0.4f), 5);
        yield return new WaitForSeconds(.50f);
        PlayerMovement.limitMaxVelocity = true;
        if (stamina <= 0) yield return new WaitForSeconds(3f); //Death extra time

        cameraPos.DOMoveY(cameraPos.position.y + 1.5f, 0.75f); //Stand back up
        yield return new WaitForSeconds(0.75f);
        PlayerCam.Instance.ClearObjectOfInterest();
        isTripped = false;
        PlayerMovement.restrictMovementTokens--;
    }

    public void ToggleTrip(bool trip)
    {
        isTripped = trip;
    }

    public bool TripCheck()
    {
        return isTripped;
    }
    
    private void UpdateSettings()
    {
        var prefs = PlayerPrefs.GetInt("EmptyHand", 0);

        if (prefs == 0) interactWithEmptyHand = false;
        else interactWithEmptyHand = true;
    }

    public IEnumerator ParryRoutine()
    {
        isParrying = true;
        parrySuccess = false;
        ToolUseToggle(true);
        float timeElapsed = 0;
        float maxTime = 0.4f;

        while(timeElapsed < maxTime && !parrySuccess)
        {
            yield return new WaitForSeconds(0.1f);
            timeElapsed += 0.1f;
        }

        isParrying = false;
        if(parrySuccess)
        {
            ToolUseToggle(false);
            invincible = true;
            yield return new WaitForSeconds(0.4f);
            invincible = false;
            yield return new WaitForSeconds(0.4f);
        }
        else
        {
            yield return new WaitForSeconds(1.05f);
            ToolUseToggle(false);
        }
        parrySuccess = false;
    }


}
