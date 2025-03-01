﻿using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

// Abner To DO: Have it fly lower, make sure ontriggerenter works, Use transform.move + transform.lookat to have it land on the ground, When it dies, have it fall straight to the ground then destroy itself
// Cam To DO: Code it stealing items sometimes, 

public class Crow : CreatureBehaviorScript
{
    #region Enums
    public enum CreatureState
    {
        Idle,
        Flee,
        CirclePlayer,
        CirclePoint,
        AttackPlayer,
        Stun,
        Land,
        Die,
        Trapped,
        Wait,
        GoAway
    }
    #endregion

    #region Variables
    public List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();
    private StructureBehaviorScript scaryStructure;

    public float radius = 10f;
    private float height = 5f;
    public float attackHeight = 3f;
    public float circleSpeed = 2f;
    private float angle = 0f;
    private bool coroutineRunning = false;
    private float timeBeforeAttack;
    private float savedAngle;
    private GameObject currentStructure;
    [SerializeField] private Collider attackCollider;
    public CreatureState currentState;
    public bool isSummoned = false;
    private Vector3 point;
    #endregion

    #region Unity Methods
    private void Start()
    {
        base.Start();
        attackCollider.enabled = false;
        if (isSummoned)
        {
            if (Vector3.Distance(transform.position, player.position) < 50)
            {
                currentState = CreatureState.CirclePlayer;
            }

            else
            {
                int randomOffset1 = Random.Range(0, 2) == 0 ? 10 : -10;
                int randomOffset2 = Random.Range(0, 2) == 0 ? 10 : -10;

                point = new Vector3(transform.position.x + randomOffset1, transform.position.y + 20, transform.position.z + randomOffset2);
                currentState = CreatureState.CirclePoint;
            }
        }
        else
        {
            currentState = CreatureState.Idle;
        }
        
        timeBeforeAttack = Random.Range(5, 10);
    }

    private void OnDestroy()
    {
        //StructureBehaviorScript.OnStructuresUpdated -= UpdateStructureList;
    }

    private void Update()
    {
        
        if (currentState != CreatureState.Land && currentState != CreatureState.Idle && currentState != CreatureState.AttackPlayer)
        {
            height = Mathf.Clamp(height, 5f, 15f);
        }

        CheckState(currentState);
    }

    #endregion

    #region State Checking


    public void CheckState(CreatureState currentState)
    {
        switch (currentState)
        {
            case CreatureState.Idle:
                Idle();
                break;
            case CreatureState.CirclePlayer:
                CircleAroundPlayer();
                break;
            case CreatureState.CirclePoint:
                CircleAroundPoint();
                break;
            case CreatureState.AttackPlayer:
                AttackPlayer();
                break;
            case CreatureState.Flee:
                Flee();
                break;
            case CreatureState.Stun:
                Stun();
                break;
            case CreatureState.Land:
                Land();
                break;
            case CreatureState.Die:
                Die();
                break;
            case CreatureState.Trapped:
                Trapped();
                break;
            case CreatureState.Wait:
                Wait();
                break;
            case CreatureState.GoAway:
                GoAway();
                break;
            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

   
    #endregion

    #region State Functions
    private void Idle()
    {
        rb.useGravity = true;
        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        if (playerInSightRange)
        {
            rb.useGravity = false;
            currentState = CreatureState.CirclePlayer;
        }
    }

    private void CircleAroundPlayer()
    {
       
        angle += circleSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        height = Mathf.PingPong(Time.time * 2, 10f) + 5f;
        Vector3 targetPosition = player.position + offset + Vector3.up * height;

        if (Vector3.Distance(player.position, transform.position) > radius * 2)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.1f);
            transform.LookAt(targetPosition);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * circleSpeed);
            transform.LookAt(targetPosition);
        }
            if (!coroutineRunning)
        {
            StartCoroutine(Decide());
        }
    }


    private void CircleAroundPoint()
    {

        angle += circleSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        height = Mathf.PingPong(Time.time * 2, 10f) + 5f;
        Vector3 targetPosition = point + offset + Vector3.up * height;

        if (Vector3.Distance(point, transform.position) > radius * 2)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.3f);
            transform.LookAt(targetPosition);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * circleSpeed);
            transform.LookAt(targetPosition);
        }
        if (!coroutineRunning)
        {
            StartCoroutine(Decide());
        }
    }

    private void AttackPlayer()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(Attack());
        }
    }

    private void Flee()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(FleeFromStructure());
        }
    }

    private void Stun()
    {
        throw new NotImplementedException();
    }

    private void Land()
    {
        if (coroutineRunning)
        {
            StopCoroutine(Decide());
            coroutineRunning = false;
        }

        if (point == null)
        {
        point = transform.position;
        }

        angle += circleSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * (radius / 2);
        height = -1;
        point = new Vector3(point.x, 0, point.z);
        Vector3 targetPosition = point + offset + Vector3.up * height;

        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 0);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * (circleSpeed / 2));

        Vector3 down = Vector3.down;
        RaycastHit hit;
        Debug.DrawRay(transform.position, down, Color.yellow);
        if (Physics.Raycast(transform.position, down, out hit, 0.1f, LayerMask.GetMask("Ground")))
        {
            currentState = CreatureState.Idle;
        }
    }


    private void Die()
    {
        Destroy(this.gameObject);
    }

    private void Trapped()
    {
        throw new NotImplementedException();
    }

    private void Wait() { }

    private void GoAway()
    {
        angle += circleSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        height = Mathf.PingPong(Time.time * 2, 10f) + 5f;
        Vector3 targetPosition = point + offset + Vector3.up * height;

        if (Vector3.Distance(point, transform.position) > radius * 2)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime);
            transform.LookAt(targetPosition);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * circleSpeed);
            transform.LookAt(targetPosition);
        }

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance > 100f)
        {
            Destroy(this.gameObject);
        }
    }


    #endregion

    #region Helper Functions


    private void UpdateStructureList()
    {
        availableStructure.Clear();
        foreach (StructureBehaviorScript structure in structManager.allStructs)
        {
            if (structure is ImbuedScarecrow scarecrow)
            {
                availableStructure.Add(scarecrow);
            }
        }
    }

    /*private void OnTriggerStay(Collider other)
    {
       
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("HIT PLAYER");
                PlayerInteraction playerInteraction = PlayerInteraction.Instance;
                if (playerInteraction != null)
                {
                    playerInteraction.StaminaChange(-10);
                    int randomOffset1 = Random.Range(0, 2) == 0 ? 150 : -150;
                    int randomOffset2 = Random.Range(0, 2) == 0 ? 150 : -150;

                    point = new Vector3(transform.position.x + randomOffset1, transform.position.y + 20, transform.position.z + randomOffset2);
                    currentState = CreatureState.GoAway;
                }
            }
        

        else if (other.CompareTag("scarecrow") && (currentState != CreatureState.CirclePoint && currentState != CreatureState.Flee))
        {
            Debug.Log("this shouldnt be running");
            currentStructure = other.gameObject;
            StopAllCoroutines();
            coroutineRunning = false;
            currentState = CreatureState.Flee;
        }
    } */


    private void OnTriggerEnter(Collider other)
    {
        
            Debug.Log("HIT PLAYER");
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                Debug.Log("HIT PLAYER");
                PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
                if (playerInteraction != null)
                {
                    playerInteraction.StaminaChange(-10);
                    int randomOffset1 = Random.Range(0, 2) == 0 ? 150 : -150;
                    int randomOffset2 = Random.Range(0, 2) == 0 ? 150 : -150;

                    point = new Vector3(transform.position.x + randomOffset1, transform.position.y + 20, transform.position.z + randomOffset2);
                    currentState = CreatureState.GoAway;
                }
            }
        

        else if (other.CompareTag("scarecrow") && (currentState != CreatureState.CirclePoint || currentState != CreatureState.Flee))
        {
            currentStructure = other.gameObject;
            StopAllCoroutines();
            coroutineRunning = false;
            currentState = CreatureState.Flee;
        }
    }


    #endregion

    #region Coroutines
    private IEnumerator FleeFromStructure()
    {
        coroutineRunning = true;
        rb.useGravity = false;
        Vector3 fleeDirection = (transform.position - currentStructure.transform.position).normalized;
        fleeDirection.y = 0;
    
        point = transform.position + fleeDirection * radius * 2;
        point = new Vector3(point.x, 15, point.z);

        Debug.Log("Creature Fleeing");

        currentState = CreatureState.CirclePoint;
        coroutineRunning = false;
        yield return null;
    }




    private IEnumerator Attack()
    {
        coroutineRunning = true;
        attackCollider.enabled = true;
        Vector3 targetPos = player.position + Vector3.up * attackHeight;
        float swoopSpeed = circleSpeed * 5;
        float rotationSpeed = 5f;

        while (Vector3.Distance(transform.position, targetPos) > 1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, swoopSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(targetPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }

        Vector3 directionToPlayer = (transform.position - player.position).normalized;
        savedAngle = Mathf.Atan2(directionToPlayer.z, directionToPlayer.x);
        float oppositeAngle = savedAngle + Mathf.PI;
        Vector3 offset = new Vector3(Mathf.Cos(oppositeAngle), 0, Mathf.Sin(oppositeAngle)) * radius;
        Vector3 endPos = player.position + offset + Vector3.up * height;

        while (Vector3.Distance(transform.position, endPos) > 1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, swoopSpeed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(endPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            yield return null;
        }
        attackCollider.enabled = false;
        angle = oppositeAngle;
        timeBeforeAttack = Random.Range(5, 10);
        currentState = CreatureState.CirclePlayer;
        Debug.Log("Completed Attack");
        coroutineRunning = false;
      
        
    }

    private IEnumerator Decide()
    {
        coroutineRunning = true;
        yield return new WaitForSeconds(3);
        int x = Random.Range(0, 3);

        if (currentState != CreatureState.Land)
        {
            if (x == 0)
            {
                if (currentState == CreatureState.CirclePlayer)
                {
                    currentState = CreatureState.AttackPlayer;
                }
                else if (currentState == CreatureState.CirclePoint)
                {
                    int y = Random.Range(0, 2);
                    if (y == 0 && Vector3.Distance(transform.position, player.position) < 50)
                    {
                        currentState = CreatureState.CirclePlayer;
                    }
                    else
                    {
                        int randomOffset1 = Random.Range(0, 2) == 0 ? 150 : -150;
                        int randomOffset2 = Random.Range(0, 2) == 0 ? 150 : -150;

                        point = new Vector3(transform.position.x + randomOffset1, transform.position.y, transform.position.z + randomOffset2);
                        currentState = CreatureState.GoAway;
                    }
                }
            }
        }

        coroutineRunning = false;
    }

    #endregion
}
