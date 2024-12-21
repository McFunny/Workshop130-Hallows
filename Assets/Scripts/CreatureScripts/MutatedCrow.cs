using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.PlacematContainer;

public class MutatedCrow : CreatureBehaviorScript
{
    // ============================
    // Enums
    // ============================
    public enum CreatureState
    {
        Idle,
        CirclePlayer,
        CirclePoint,
        AttackPlayer,
        Land,
        GoAway,
        Dead,
        GoEatCrop
    }

    // ============================
    // Public Variables
    // ============================
    public bool isAttackCrow;
    public CreatureState currentState;
    public bool isSummoned = false;
    public float circleRadius = 10f;
    public float height = 5f;
    public float attackHeight = 2f;
    public float speed = 5f;
    public float attackCooldown = 3f;
    public float rotationSpeed = 25f;
    public bool chooseRandomStats = false;
    public List<CropData> desiredCrops;
    public List<StructureBehaviorScript> availableStructure = new List<StructureBehaviorScript>();
    public ParticleSystem attackParticle;
    public ParticleSystem cropParticle;


    FarmLand foundFarmTile;

    // ============================
    // Serialized Fields
    // ============================
    [SerializeField] private LayerMask groundLayer;

    // ============================
    // Private Variables
    // ============================
    private float angle = 0f;
    private float savedAngle;
    private float timeBeforeAttack;
    private Vector3 point;
    private GameObject currentStructure;
    public bool coroutineRunning = false; 
    private bool canAttack = true;
    private bool isFleeing = false;
    private float minX = -10f;
    private float maxX = 10f;
    private StructureBehaviorScript targetStructure;
    private int numberOfAttacks = 0;
    private float circleDirection = 1f;


    // ============================
    // Unity Lifecycle Methods
    // ============================
    void Start()
    {
        base.Start();
        if (chooseRandomStats)
        {
            RandomizeStats();
        }
        StructureBehaviorScript.OnStructuresUpdated += UpdateStructureList;
        UpdateStructureList();
        targetStructure = null;
        currentState = CreatureState.Idle;
    }

    private void OnDisable()
    {
        StructureBehaviorScript.OnStructuresUpdated -= UpdateStructureList;
    }

    void Update()
    {
        if (currentState == CreatureState.CirclePlayer)
        {
            // Rotation clamp
            Vector3 rotation = transform.eulerAngles;
            rotation.x = Mathf.Clamp(rotation.x, minX, maxX);
            transform.eulerAngles = rotation;
        }
        CheckState(currentState);
    }

    // ============================
    // State Management
    // ============================
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
            case CreatureState.Land:
                Land();
                break;
            case CreatureState.GoAway:
                GoAway();
                break;
            case CreatureState.Dead:
                DeadBird();
                break;
            case CreatureState.GoEatCrop:
                Eat();
                break;
            default:
                Debug.LogError("Unknown state: " + currentState);
                break;
        }
    }

    // ============================
    // State Behavior Methods
    // ============================
    private void Idle()
    {
        rb.useGravity = true;
        float distance = Vector3.Distance(player.position, transform.position);
        playerInSightRange = distance <= sightRange;
        if (isSummoned)
        {
             rb.useGravity = false;
            rb.freezeRotation = false;

            if (isAttackCrow)
            {
                StartCoroutine(DoAttackCooldown(attackCooldown));
                currentState = CreatureState.CirclePlayer;
            }
            else
            {
                UpdateStructureList();
                GetRandomPoint(6);
                point.y = height;
                currentState = CreatureState.CirclePoint;
            }

            coroutineRunning = false;

        }
        else if (playerInSightRange)
        {
            rb.useGravity = false;
            rb.freezeRotation = false;

            if (isAttackCrow)
            {
                StartCoroutine(DoAttackCooldown(attackCooldown));
                currentState = CreatureState.CirclePlayer;
            }
            else
            {
                UpdateStructureList();
                GetRandomPoint(6);
                point.y = height;
                currentState = CreatureState.CirclePoint;
            }

            coroutineRunning = false;
        }

        else if (!coroutineRunning && !playerInSightRange)
        {
            StartCoroutine(IdleCoroutine());
        }
    }


    private void CircleAroundPlayer()
    {
        angle += circleDirection * (speed / circleRadius) * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;
        if (angle < 0f) angle += 360f; 

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circleRadius;
        Vector3 targetPosition = player.position + offset + Vector3.up * height;

        float distance = Vector3.Distance(transform.position, targetPosition);
        float t = (speed * Time.deltaTime) / distance;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp01(t));
        transform.LookAt(targetPosition);

        if (!coroutineRunning && canAttack)
        {
            StartCoroutine(DoesCrowAttack());
        }
    }

    private void CircleAroundPoint()
    {
        angle += circleDirection * (speed / circleRadius) * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;
        if (angle < 0f) angle += 360f; 

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * circleRadius;
        Vector3 targetPosition = point + offset;

        float distance = Vector3.Distance(transform.position, targetPosition);
        float t = (speed * Time.deltaTime) / distance;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp01(t));
        transform.LookAt(targetPosition);

        if (!coroutineRunning)
        {
            StartCoroutine(Decide());
        }
    }

    private void GoAway()
    {
        Vector3 targetPosition = point;
        float distance = Vector3.Distance(transform.position, targetPosition);
        float t = (speed * Time.deltaTime) / distance;
        transform.position = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp01(t));
        transform.LookAt(targetPosition);

        float playerDistance = Vector3.Distance(player.position, transform.position);
        if (playerDistance > 100f)
        {
            Destroy(this.gameObject);
        }
    }

    private void AttackPlayer()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(SwoopPlayer());
        }
    }

    private void Land()
    {
        if (IsGrounded())
        {
            Debug.Log("Bird has landed!");
            rb.useGravity = true;
            coroutineRunning = false;

            Vector3 rotation = transform.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;
            transform.eulerAngles = rotation;

            currentState = CreatureState.Idle;
        }
        else
        {
            Vector3 rotation = transform.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;
            transform.eulerAngles = rotation;

            Vector3 targetPosition = transform.position + Vector3.down * (speed * 0.5f) * Time.deltaTime;
            float distance = Vector3.Distance(transform.position, targetPosition);
            float t = ((speed * 0.5f) * Time.deltaTime) / distance;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp01(t));
        }
    }

    private void Eat()
    {
        if (Vector3.Distance(transform.position, targetStructure.transform.position) < 1f)
        {
            rb.useGravity = true;

            Vector3 rotation = transform.eulerAngles;
            rotation.x = 0;
            rotation.z = 0;
            transform.eulerAngles = rotation;

            if (!coroutineRunning)
            {
            StartCoroutine(EatTheCrop());
            }
        }
        else
        {
            Vector3 targetPosition = targetStructure.transform.position + Vector3.down * (speed * 0.5f) * Time.deltaTime;
            transform.LookAt(targetPosition);
            float distance = Vector3.Distance(transform.position, targetPosition);
            float t = ((speed * 0.5f) * Time.deltaTime) / distance;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Mathf.Clamp01(t));
        }
    }

    public override void OnDeath()
    {
        base.OnDeath();
        currentState = CreatureState.Dead;
        if (!isDead)
        {
            isDead = true;
            StopAllCoroutines();
            rb.useGravity = true;
        }
    }

    private void DeadBird()
    {}



    // ============================
    // Coroutines
    // ============================

    IEnumerator IdleCoroutine()
    {
        coroutineRunning = true;

        yield return new WaitForSeconds(5);
            int r = Random.Range(0, 4);
            switch (r)
            {
                case 0: // Wait
                    yield return new WaitForSeconds(5);
                    coroutineRunning = false;
                    break;

                case 1: // Wait longer
                    yield return new WaitForSeconds(10);
                    coroutineRunning = false;
                    break;

                case 2: // Circle the player
                    if (isAttackCrow)
                    {
                        rb.useGravity = false;
                        coroutineRunning = false;
                        StartCoroutine(DoAttackCooldown(attackCooldown));
                        currentState = CreatureState.CirclePlayer;
                        yield break; // Exit the coroutine
                    }
                    else
                    {
                        yield return new WaitForSeconds(5);
                        coroutineRunning = false;
                    }
                    break;

                case 3: // Find a crop or random point to circle
                    if (!isAttackCrow)
                    {
                        Vector3? cropPosition = FindCrop();
                        if (cropPosition.HasValue)
                        {
                            rb.useGravity = false;
                            point = cropPosition.Value + Vector3.up * height;
                            currentState = CreatureState.CirclePoint;
                            yield return new WaitForSeconds(7);
                            coroutineRunning = false;
                        }
                        else
                        {
                            rb.useGravity = false;
                            GetRandomPoint(6);
                            point.y = height;
                            currentState = CreatureState.CirclePoint;
                            coroutineRunning = false;

                        }

                        coroutineRunning = false;
                        yield break; // Exit the coroutine
                    }
                    else
                    {
                        yield return new WaitForSeconds(5);
                        coroutineRunning = false;
                    }
                    break;
            }
        
    }

    IEnumerator DoesCrowAttack()
    {
        coroutineRunning = true;
        int r = Random.Range(0, 3);
        if (r == 0)
        {
            currentState = CreatureState.AttackPlayer;
        }
        else
        {
            yield return new WaitForSeconds(3);
        }
        coroutineRunning = false;
    }

    IEnumerator EatTheCrop()
    {
        //
        // Needs animation for eating
        //
        Debug.Log("Crow is eating");
        coroutineRunning = true;

        while (true) 
        {
           
            float distance = Vector3.Distance(transform.position, player.position);

           
            if (distance <= sightRange)
            {
                rb.useGravity = false;
                GetRandomPoint(15);
                point.y = height;
                currentState = CreatureState.CirclePoint;
                coroutineRunning = false;
                yield break; 
            }

            // Handle crop damage logic
            if (targetStructure != null)
            {
                targetStructure.TakeDamage(2);

                if (targetStructure.health <= 0)
                {
                    rb.useGravity = false;
                    targetStructure = null;
                    GetRandomPoint(5);
                    point.y = height;
                    currentState = CreatureState.CirclePoint;
                    coroutineRunning = false;
                    yield break; 
                }
            }

            yield return new WaitForSeconds(2); 
        }
    }


    IEnumerator SwoopPlayer()
    {
        coroutineRunning = true;

        Vector3 abovePlayerPos = player.position + Vector3.up * attackHeight;
        Vector3 direction = (abovePlayerPos - transform.position).normalized;
        while (Vector3.Distance(transform.position, abovePlayerPos) > 0.5f)
        {
            float distance = Vector3.Distance(transform.position, abovePlayerPos);
            float t = (speed * Time.deltaTime) / distance;
            transform.position = Vector3.Lerp(transform.position, abovePlayerPos, Mathf.Clamp01(t));
            transform.LookAt(abovePlayerPos);
            yield return null;
        }

        // Post-swoop behavior
        Vector3 abovePlayerPosPostSwoop = player.position + Vector3.up * attackHeight;
        if (Vector3.Distance(transform.position, abovePlayerPosPostSwoop) < 1f)
        {
            PlayerInteraction.Instance.StaminaChange(-10);
        }

        Vector3 endPos = transform.position + direction * 10f + Vector3.up * height;
        while (Vector3.Distance(transform.position, endPos) > 1f)
        {
            float distance = Vector3.Distance(transform.position, endPos);
            float t = (speed * Time.deltaTime) / distance;
            transform.position = Vector3.Lerp(transform.position, endPos, Mathf.Clamp01(t));
            transform.LookAt(endPos);
            yield return null;
        }

        StartCoroutine(DoAttackCooldown(attackCooldown));
        numberOfAttacks++;
        if (numberOfAttacks >= 3)
        {
            coroutineRunning = false;
            numberOfAttacks = 0;
            StartCoroutine(Decide());
            GetRandomPoint(5);
            currentState = CreatureState.CirclePoint;
        }
        else
        {
            currentState = CreatureState.CirclePlayer;
            coroutineRunning = false;
        }
    }

    IEnumerator DoAttackCooldown(float cooldown)
    {
        canAttack = false;
        yield return new WaitForSeconds(cooldown);
        canAttack = true;
    }

    IEnumerator StartCirclingCrop(float cooldown)
    {
        coroutineRunning = true;
        yield return new WaitForSeconds(cooldown);
        coroutineRunning = false;
    }

    IEnumerator Flee()
    {
        coroutineRunning = true;
        isFleeing = true;
        yield return new WaitForSeconds(5);
        isFleeing = false;
        coroutineRunning = false;
    }

    private IEnumerator Decide()
    {
        coroutineRunning = true;
        //------------------------------------------------
        // If the crow is in attack mode
        //------------------------------------------------

        if (isAttackCrow)
        {
            int r = Random.Range(0, 7);
            switch (r)
            {
                case 0: //Keep doing what you are doing
                    yield return new WaitForSeconds(4);
                    break;
                case 1: //if player is near, go attack them, if not, go away
                    if (Vector3.Distance(transform.position, player.position) < 50)
                    {
                        currentState = CreatureState.CirclePlayer;
                    }
                    else
                    {
                        point = GetRandomPoint(150);
                        currentState = CreatureState.GoAway;
                    }
                    break;
                case 2: //go away
                    point = GetRandomPoint(150);
                    currentState = CreatureState.GoAway;
                    break;
                case 3: //circle a random point
                    point = GetRandomPoint(15);
                    currentState = CreatureState.CirclePoint;
                    yield return new WaitForSeconds(5);
                    break;
                case 4: //Land the bird
                    currentState = CreatureState.Land;
                    break;
                case 5: //Go crop mode and circle a point
                    isAttackCrow = false;
                    point = GetRandomPoint(15);
                    currentState = CreatureState.CirclePoint;
                    break;
                case 6: //go crop mode. If crop available circle it, if not circle a random point
                    isAttackCrow = false;
                    Vector3? cropPosition = FindCrop();
                    if (cropPosition.HasValue)
                    {
                        point = cropPosition.Value + Vector3.up * height;
                        currentState = CreatureState.CirclePoint;
                        yield return new WaitForSeconds(7);
                    }
                    else
                    {
                        point = GetRandomPoint(15);
                        currentState = CreatureState.CirclePoint;
                    }

                    break;
            }
            coroutineRunning = false;
        }

        //------------------------------------------------
        // If the crow is in crop mode
        //------------------------------------------------

        else if (!isAttackCrow)
        {
            if (targetStructure != null && Vector3.Distance(transform.position, player.position) > sightRange) //if has target crop go get it
            {
                Debug.Log("TargetStructure isnt null");
                currentState = CreatureState.GoEatCrop;
                coroutineRunning = false;
            }
            else
            {
                int r = Random.Range(0, 5);
                switch (r)
                {
                    case 0: //Stay in current point
                        yield return new WaitForSeconds(4);
                        break;
                    case 1: //Find a new point to circle
                        point = GetRandomPoint(15);
                        currentState = CreatureState.CirclePoint;
                        yield return new WaitForSeconds(6);
                        break;
                    case 2: //Find a crop and if none are available go attack mode
                        Vector3? cropPosition = FindCrop();
                        if (cropPosition.HasValue)
                        {
                            point = cropPosition.Value + Vector3.up * height;
                            currentState = CreatureState.CirclePoint;
                            yield return new WaitForSeconds(7);
                        }
                        else
                        {
                            isAttackCrow = true;
                            currentState = CreatureState.CirclePlayer;
                        }
                        break;
                    case 3: //Find a crop and if none are available go away
                        Vector3? cropPosition2 = FindCrop();
                        if (cropPosition2.HasValue)
                        {
                            point = cropPosition2.Value + Vector3.up * height;
                            currentState = CreatureState.CirclePoint;
                            yield return new WaitForSeconds(7);
                        }
                        else
                        {
                            point = GetRandomPoint(150);
                            currentState = CreatureState.GoAway;
                        }
                        break;
                    case 4: //attack player if they are near and if not go away
                        if (Vector3.Distance(transform.position, player.position) < 50)
                        {
                            isAttackCrow = true;
                            currentState = CreatureState.CirclePlayer;
                            yield return new WaitForSeconds(6);
                        }
                        else
                        {
                            point = GetRandomPoint(150);
                            currentState = CreatureState.GoAway;
                        }

                        break;
                }
                coroutineRunning = false;
            }
        }
    }

    // ============================
    // Utility Methods
    // ============================

    private void UpdateStructureList()
    {
        availableStructure.Clear();
        foreach (StructureBehaviorScript structure in structManager.allStructs)
        {
            FarmLand potentialFarmTile = structure as FarmLand;
            if (potentialFarmTile && desiredCrops.Contains(potentialFarmTile.crop))
            {
                availableStructure.Add(potentialFarmTile);
            }
        }

       
    }
    private bool IsGrounded()
    {
        RaycastHit hit;
        float rayDistance = 0.75f;

        // Check for ground layer
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance, groundLayer))
        {
            Debug.Log("Grounded on ground layer.");
            return true;
        }
        // Check separately for FarmLand tag
        else if (Physics.Raycast(transform.position, Vector3.down, out hit, rayDistance))
        {
            if (hit.collider.CompareTag("FarmLand"))
            {
                Debug.Log("Grounded on FarmLand!");
               
                return true; 
            }
        }

        return false;
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("scarecrow") && !isFleeing)
        {
            if (currentState == CreatureState.GoAway) return;
            StopAllCoroutines();
            coroutineRunning = false;

            StartCoroutine(Flee());

            Vector3 fleeDirection = (transform.position - other.transform.position).normalized;
            point = transform.position + fleeDirection * circleRadius * 2;
            point.y = height;

            currentState = CreatureState.CirclePoint;
        }
    }

    private Vector3? FindCrop()
    {
        if (availableStructure.Count == 0) return null;
        else if (availableStructure.Count > 0)
        {
            int r = Random.Range(0, availableStructure.Count);
            targetStructure = availableStructure[r];
        }

        return targetStructure.transform.position;
        
    }

    private void RandomizeStats()
    {
        circleRadius = Random.Range(5, 15);
        height = Random.Range(4, 10);
        speed = Random.Range(7, 10);
        attackCooldown = Random.Range(3, 15);
        circleDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
    }

    private Vector3 GetRandomPoint(float range)
    {
        float randomOffset1 = Random.Range(0, 2) == 0 ? range : -range;
        float randomOffset2 = Random.Range(0, 2) == 0 ? range : -range;
        return new Vector3(transform.position.x + randomOffset1, transform.position.y + 5, transform.position.z + randomOffset2);
    }
}
