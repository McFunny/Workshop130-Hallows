using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandMine : StructureBehaviorScript
{
    public InventoryItemData recoveredItem;

    public MeshRenderer light;
    //public Material yellow, red, purple, green, black;
    public Color yellow, red, purple, green, black;
    Color activeColor;
    public Animator anim;
    public ParticleSystem fizzParticles;
    public GameObject explodeObject, dirtObject;
    AudioSource source;
    float lightLerp;
    bool flashOn = true;

    float structureRange = 3;
    float creatureRange = 4.5f;
    float cooldownProgress = 0;
    float cooldownLength = 45; //seconds
    float newPitch = 0.8f;
    bool isPrimed = false;
    bool validTile = true;
    bool isExploding = false;
    bool pulseLight = false;
    //bool isDestroyed = false;
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
        source = audioHandler.GetSource();
    }


    void Update()
    {
        /*if(health <= 0 && isDestroyed)
        {
            isDestroyed = true;
            //if(isPrimed)
            //Destroy itself. If its primed, it will explode first
        }*/


        if(flashOn && lightLerp >= 1) flashOn = false;
        if(!flashOn && lightLerp <= 0) flashOn = true;

        if(flashOn) lightLerp += 0.01f;
        else lightLerp -= 0.01f;
        //
        light.material.color = Color.Lerp(activeColor, black, lightLerp);
        light.material.SetColor("_EmissionColor", Color.Lerp(activeColor, black, lightLerp));

        if(isExploding)
        {
            newPitch += 0.01f;
            source.pitch = newPitch;
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
        else if(health < 0 && !isExploding) Destroy(this.gameObject);
    }

    public override void HitWithWater()
    {
        TryExplosion();
    }

    public override void StructureInteraction()
    {
        //Swap active nutrients
        if(!isPrimed) return;

        if(nutrientType == NutrientType.Gloamphage)
        {
            if(nutrients.terraLevel >= 8) nutrientType = NutrientType.Terrazyme;
            else if(nutrients.ichorLevel >= 8) nutrientType = NutrientType.Ichor;
        }
        else if(nutrientType == NutrientType.Terrazyme)
        {
            if(nutrients.ichorLevel >= 8) nutrientType = NutrientType.Ichor;
            else if(nutrients.gloamLevel >= 8) nutrientType = NutrientType.Gloamphage;
        }
        else if(nutrientType == NutrientType.Ichor)
        {
            if(nutrients.gloamLevel >= 8) nutrientType = NutrientType.Gloamphage;
            else if(nutrients.terraLevel >= 8) nutrientType = NutrientType.Terrazyme;
        }
        LightColorChange();
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

    void ExplosionLogic()
    {
        //ParticlePoolManager.Instance.GrabExplosionParticle().transform.position = transform.position;
        explodeObject.SetActive(true);
        dirtObject.SetActive(true);
        audioHandler.PlaySound(audioHandler.activatedSound);
        ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        
        if(Vector3.Distance(transform.position, PlayerInteraction.Instance.transform.position) < 4.5f) PlayerInteraction.Instance.StaminaChange(-65);
        Collider[] hitStructures = Physics.OverlapSphere(transform.position, structureRange, 1 << 6);
        foreach(Collider collider in hitStructures)
        {
            StructureBehaviorScript structure = collider.gameObject.GetComponentInParent<StructureBehaviorScript>();
            if(structure && structure != this)
            {
                structure.TakeDamage(25);
            }
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, creatureRange, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(75);
                creature.PlayHitParticle(new Vector3(transform.position.x, transform.position.y, transform.position.z));
            }
        }
    }

    IEnumerator Explode()
    {
        newPitch = 0.8f;
        fizzParticles.Play();
        source.Play();
        yield return new WaitForSeconds(1.6f);
        fizzParticles.Stop();
        source.Stop();

        isExploding = false;
        source.pitch = 1;

        if(nutrientType == NutrientType.Gloamphage) nutrients.gloamLevel = 0;
        if(nutrientType == NutrientType.Terrazyme) nutrients.terraLevel = 0;
        if(nutrientType == NutrientType.Ichor) nutrients.ichorLevel = 0;

        ExplosionLogic();

        InitializeMine();

        anim.SetTrigger("Exploded");


        if(health < 0) Destroy(this.gameObject);
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
                    anim.SetBool("IsActivated", true);
                }
            }
            LightColorChange();
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
        if(!isPrimed)
        {
            if(NutrientsAvailable())
            {
                activeColor = yellow;
                anim.SetBool("IsActivated", true);
            }
            else
            {
                activeColor = black;
                anim.SetBool("IsActivated", false);
            }
            return;
        }
        anim.SetBool("IsActivated", true);

        if(nutrientType == NutrientType.Gloamphage)
        {
            activeColor = purple;
        } 

        if(nutrientType == NutrientType.Terrazyme)
        {
            activeColor = green;
        } 

        if(nutrientType == NutrientType.Ichor)
        {
            activeColor = red;
        } 
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
        if (!gameObject.scene.isLoaded) return; 
        //if(dirtObject.activeSelf)
        //{
            dirtObject.GetComponent<DisableAfterTimer>().destroyOnCompletion = true;
            dirtObject.transform.parent = null;
        //}
        
    }

    public override void LoadVariables()
    {
        if(saveString1 == "gloam") nutrientType = NutrientType.Gloamphage;
        if(saveString1 == "terra") nutrientType = NutrientType.Terrazyme;
        if(saveString1 == "ichor") nutrientType = NutrientType.Ichor;
        //LightColorChange();
    }

    public override void SaveVariables()
    {
        if(nutrientType == NutrientType.Gloamphage) saveString1 = "gloam";
        if(nutrientType == NutrientType.Terrazyme) saveString1 = "terra";
        if(nutrientType == NutrientType.Ichor) saveString1 = "ichor";
    }
}

public enum NutrientType
{
    Gloamphage,
    Terrazyme,
    Ichor
}
