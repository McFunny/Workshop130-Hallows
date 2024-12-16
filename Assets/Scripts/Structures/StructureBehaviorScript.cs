using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StructureBehaviorScript : MonoBehaviour
{
    //This is the base class that ALL structures should derive from

    public delegate void StructuresUpdated();
    public static event StructuresUpdated OnStructuresUpdated; //Unity Event that will notify enemies when structures are updated
    //Should this be static Abner?

    public delegate void Damaged();
    [HideInInspector] public event Damaged OnDamage; //Unity Event that will notify enemies when structures are updated

    public StructureObject structData;

    public float health = 5;
    public float maxHealth = 5;

    public float wealthValue = 1; //dictates how hard a night could be 

    public bool destructable = true;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    [HideInInspector] public StructureAudioHandler audioHandler;
    //[HideInInspector] public AudioSource source;

    [HideInInspector] public bool clearTileOnDestroy = true;


    public void Awake()
    {
        OnStructuresUpdated?.Invoke();
        //source = GetComponent<AudioSource>();
        audioHandler = GetComponent<StructureAudioHandler>();

        TimeManager.OnHourlyUpdate += HourPassed;
        foreach(GameObject thing in highlight) thing.SetActive(false);
    }

    public void Start()
    {
        StructureManager.Instance.allStructs.Add(this);
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

    public void TakeDamage(float damage)
    {
        if(!destructable) return;
        health -= damage;
        OnDamage?.Invoke();
    }

    public void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= HourPassed;
        if(!gameObject.scene.isLoaded) return;
        print("Destroyed");
        if(clearTileOnDestroy && structData)
        {
            if(!structData.isLarge) StructureManager.Instance.ClearTile(transform.position);
            else StructureManager.Instance.ClearLargeTile(transform.position);
        } 
        StructureManager.Instance.allStructs.Remove(this);
        NightSpawningManager.Instance.RemoveDifficultyPoints(wealthValue);
        OnStructuresUpdated?.Invoke();

    }

    public void ToggleHighlight(bool enable)
    {
        if(highlight.Count == 0) return;
        if(highlightMaterial.Count == 0)
        {
            foreach(GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if(enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach(GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if(!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach(GameObject thing in highlight) thing.SetActive(false);
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
                power -= 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power > 0.7f && highlightEnabled);
            do
            {
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power < 1.9f && highlightEnabled);
        }
    }
}
