using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MurderMancer : CreatureBehaviorScript
{
    [SerializeField] private float timeSinceLastSeenPlayer;
    private bool coroutineRunning;
    public Transform rightArmCrowSummon;
    public Transform leftArmCrowSummon;
    public CreatureObject crowData;
    public GameObject burningParticles;
    public ParticleSystem stageParticles; //rates are 0, 2, 5, and 15

    Vector3 origin;

    public enum CreatureState
    {
        FlyIn,
        Stage1,
        Stage2,
        Stage3,
        SummonCrows,
        Idle,
        Stun,
        Die,
    }

    public CreatureState currentState;

    void Start()
    {
        base.Start();
        StartCoroutine(SecondTimer());
        SpawnIn();
    }


    void Update()
    {
        //ISSUE, IT NEEDS TO OCCUPY THE CURRENT TILE OTHERWISE THE PLAYER CAN BUILD STRUCTS ON IT
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.Torch && PlayerInteraction.Instance.torchLit && !coroutineRunning)
        {
            StartCoroutine(SnuffTorch());
            success = true;
        }
        else success = false;
    }

    IEnumerator SnuffTorch()
    {
        coroutineRunning = true;
        burningParticles.SetActive(true);
        yield return new WaitForSeconds(1.8f);
        effectsHandler.MiscSound();
        HandItemManager.Instance.TorchFlameToggle(false);
        LowerStage();
        yield return new WaitForSeconds(0.4f);
        burningParticles.SetActive(false);
        coroutineRunning = false;
    }

    IEnumerator SecondTimer()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1);
            if(coroutineRunning) yield return null;
            if (TimeManager.Instance.isDay)
            {
                TakeDamage(999);
            }

            timeSinceLastSeenPlayer += 1;
            CheckStage();
            CheckState(currentState);
        }
    }

    private void CheckStage()
    {
        var pEmission = stageParticles.emission;
        if (timeSinceLastSeenPlayer >= 40 && currentState != CreatureState.SummonCrows)
        {
            currentState = CreatureState.SummonCrows;
            anim.SetInteger("PowerLevel", 4);
        }
        else if (timeSinceLastSeenPlayer >= 30 && currentState != CreatureState.Stage3)
        {
            if(currentState != CreatureState.Stage3) effectsHandler.RandomIdle();
            currentState = CreatureState.Stage3;
            anim.SetInteger("PowerLevel", 3);
            pEmission.rateOverTime = 15;
        }
        else if (timeSinceLastSeenPlayer >= 20)
        {
            if(currentState != CreatureState.Stage2) effectsHandler.RandomIdle();
            currentState = CreatureState.Stage2;
            anim.SetInteger("PowerLevel", 2);
            pEmission.rateOverTime = 5;
        }
        else if (timeSinceLastSeenPlayer >= 10 && currentState != CreatureState.Stage1)
        {
            if(currentState != CreatureState.Stage1) effectsHandler.RandomIdle();
            currentState = CreatureState.Stage1;
            anim.SetInteger("PowerLevel", 1);
            pEmission.rateOverTime = 2;
        }
        else if (timeSinceLastSeenPlayer < 10 && currentState != CreatureState.Idle)
        {
            currentState = CreatureState.Idle;
            anim.SetInteger("PowerLevel", 0);
            pEmission.rateOverTime = 0;
            //effectsHandler.RandomIdle();
        }
    }

    void LowerStage()
    {
        if (timeSinceLastSeenPlayer < 10)
        {
            /*TakeDamage(50);
            if(health <= 0)
            {
                //dropitems
                Destroy(this.gameObject);
                return;
            }*/
            StructureManager.Instance.ClearTile(origin);
            SpawnIn();
        }
        timeSinceLastSeenPlayer = 0;
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.FlyIn:
                FlyIn();
                break;
            case CreatureState.Stage1:
                Stage1();
                break;
            case CreatureState.Stage2:
                Stage2();
                break;
            case CreatureState.Stage3:
                Stage3();
                break;
            case CreatureState.SummonCrows:
                SummonCrows();
                break;
            case CreatureState.Idle:
                Idle();
                break;
            case CreatureState.Stun:
                Stun();
                break;
            case CreatureState.Die:
                Die();
                break;
        }
    }

    private void FlyIn()
    {
        
    }

    private void Stage1()
    {
       //do a certain animation
       //play a certain particle
       //emit a certain light
    }

    private void Stage2()
    {
        //do a certain animation
        //play a certain particle
        //emit a certain light
        //Summon a crow on a shoulder
    }

    private void Stage3()
    {
        //do a certain animation
        //play a certain particle
        //emit a certain light
        //Summon another crow
    }

    private void SummonCrows()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(Summon());
        }
    }

    IEnumerator Summon()
    {
        coroutineRunning = true;
        effectsHandler.Idle1();
        if(NightSpawningManager.Instance.ReportTotalOfCreature(crowData) < 10)
        {
            MutatedCrow crow1 = Instantiate(crowData.objectPrefab, leftArmCrowSummon.position, leftArmCrowSummon.rotation).GetComponent<MutatedCrow>();
            MutatedCrow crow2 = Instantiate(crowData.objectPrefab, rightArmCrowSummon.position, rightArmCrowSummon.rotation).GetComponent<MutatedCrow>();

            crow1.isSummoned = true;
            crow2.isSummoned = true;
            crow1.isAttackCrow = true;
            crow2.isAttackCrow = false;
        }
        
        yield return new WaitForSeconds(0.3f);
        HandItemManager.Instance.TorchFlameToggle(false);
        foreach (var structure in structManager.allStructs)
        {
            Brazier brazier = structure as Brazier;
            if (brazier && brazier.flameLeft > 0)
            {
                brazier.flameLeft = 0;
            }
        }
        effectsHandler.MiscSound2();

        timeSinceLastSeenPlayer = 0f; //Reset
       
        yield return new WaitForSeconds(0.1f);
        coroutineRunning = false;
    }

    void SpawnIn()
    {
        Vector3 newPos = StructureManager.Instance.GetRandomClearTile();
        if(newPos == new Vector3(0,0,0)) Destroy(this.gameObject);
        else
        {
            transform.position = newPos;
            StructureManager.Instance.SetTile(newPos);
            origin = transform.position;
            effectsHandler.OnMove(1);
        }
    }

    private void Idle()
    {
       
    }

    private void Stun()
    {
        throw new NotImplementedException();
    }

    private void Die()
    {
        throw new NotImplementedException();
    }

    void OnDestroy()
    {
        if (!gameObject.scene.isLoaded) return; 
        StructureManager.Instance.ClearTile(transform.position);

    }
}
