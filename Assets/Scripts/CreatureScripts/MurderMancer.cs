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
    public GameObject crowPrefab;
    public ParticleSystem stageParticles; //rates are 0, 2, 5, and 15

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

    IEnumerator SecondTimer()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1);
            if (TimeManager.Instance.isDay)
            {
                base.OnDeath();
                Destroy(this.gameObject);
            }

            float distance = Vector3.Distance(player.position, transform.position);
            playerInSightRange = distance <= sightRange;
            if (playerInSightRange)
            {
                if (timeSinceLastSeenPlayer >= 60)
                {
                    timeSinceLastSeenPlayer = 60;
                }
                else if (timeSinceLastSeenPlayer >= 40)
                {
                    timeSinceLastSeenPlayer = 40;
                }
                else if (timeSinceLastSeenPlayer >= 20)
                {
                    timeSinceLastSeenPlayer = 20;
                }
                else if (timeSinceLastSeenPlayer < 20)
                {
                    timeSinceLastSeenPlayer = 0;
                }
            }
            else { timeSinceLastSeenPlayer += 1; }
            CheckStage();
            CheckState(currentState);
        }
    }

    private void CheckStage()
    {
        var pEmission = stageParticles.emission;
        if (timeSinceLastSeenPlayer >= 80)
        {
            currentState = CreatureState.SummonCrows;
        }
        else if (timeSinceLastSeenPlayer >= 60)
        {
            currentState = CreatureState.Stage3;
            pEmission.rateOverTime = 15;
        }
        else if (timeSinceLastSeenPlayer >= 40)
        {
            currentState = CreatureState.Stage2;
            pEmission.rateOverTime = 5;
        }
        else if (timeSinceLastSeenPlayer >= 20)
        {
            currentState = CreatureState.Stage1;
            pEmission.rateOverTime = 2;
        }
        else if (timeSinceLastSeenPlayer < 20)
        {
            currentState = CreatureState.Idle;
            pEmission.rateOverTime = 0;
        }
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
        MutatedCrow crow1 = Instantiate(crowPrefab, leftArmCrowSummon.position, leftArmCrowSummon.rotation).GetComponent<MutatedCrow>();
        MutatedCrow crow2 = Instantiate(crowPrefab, rightArmCrowSummon.position, rightArmCrowSummon.rotation).GetComponent<MutatedCrow>();

        crow1.isSummoned = true;
        crow2.isSummoned = true;
        crow1.isAttackCrow = true;
        crow2.isAttackCrow = false;
        


        timeSinceLastSeenPlayer = 40f; //Put back into stage 2
       
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
