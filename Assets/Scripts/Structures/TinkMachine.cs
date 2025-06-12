using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TinkMachine : StructureBehaviorScript
{

    public GameObject normal, overheating, thrusters;

    public ParticleSystem extinguishedParticles;

    public bool isOverheating = false;

    int heatPoints = 0;
    int maxHeatPoints = 10;

    //
    void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0)
        {
            heatPoints = 0;
            OverheatToggle(false);
            PlayerInteraction.Instance.waterHeld--;
            success = true;
        }
    }

    public override void HitWithWater()
    {
        heatPoints = 0;
        OverheatToggle(false);
    }

    public override void HourPassed()
    {
        if(TimeManager.Instance.timeSkipping && !TimeManager.Instance.isDay)
        {
            TakeDamage(999);
        }

        if(TimeManager.Instance.currentHour == 6)
        {
            //Clear Tiles
            StructureManager.Instance.ClearLargeTile(transform.position);
            StartCoroutine(LiftOff());
            thrusters.SetActive(true);
        }
    }

    void OverheatToggle(bool overheat)
    {
        if(overheat == isOverheating) return;
        if(overheat)
        {
            isOverheating = true;
            normal.SetActive(false);
            overheating.SetActive(true);
        }
        else
        {
            isOverheating = false;
            normal.SetActive(false);
            overheating.SetActive(true);

            extinguishedParticles.Play();
        }
    }

    IEnumerator AttractCoroutine()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(7, 35));

            Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 15f, 1 << 9);
            foreach(Collider collider in hitEnemies)
            {
                var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
                if (creature != null)
                {
                    creature.NewPriorityTarget(this);
                }
            }
        }
    }

    IEnumerator Overheat()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(Random.Range(5, 10));

            if(isOverheating) TakeDamage(5);

            else
            {
                heatPoints += Random.Range(1, 3);
            }
            if(heatPoints >= maxHeatPoints)
            {
                OverheatToggle(true);
            }
        }
    }

    IEnumerator LiftOff()
    {
        int y = 0;
        yield return new WaitForSeconds(1.6f);
        while(y < 100)
        {
            yield return new WaitForSeconds(0.1f);

            transform.Translate(Vector3.up * 1, Space.World);
            y++;
        }
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded || health > 0) return; 
        ParticlePoolManager.Instance.GrabExplosionParticle().transform.position = focalPoint.position;
    }
}
