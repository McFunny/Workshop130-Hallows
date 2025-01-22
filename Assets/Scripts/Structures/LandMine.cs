using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMine : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    public MeshRenderer light;
    //public Material yellow, red, purple, green, black;
    public Color yellow, red, purple, green, black;
    public Animator anim;
    public ParticleSystem fizzParticles;

    float structureRange = 3;
    float creatureRange = 4.5f;
    float cooldownProgress = 0;
    float cooldownLength = 20; //seconds
    bool isPrimed = false;
    bool validTile = true;
    bool isExploding = false;
    NutrientType nutrientType;
    

    private NutrientStorage nutrients;

    //When loading from save data, just have it already armed

    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
        OnDamage += TryExplosion;
        StartCoroutine(SecondTimer());
        nutrients = StructureManager.Instance.FetchNutrient(transform.position);

        InitializeMine();
    }


    void Update()
    {
        if(health <= 0)
        {
            //Destroy itself. If its primed, it will explode first
        }
    }

    void InitializeMine()
    {
        if(nutrients.gloamLevel >= 8)
        {
            nutrientType = NutrientType.Gloamphage;
            return;
        }
        if(nutrients.terraLevel >= 8)
        {
            nutrientType = NutrientType.Terrazyme;
            return;
        }
        if(nutrients.ichorLevel >= 8)
        {
            nutrientType = NutrientType.Ichor;
            return;
        }
    }

    void TryExplosion()
    {
        if(isPrimed && !isExploding)
        {
            isPrimed = false;
            isExploding = true;
            StartCoroutine(Explode());
        }
    }

    public override void StructureInteraction()
    {
        //Swap active nutrients
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel && !isPrimed && !isExploding)
        {
            StartCoroutine(DugUp());
            success = true;
        }
    }

    IEnumerator Explode()
    {
        fizzParticles.Play();
        yield return new WaitForSeconds(1.2f);
        //explode logic
        isExploding = false;

        if(nutrientType == NutrientType.Gloamphage) nutrients.gloamLevel = 0;
        if(nutrientType == NutrientType.Terrazyme) nutrients.terraLevel = 0;
        if(nutrientType == NutrientType.Ichor) nutrients.ichorLevel = 0;
    }

    IEnumerator SecondTimer()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(1);
            if(!isPrimed && !isExploding && cooldownProgress < cooldownLength)
            {
                cooldownProgress++;
                if(cooldownProgress == cooldownLength)
                {
                    cooldownProgress = 0;
                    isPrimed = true;
                }
            }
            //code to check

        }
    }

    IEnumerator DugUp()
    {
        yield return new WaitForSeconds(1);
        GameObject droppedItem = ItemPoolManager.Instance.GrabItem(recoveredItem);
        droppedItem.transform.position = transform.position;

        Destroy(this.gameObject);
    }

    void LightColorChange()
    {
        //
    }

    bool NutrientsAvailable()
    {
        if(nutrientType == NutrientType.Gloamphage && nutrients.gloamLevel > 8) return true;

        if(nutrientType == NutrientType.Terrazyme && nutrients.terraLevel > 8) return true;

        if(nutrientType == NutrientType.Ichor && nutrients.ichorLevel > 8) return true;

        return false;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        OnDamage -= TryExplosion;
    }
}

public enum NutrientType
{
    Gloamphage,
    Terrazyme,
    Ichor
}
