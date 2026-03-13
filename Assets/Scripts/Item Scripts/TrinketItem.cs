using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Inventory System/Trinket Item")]
public class TrinketItem : InventoryItemData
{
    public TrinketBehavior behavior;

    public TrinketKey key;

    public bool stackable = false;
    [Tooltip("Does taking damage lose durability? If so, lose durability for each damage")]
    public bool damagedByAttacks = false;

    public float maxDurability = 10;

    //public float durabilityLossRate = 1;

    [Tooltip("If durability is less than or equal to this, guaranteed chance to break")]
    public float guaranteedBreakThreshold = 2; 

    [Tooltip("How much to multiply oncoming damage. Used for basic armor trinkets")]
    public float damageMultiplier = 1;

    [Tooltip("How likely on a scale of 1-100 will this break upon taking enough damage to shatter it?")]
    public float breakChance = 100;

    public void OnEquip()
    {
        behavior.OnEquip();
    }

    public void OnRemove()
    {
        behavior.OnRemove();
    }

    public void TriggerEffect(out float durabilityCost)
    {
        behavior.TriggerEffect(out durabilityCost);
    }
}

public enum TrinketKey
{
    Basic,
    WaterFlask,
    Parry,
    OrangeWingTag,
    YellowWingTag,
    Fogchime,
    Pyrecharge,
    Autocrank,
    RoachRegen,
    HareBoots,
    DunemiteBoots,
    BoneBreaker,
    Coolant,
    WaterGuard,
    TickRegen,
    WeedWard,
    LumenAnklet,
    CarrionCooker,
    MimicNose,
    DuneBoots,
    DewDripper,
    BackstepPendant,
    SeedTalisman,
    GrubBomb,
    PrudentPeriapt
}
