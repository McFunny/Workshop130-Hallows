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
    public Transform hareSummon;
    public CreatureObject spawnedCreatureData;
    public GameObject burningParticles;
    public ParticleSystem[] stage1Particles, stage2Particles;

    Vector3 origin;

    public enum Variant
    {
        Normal,
        Hare
    }

    public Variant variant; // what variant of creature is this?

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
        //ISSUE, IT NEEDS TO OCCUPY THE CURRENT TILE OTHERWISE THE PLAYER CAN BUILD STRUCTS ON IT
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.Torch && PlayerInteraction.Instance.torchLit && !coroutineRunning)
        {
            StartCoroutine(ExtinguishSelf(true));
            success = true;
        }
        else success = false;
    }

    public void IgnitedByOther()
    {
        if(!coroutineRunning) StartCoroutine(ExtinguishSelf(false));
    }

    IEnumerator ExtinguishSelf(bool litByPlayer)
    {
        float extinguishTime = 10;
        coroutineRunning = true;
        burningParticles.SetActive(true);
        anim.SetBool("OnFire", true);
        yield return new WaitForSeconds(1.8f);
        if(litByPlayer)
        {
            effectsHandler.MiscSound();
            //HandItemManager.Instance.TorchFlameToggle(false);
        }
        LowerStage();
        CheckStage();
        yield return new WaitForSeconds(extinguishTime);
        burningParticles.SetActive(false);
        anim.SetBool("OnFire", false);
        yield return new WaitForSeconds(0.6f);
        coroutineRunning = false;
    }

    IEnumerator SecondTimer()
    {
        while(gameObject.activeSelf)
        {
            yield return new WaitForSeconds(1);
            if(!coroutineRunning)
            {
                if (TimeManager.Instance.isDay)
                {
                    GameObject poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
                    poofParticle.transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
                    TakeDamage(999);
                }

                timeSinceLastSeenPlayer += 1;
                CheckStage();
                CheckState(currentState);
            }
            
        }
    }

    private void CheckStage()
    {
        if (timeSinceLastSeenPlayer >= 85 && currentState != CreatureState.SummonCrows)
        {
            currentState = CreatureState.SummonCrows;
            anim.SetInteger("PowerLevel", 4);
        }
        else if (timeSinceLastSeenPlayer >= 65)
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
        else if (timeSinceLastSeenPlayer >= 45)
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
        else if (timeSinceLastSeenPlayer >= 25)
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
        else if (timeSinceLastSeenPlayer < 25 && currentState != CreatureState.Idle)
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

        if(variant == Variant.Normal)
        {
            int totalCrows = NightSpawningManager.Instance.ReportTotalOfCreature(spawnedCreatureData);
            if(totalCrows < 6)
            {
                int spawnCycles = 1;
                if(totalCrows < 4) spawnCycles++;
                for(int i = 0; i < spawnCycles; i++)
                {
                    MutatedCrow crow1 = Instantiate(spawnedCreatureData.objectPrefab, leftArmCrowSummon.position, leftArmCrowSummon.rotation).GetComponent<MutatedCrow>();
                    MutatedCrow crow2 = Instantiate(spawnedCreatureData.objectPrefab, rightArmCrowSummon.position, rightArmCrowSummon.rotation).GetComponent<MutatedCrow>();

                    GameObject poofParticle1 = ParticlePoolManager.Instance.GrabCloudParticle();
                    poofParticle1.transform.position = crow1.transform.position;
                    GameObject poofParticle2 = ParticlePoolManager.Instance.GrabCloudParticle();
                    poofParticle2.transform.position = crow2.transform.position;

                    crow1.isSummoned = true;
                    crow2.isSummoned = true;
                    crow1.isAttackCrow = true;
                    crow2.isAttackCrow = false;

                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        if(variant == Variant.Hare)
        {
            int totalHares = NightSpawningManager.Instance.ReportTotalOfCreature(spawnedCreatureData);
            if(totalHares < 6)
            {
                for(int i = 0; i < 4; i++)
                {
                    int hareType = UnityEngine.Random.Range(0, (spawnedCreatureData.creatureVariants.Count + 1));
                    if(hareType > 0 && spawnedCreatureData.creatureVariants[hareType - 1].probabilityInFarm <= 0) hareType = 0;
                    GameObject prefab;
                    if(hareType == 0) prefab = spawnedCreatureData.objectPrefab;
                    else prefab = spawnedCreatureData.creatureVariants[hareType - 1].prefab;
                    Instantiate(prefab, hareSummon.position, Quaternion.identity);
                    ParticlePoolManager.Instance.GrabCloudParticle().transform.position = hareSummon.position;
                }


            }
        }
        
        
        //yield return new WaitForSeconds(0.3f);
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
            GameObject poofParticle;
            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);

            transform.position = newPos;
            StructureManager.Instance.SetTile(newPos);
            origin = transform.position;
            effectsHandler.OnMove(1);

            poofParticle = ParticlePoolManager.Instance.GrabCloudParticle();
            poofParticle.transform.position = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z);
            
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
