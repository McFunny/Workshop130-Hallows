using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[CreateAssetMenu(fileName = "New Armor Behavior", menuName = "Armor Behavior/MiningHelmet")]
public class ArmorBehavior : ScriptableObject
{
    public List<StatModifier> modifiers = new List<StatModifier>();
    public List<ArmorEffect> effects = new List<ArmorEffect>();

    public virtual void OnEquip()
    {
        Apply(true);
    }

    public virtual void OnUnequip()
    {
        Apply(false);
    }

    private void Apply(bool equip)
    {
        for (int i = 0; i < effects.Count; i++)
        {
            switch (effects[i])
            {
                case ArmorEffect.none:
                    break;
                case ArmorEffect.light:
                    if (ArmorManager.Instance) ArmorManager.Instance.miningLight?.SetActive(equip);
                    break;
            }
        }
    }
}
    [System.Serializable]

    public struct StatModifier
    {
        public StatType stat;
        public float flat;
        public float percent;
    }

    public enum StatType
    {
        MaxHealth,
        MaxWater,
        Defense,
        MoveSpeed,
        AttackDamage,
        TillingSpeed
    }

    public enum ArmorEffect
    {
        none,
        light
    }

