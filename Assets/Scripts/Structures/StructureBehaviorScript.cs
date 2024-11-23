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

    public StructureObject structData;

    public float health = 5;
    public float maxHealth = 5;

    public float wealthValue = 1; //dictates how hard a night could be 

    [HideInInspector] public StructureAudioHandler audioHandler;
    //[HideInInspector] public AudioSource source;

    [HideInInspector] public bool clearTileOnDestroy = true;


    public void Awake()
    {
        StructureManager.Instance.allStructs.Add(this);
        OnStructuresUpdated?.Invoke();
        //source = GetComponent<AudioSource>();
        audioHandler = GetComponent<StructureAudioHandler>();

        TimeManager.OnHourlyUpdate += HourPassed;
    }


    public void Update()
    {
        if(health <= 0) Destroy(this.gameObject);
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

    public void OnDestroy()
    {
        if(!gameObject.scene.isLoaded) return;
        print("Destroyed");
        if(clearTileOnDestroy && structData)
        {
            if(!structData.isLarge) StructureManager.Instance.ClearTile(transform.position);
            else StructureManager.Instance.ClearLargeTile(transform.position);
        } 
        StructureManager.Instance.allStructs.Remove(this);
        OnStructuresUpdated?.Invoke();
        TimeManager.OnHourlyUpdate -= HourPassed;

    }
}
