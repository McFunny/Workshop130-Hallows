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
    public ParticleSystem[] stage1Particles, stage2Particles;

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

    void Awake()
    {
        for(int i = 0; i < stage1Particles.Length; i++)
        {
            stage1Particles[i].enableEmission = false;
        }
        for(int i = 0; i < stage2Particles.Length; i++)
        {
            stage2Particles[i].enableEmission = false;
        }
    }

    void Start()
    {
        base.Start();
        StartCoroutine(SecondTimer());
        SpawnIn();
    }


    void Update()
    {
        //ISSUE, IT NEEDS TO OCCUPY THE CURRENT TILE ORTHERWISE THE PLAYER CAN BUILD STRUCTS ON IT
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
        if (timeSinceLastSeenPlayer >= 80 && currentState != CreatureState.SummonCrows)
        {
            currentState = CreatureState.SummonCrows;
        }
        else if (timeSinceLastSeenPlayer >= 60)
        {
            if(currentState != CreatureState.Stage3)
            {
                effectsHandler.RandomIdle();
                for(int i = 0; i < stage1Particles.Length; i++)
                {
                    stage1Particles[i].enableEmission = true;
                }
                for(int i = 0; i < stage2Particles.Length; i++)
                {
                    stage2Particles[i].enableEmission = true;
                }
            }
            currentState = CreatureState.Stage3;
            anim.SetInteger("PowerLevel", 3);
        }
        else if (timeSinceLastSeenPlayer >= 40)
        {
            if(currentState != CreatureState.Stage2)
            {
                effectsHandler.RandomIdle();
                for(int i = 0; i < stage1Particles.Length; i++)
                {
                    stage1Particles[i].enableEmission = true;
                }
                for(int i = 0; i < stage2Particles.Length; i++)
                {
                    stage2Particles[i].enableEmission = false;
                }
            }
            currentState = CreatureState.Stage2;
            anim.SetInteger("PowerLevel", 2);
        }
        else if (timeSinceLastSeenPlayer >= 40)
        {
            currentState = CreatureState.Stage2;
            pEmission.rateOverTime = 5;
        }
        else if (timeSinceLastSeenPlayer >= 20)
        {
            if(currentState != CreatureState.Stage1)
            {
                effectsHandler.RandomIdle();
                for(int i = 0; i < stage1Particles.Length; i++)
                {
                    stage1Particles[i].enableEmission = false;
                }
                for(int i = 0; i < stage2Particles.Length; i++)
                {
                    stage2Particles[i].enableEmission = false;
                }
            } 
            currentState = CreatureState.Stage1;
            anim.SetInteger("PowerLevel", 1);
        }
        else if (timeSinceLastSeenPlayer < 20 && currentState != CreatureState.Idle)
        {
            if(currentState != CreatureState.Idle)
            {
                for(int i = 0; i < stage1Particles.Length; i++)
                {
                    stage1Particles[i].enableEmission = false;
                }
                for(int i = 0; i < stage2Particles.Length; i++)
                {
                    stage2Particles[i].enableEmission = false;
                }
            }

            currentState = CreatureState.Idle;
            anim.SetInteger("PowerLevel", 0);

            //effectsHandler.RandomIdle();
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
       
        yield return new WaitForSeconds(0.8f);
        timeSinceLastSeenPlayer = 0f; //Reset
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
