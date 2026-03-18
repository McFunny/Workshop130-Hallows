using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MiniMandrake : CreatureBehaviorScript
{
    [HideInInspector] public NavMeshAgent agent;
    private bool coroutineRunning = false;
    float oldSpeed;

    Vector3 despawnPos;

    float waterLevel = 100; //Dies when reaches 0
    float waterLossRate = 0.5f; //Amount per second
    public GameObject waterIcon, splashObject;

    [HideInInspector] public CreatureBehaviorScript targetCreature;

    public GameObject attackParticle;


    public enum CreatureState
    {
        WakeUp,
        Follow,
        Attack,
        Die,
        Trapped
    }

    public CreatureState currentState;
    private bool hasTarget = false;
    private bool isMoving;

    void Start()
    {
        base.Start();
        agent = GetComponent<NavMeshAgent>();
        currentState = CreatureState.WakeUp;
        //savedTime = timeBeforeLeavingFarm;

        int r = Random.Range(0, NightSpawningManager.Instance.despawnPositions.Length);
        despawnPos = NightSpawningManager.Instance.despawnPositions[r].position;

        //StartCoroutine(WaterDrain());

        PlayerInteraction.OnPlayerAttack += NewTarget;

        oldSpeed = agent.speed;

        GameSaveData.Instance.manikkinsAlive++;

    }

    void OnDestroy()
    {
        PlayerInteraction.OnPlayerAttack -= NewTarget;
        base.OnDestroy();
    }

    void NewTarget(CreatureBehaviorScript c)
    {
        if(!targetCreature) targetCreature = c;
    }

    private void Update()
    {
        if(currentState == CreatureState.Die || currentState == CreatureState.Trapped) return;
        if(currentState == CreatureState.WakeUp)
        {
            CheckState(currentState);
            return;
        }

        CheckState(currentState);

        if(agent.velocity.sqrMagnitude > 0) anim.SetBool("IsRunning", true);
        else anim.SetBool("IsRunning", false);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        if(type == ToolType.WateringCan && PlayerInteraction.Instance.waterHeld > 0 && waterLevel < 100)
        {
            PlayerInteraction.Instance.waterHeld--;
            waterLevel = 100;
            splashObject.SetActive(true);
            success = true;
        }
        else success = false;
    }

    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.WakeUp:
                WakeUp();
                break;

            case CreatureState.Follow:
                Follow();
                break;

            case CreatureState.Attack:
                Attack();
                break;

            case CreatureState.Die:
                //OnDeath();
                break;

            case CreatureState.Trapped:
                Trapped();
                break;
        }
    }

    void Follow()
    {
        if(TownGate.Instance.location != PlayerLocation.InFarm && TownGate.Instance.location != PlayerLocation.InTown) return;
        if(targetCreature)
        {
            currentState = CreatureState.Attack;
            return;
        }
        if (!isMoving && !coroutineRunning)
        {
            Vector3 randomPoint = GetRandomPointAround(player.position, 7f); //gets a random point within a 5 unit radius of itself
            StartCoroutine(MoveToPoint(randomPoint));
        }
    }

    void Attack()
    {
        if(!targetCreature)
        {
            currentState = CreatureState.Follow;
            return;
        }

        if(!coroutineRunning)
        {
            if(targetCreature && Vector3.Distance(transform.position, targetCreature.transform.position) < attackRange)
            {
                StartCoroutine(AttackTarget());
                hasTarget = false;
            }
        }

        agent.SetDestination(targetCreature.transform.position);
    }

    IEnumerator AttackTarget()
    {
        coroutineRunning = true;
        anim.Play("MandrakeAttack");

        yield return new WaitForSeconds(0.3f);
        effectsHandler.OnHit();
        if(!targetCreature || targetCreature.health <= 0)
        {
            targetCreature = null;
        }
        else if(Vector3.Distance(transform.position, targetCreature.transform.position) < attackRange)
        {
            attackParticle.SetActive(true);
            targetCreature.TakeDamage(5);
            if(targetCreature.corpseParticleTransform) targetCreature.PlayHitParticle(targetCreature.corpseParticleTransform.position);
            else targetCreature.PlayHitParticle(targetCreature.transform.position);
        }
        agent.speed = 1;
        yield return new WaitForSeconds(1.5f);
        agent.speed = oldSpeed;
        coroutineRunning = false;
    }

    private IEnumerator WaitAround()
    {
        coroutineRunning = true;
        float r = Random.Range(0.4f, 1.8f);
        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    } 

    private IEnumerator Scream()
    {
        coroutineRunning = true;
        float r = 1.5f;
        effectsHandler.MiscSound();
        anim.SetTrigger("IsScreaming");
        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    }

    private void WakeUp()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(FreakOut());
        }
    }

    IEnumerator FreakOut()
    {
        //Play freak out Animation
        effectsHandler.MiscSound2();
        coroutineRunning = true;
        yield return new WaitForSeconds(1.2f); //Adjust this based off of animation time
        coroutineRunning = false;
        currentState = CreatureState.Follow;
    }

    private Vector3 GetRandomPointAround(Vector3 origin, float radius)
    {

        Vector2 randomDirection = Random.insideUnitCircle * radius;


        Vector3 randomPoint = new Vector3(randomDirection.x, origin.y, randomDirection.y) + origin;

        return randomPoint;
    }

    private IEnumerator MoveToPoint(Vector3 destination)
    {
        isMoving = true;


        agent.destination = destination;

        float timeSpent = 0;

        float duration = 5;


        while (((!agent.pathPending && agent.remainingDistance > agent.stoppingDistance + 0.1f) || targetCreature) && timeSpent < duration)
        {
            timeSpent += Time.deltaTime;
            yield return null;
        }

        if(!targetCreature && Vector3.Distance(transform.position, player.position) < 10) StartCoroutine(WaitAround());

        isMoving = false;
    }

    IEnumerator WaterDrain()
    {
        yield break; //Disabled for now
        while(health > 0)
        {
            yield return new WaitForSeconds(1);
            if(TimeManager.Instance.isDay) continue;
            waterLevel -= waterLossRate;

            if(waterLevel < 25)
            {
                waterIcon.SetActive(true);
            }
            else waterIcon.SetActive(false);

            if(waterLevel <= 0)
            {
                TakeDamage(5);
                waterLevel = 0;
            }
        }
        waterIcon.SetActive(false);
    }

    public override bool OnBearTrapStun(StructureBehaviorScript b)
    {
        if(currentState == CreatureState.Trapped) return false;
        currentState = CreatureState.Trapped;
        StartCoroutine(BearTrapHold(b));
        agent.destination = transform.position;
        anim.SetBool("IsRunning", false);
        return false;
    }

    private IEnumerator BearTrapHold(StructureBehaviorScript b)
    {
        agent.ResetPath();
        float oldSpeed = agent.speed;
        agent.speed = 0;
        while (b && b.health > 0)
        {
            yield return null;
        }
        agent.speed = oldSpeed;
        currentState = CreatureState.Follow;
    }

    public override void OnDamage()
    {
        effectsHandler.OnHit();
    }

    public override void OnDeath()
    {
        StopAllCoroutines();
        currentState = CreatureState.Die;
        anim.SetTrigger("IsDead");
        agent.enabled = false;
        base.OnDeath();
        GameSaveData.Instance.manikkinsAlive--;
    }

    void OnDisable()
    {
        if(!isDead) GameSaveData.Instance.manikkinsAlive--;
    }

    private void Trapped()
    {
        rb.isKinematic = true;
    }

    public override void FogTeleport()
    {
        Destroy(this.gameObject);
    }

    public override void HitWithWater()
    {
        waterLevel = 100;
    }
}
