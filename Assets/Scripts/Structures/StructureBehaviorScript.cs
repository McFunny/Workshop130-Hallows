using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;

public class StructureBehaviorScript : MonoBehaviour
{
    //This is the base class that ALL structures should derive from

    public delegate void StructuresUpdated();
    public static event StructuresUpdated OnStructuresUpdated; //Unity Event that will notify enemies when structures are updated

    public delegate void StructureDestroyed(StructureObject structData, Vector3 pos);
    public static event StructureDestroyed OnStructureDestroyed; //Unity Event that will listeners when a specific structure is destroyed

    public delegate void Damaged();
    [HideInInspector] public event Damaged OnDamage;

    public delegate void DamagedWithValue(float damage);
    [HideInInspector] public event DamagedWithValue OnDamageWithValue;

    [Header("Structure Stats")]

    public StructureObject structData;
    public InventoryItemData itemForm;

    public float health = 5;
    public float maxHealth = 5;

    public float wealthValue = 0; //dictates how hard a night could be 

    public float salvageChance = 0; //number out of 100 that dictates if it collapses into a pile or not

    [Tooltip("Can this structure be destroyed by lowering its health?")]
    public bool destructable = true;
    
    [Tooltip("Does this structure burn?")]
    public bool flammable = true;
    public bool onFire = false;
    [Tooltip("Does this structure impede movement? If yes, creatures will attack this if nearby and facing it")]
    public bool isObstacle = true;
    public bool repairableWithGlue;

    public bool absentFromGrid = false; //if true, this object wont count as all structs, nor will it interact with tiles, allowing free placement.
    [HideInInspector] public bool absentFromFarmGrid = false; //if true, this object should be ignored by creatures that target structures

    public Transform focalPoint; //for when the camera needs to focus on the object
    public Transform particleCenter; //for particles

    //Save Data
    [HideInInspector] public List<InventoryItemData> savedItems = new List<InventoryItemData>(); //For saving items stored in a structure, for example meat on a drying rack, seeds in a turret
    [HideInInspector] public int saveInt1, saveInt2, saveInt3;
    [HideInInspector] public float saveFloat1, saveFloat2, saveFloat3;
    [HideInInspector] public string saveString1, saveString2, saveString3;
    [HideInInspector] public bool saveBool1;

    public GameObject damageParticlesObject;
    List<ParticleSystem> damageParticles = new List<ParticleSystem>();
    //public DestructionType destructionType;
    public GameObject gibs;

    public List<FireFearTrigger> nearbyFires = new List<FireFearTrigger>(); //to track if this structure is currently illuminated
    
    [Header("Highlights")]
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    [HideInInspector] bool highlightEnabled;
    public bool canShowHighlight = true;

    [HideInInspector] public StructureAudioHandler audioHandler;
    //[HideInInspector] public AudioSource source;

    [HideInInspector] public bool clearTileOnDestroy = true;
    bool forcePile = false;
    [HideInInspector] public bool muteSound = false;

    [Tooltip("Specific UI for this structure, if it has any")]
    public GameObject structureUI; 
    public StructureUIValues structureUIVariables;

    Coroutine highlightCoroutine;

    //NavMeshSurface navSurface;

    //[Header("Structure Specific")]

    //Once we get structure specific UI to see health, then we can add repairability to structures so players can know if they can dig it up safely


    public void Awake()
    {
        OnStructuresUpdated?.Invoke();
        //navSurface = FindObjectOfType<NavMeshSurface>();

        //navSurface.UpdateNavMesh(navSurface.navMeshData);
        audioHandler = GetComponent<StructureAudioHandler>();

        TimeManager.OnHourlyUpdate += HourPassed;
        foreach(GameObject thing in highlight) thing.SetActive(false);

        if(structureUI) structureUI.SetActive(false);

        if(damageParticlesObject)
        {
            foreach(Transform child in damageParticlesObject.transform)
            {
                damageParticles.Add(child.GetComponent<ParticleSystem>());
            }
        }

    }

    public void Start() //make sure absent from grid is checked if not on farm
    {
        if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Any) == false) absentFromGrid = true;
        else if(StructureManager.Instance.ValidateGridType(transform.position, GridType.Farm) == false) absentFromFarmGrid = true;
        
        if (absentFromGrid) return;
        StructureManager.Instance.allStructs.Add(this);

        if(structData)
        {
            if(structData.gridSize == GridSize.OneByOne)
            {
                StructureManager.Instance.SetTile(transform.position);
            }
            if(structData.gridSize == GridSize.TwoByTwo)
            {
                StructureManager.Instance.SetLargeTile(transform.position);
            }
            if(structData.gridSize == GridSize.OneByTwo)
            {
                StructureManager.Instance.SetOneByTwoTile(transform.position);
            }
            if(structData.gridSize == GridSize.ThreeByThree)
            {
                StructureManager.Instance.SetExtraLargeTile(transform.position);
            }
        }
    }

    public void Update()
    {
        if(health <= 0 && destructable) Destroy(this.gameObject);
    }

    public virtual void StructureInteraction(){}
    public virtual void ItemInteraction(InventoryItemData item){}
    public virtual void ToolInteraction(ToolType tool, out bool success)
    {
        success = false;
    }
    public virtual void HourPassed(){}
    public virtual void OnLook(){} //populate the ui if it has things to show

    public virtual void TimeLapse(int hours){}

    public virtual void HitWithWater(){}

    public virtual bool IsFlammable()
    {
        if(onFire) return false;
        return flammable;
    }

    public void TakeDamage(float damage)
    {
        float finalDamage = ApplyDamageModifier(damage);
        OnDamage?.Invoke();
        OnDamageWithValue?.Invoke(finalDamage);
        if(!destructable || health <= 0) return;
        health -= finalDamage;
        //if(damageParticles) damageParticles.Play();
        for(int i = 0; i < damageParticles.Count; i++)
        {
            damageParticles[i].Play();
        }

        if(audioHandler && audioHandler.hitSounds.Length > 0) audioHandler.PlayRandomSound(audioHandler.hitSounds);
    }

    protected virtual float ApplyDamageModifier(float damage)
    {
        return damage;
    }

    //ALWAYS CALL BASE.ONDESTROY IF RUNNING ONDESTROY ON ANOTHER STRUCT
    public void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourPassed;
        if(!gameObject.scene.isLoaded) return;
        //print("Destroyed");
        if(clearTileOnDestroy && structData && !absentFromGrid && !forcePile)
        {
            if(structData.gridSize == GridSize.OneByOne)
            {
                StructureManager.Instance.ClearTile(transform.position);
            }
            if(structData.gridSize == GridSize.TwoByTwo)
            {
                StructureManager.Instance.ClearLargeTile(transform.position);
            }
            if(structData.gridSize == GridSize.OneByTwo)
            {
                StructureManager.Instance.ClearOneByTwoTile(transform.position);
            }
            if(structData.gridSize == GridSize.ThreeByThree)
            {
                StructureManager.Instance.ClearExtraLargeTile(transform.position);
            }

        } 
        StructureManager.Instance.allStructs.Remove(this);
        NightSpawningManager.Instance.RemoveDifficultyPoints(wealthValue);
        OnStructuresUpdated?.Invoke();
        
        if(health <= 0 || forcePile) //For when a structure is destroyed by removing all the hp
        {
            if(health <= 0)
            {
                GameObject p = ParticlePoolManager.Instance.GrabDestructionParticle(structData.structureType);
                if(p)
                {
                    if(particleCenter) p.transform.position = particleCenter.position;
                    else p.transform.position = transform.position;
                }

                if(gibs)
                {
                    if(particleCenter) Instantiate(gibs, particleCenter.position, Quaternion.identity);
                    else Instantiate(gibs, transform.position, Quaternion.identity);
                }

                if(structData) OnStructureDestroyed?.Invoke(structData, transform.position);
            }

            //logic for spawning the salvagable pile//
            if(structData && !absentFromGrid && salvageChance > Random.Range(0,100) && (!onFire || MainMenuScript.currentFileMode == FileMode.Cozy))
            {
                //Spawn the pile
                DebrisPile newPile = StructureManager.Instance.SpawnStructureWithInstance(StructureDatabase.Instance.GetPile(structData).objectPrefab, transform.position).GetComponent<DebrisPile>();
                newPile.InsertStructure(structData);
                newPile.transform.rotation = transform.rotation;
                if(forcePile) newPile.giveItemBack = true;
            }

        }

        if(audioHandler && audioHandler.breakSound && !muteSound) audioHandler.PlaySoundAtPoint(audioHandler.breakSound, transform.position);

    }

    protected void CallDestroyedEvent() //Used for the farm tiles
    {
        OnStructureDestroyed?.Invoke(structData, transform.position);
    }

    public void ToggleHighlight(bool enable)
    {
        OnHighlight(enable);
        if(HideUI.hideUI) return; //if the UI is hidden, do not show highlights
        
        if (highlight.Count == 0)
        {
            return;
        }
        if(!canShowHighlight)
        {
            if(structureUI && enable) structureUI.SetActive(true);
            if(structureUI && !enable) structureUI.SetActive(false);
            if(highlightEnabled)
            {
                highlightEnabled = false;
                foreach(GameObject thing in highlight) thing.SetActive(false);
                //if(structureUI) structureUI.SetActive(false);
            }
            return;
        }
        
        if(highlightMaterial.Count == 0)
        {
            foreach(GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<Renderer>().material);
        }
        if(enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach(GameObject thing in highlight) thing.SetActive(true);
            if(structureUI) structureUI.SetActive(true);
            if(highlightCoroutine == null) highlightCoroutine = StartCoroutine(HightlightFlash());
        }

        if(!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach(GameObject thing in highlight) thing.SetActive(false);
            if(structureUI) structureUI.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while(highlightEnabled)
        {
            do
            {
                yield return new WaitForSeconds(0.1f);
                power -= 0.1f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power > 1f && highlightEnabled);
            do
            {
                yield return new WaitForSeconds(0.1f);
                power += 0.1f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power < 2.5f && highlightEnabled);
        }
        highlightCoroutine = null;
    }

    protected virtual void OnHighlight(bool enabled){}

    public void LitOnFire()
    {
        if(onFire || !flammable) return;
        onFire = true;
        GameObject flame = ParticlePoolManager.Instance.GrabFlameEffect();
        flame.transform.position = transform.position;
        flame.GetComponent<StructureFire>().burningStruct = this;
        StartCoroutine(Burn());
    }

    public void Extinguish()
    {
        onFire = false;
        StartCoroutine(ExtinguishCooldown());
    }

    IEnumerator ExtinguishCooldown()
    {
        if(flammable)
        {
            flammable = false;
            yield return new WaitForSeconds(10);
            flammable = true;
        }
    }

    IEnumerator Burn()
    {
        while(onFire)
        {
            if(health > 20) TakeDamage(Mathf.Round(health / 10));
            else TakeDamage(2);
            yield return new WaitForSeconds(2f);
            if(MainMenuScript.currentFileMode == FileMode.Cozy) yield return new WaitForSeconds(2f);
        }
    }

    public virtual void DigAction()
    {
        if(itemForm)
        {
            if(Random.Range(0, maxHealth) <= health)
            {
                GameObject droppedItem = ItemPoolManager.Instance.GrabItem(itemForm);
                droppedItem.transform.position = transform.position;
            }
            else health = -5;

            AudioPoolManager.Instance.PlayClipAtPosition(AudioPoolManager.Instance.digUpSound, transform.position);
        }
        ParticlePoolManager.Instance.GrabStructDigParticle().transform.position = transform.position;
        Destroy(this.gameObject);
    }

    public void PlaceAsPile()
    {
        salvageChance = 101;
        forcePile = true;
        Destroy(this.gameObject);
    }

    public virtual bool RepairWithSealant(int amount)
    {
        if(!repairableWithGlue || health == maxHealth) return false;
        health += amount;
        if(health > maxHealth) health = maxHealth;
        return true;
    }

    public virtual void SaveVariables()
    {
        //
    }

    public virtual void LoadVariables()
    {
        //
    }

    public virtual List<StructureUIValueGroup> GetStructureUIValues()
    {
        if(!structureUIVariables.enableUI || structureUIVariables.valueGroups.Count == 0) return null;
        structureUIVariables.valueGroups[0].name = "Integrity";
        structureUIVariables.valueGroups[0].value = health;
        structureUIVariables.valueGroups[0].maxValue = maxHealth;
        structureUIVariables.valueGroups[0].barColor = Color.red;
        return structureUIVariables.valueGroups;
    }
}

[System.Serializable]
public class RepairItem
{
    public InventoryItemData item;
    public int repairAmount;
}

[System.Serializable]
public class StructureUIValues
{
    public bool enableUI = false;
    public List<StructureUIValueGroup> valueGroups = new List<StructureUIValueGroup>();
}

[System.Serializable]
public class StructureUIValueGroup
{
    public string name;
    public Sprite icon;
    [HideInInspector] public float value;
    public float maxValue;
    public Color barColor;
}


