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
    }


    void Update()
    {
        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        if (playerInSightRange)
        {
            timeSinceLastSeenPlayer = 0;
        }
        else { timeSinceLastSeenPlayer += Time.deltaTime; }
        CheckStage();
        CheckState(currentState);
    }

    private void CheckStage()
    {
        if (timeSinceLastSeenPlayer >= 80)
        {
            currentState = CreatureState.SummonCrows;
        }
        else if (timeSinceLastSeenPlayer >= 60)
        {
            currentState = CreatureState.Stage3;
        }
        else if (timeSinceLastSeenPlayer >= 40)
        {
            currentState = CreatureState.Stage2;
        }
        else if (timeSinceLastSeenPlayer >= 20)
        {
            currentState = CreatureState.Stage1;
        }
        else if (timeSinceLastSeenPlayer < 20)
        {
            currentState = CreatureState.Idle;
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
        Crow crow1 = Instantiate(crowPrefab, leftArmCrowSummon.position, leftArmCrowSummon.rotation).GetComponent<Crow>();
        Crow crow2 = Instantiate(crowPrefab, rightArmCrowSummon.position, rightArmCrowSummon.rotation).GetComponent<Crow>();

        crow1.isSummoned = true;
        crow2.isSummoned = true;


        timeSinceLastSeenPlayer = 60f; //Put back into stage 3
       
        yield return new WaitForSeconds(0.1f);
        coroutineRunning = false;
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
}
