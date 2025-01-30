using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropCrow : CreatureBehaviorScript
{
    #region Enums
    public enum CreatureState
    {
        Idle,
        FlyToCrop,
        CircleCrop,
        LandNextToCrop,
        AttackCrop,
        Flee,
        GoAway,
        Die,
    }

    public CreatureState currentState;
    #endregion

    #region Variables
    public List<CropData> desiredCrops; // Crops the crow targets
    private FarmLand currentFarmTile;
    private Vector3 point; // Destination point for various behaviors
    public float radius = 10f;
    public float height = 5f;
    public float circleSpeed = 2f;
    private float angle = 0f;
    private bool coroutineRunning = false;
    private int cropsEaten = 0;
    public int maxCropsToEat = 3;
    public float scareRange = 5f; // Range to detect player
    private bool isFleeing = false;

    #endregion

    #region Unity Methods

    void Start()
    {
        base.Start();
        currentState = CreatureState.Idle;
    }

    void Update()
    {
        if (isDead) return;

        // Check for player proximity
        float playerDistance = Vector3.Distance(player.position, transform.position);
        if (playerDistance <= scareRange)
        {
            currentState = CreatureState.Flee;
        }

        CheckState(currentState);
    }

    #endregion

    #region State Management

    private void CheckState(CreatureState state)
    {
        switch (state)
        {
            case CreatureState.Idle:
                Idle();
                break;
            case CreatureState.FlyToCrop:
                FlyToCrop();
                break;
            case CreatureState.CircleCrop:
                CircleCrop();
                break;
            case CreatureState.LandNextToCrop:
                LandNextToCrop();
                break;
            case CreatureState.AttackCrop:
                AttackCrop();
                break;
            case CreatureState.Flee:
                Flee();
                break;
            case CreatureState.GoAway:
                GoAway();
                break;
            case CreatureState.Die:
                Die();
                break;
        }
    }

    #endregion

    #region State Behaviors

    private void Idle()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(FindCrop());
        }
    }

    private void FlyToCrop()
    {
        if (currentFarmTile == null)
        {
            currentState = CreatureState.Idle;
            return;
        }

        Vector3 targetPosition = currentFarmTile.transform.position;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.3f);
        transform.LookAt(currentFarmTile.transform.position);

        if (Vector3.Distance(transform.position, targetPosition) <= 1f)
        {
            currentState = CreatureState.LandNextToCrop;
        }
    }

    private void CircleCrop()
    {
        if (currentFarmTile == null)
        {
            currentState = CreatureState.Idle;
            return;
        }

        angle += circleSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        Vector3 targetPosition = currentFarmTile.transform.position + offset + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * circleSpeed);
        transform.LookAt(currentFarmTile.transform.position);

        if (!coroutineRunning)
        {
            StartCoroutine(DecideToLand());
        }
    }

    private void LandNextToCrop()
    {
        Vector3 targetPosition = currentFarmTile.transform.position + (transform.right * 2f);
        targetPosition.y = 0.1f;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.3f);
        transform.LookAt(currentFarmTile.transform.position);

        if (Vector3.Distance(transform.position, targetPosition) <= 0.5f)
        {
            currentState = CreatureState.AttackCrop;
        }
    }

    private void AttackCrop()
    {
        if (currentFarmTile == null || currentFarmTile.crop == null)
        {
            currentState = CreatureState.Idle;
            return;
        }

        if (!coroutineRunning)
        {
            StartCoroutine(EatCrop());
        }
    }

    private void Flee()
    {
        if (isFleeing) return;

        isFleeing = true;
        StopAllCoroutines();

        Vector3 fleeDirection = (transform.position - player.position).normalized;
        point = transform.position + fleeDirection * radius * 2;
        point.y = height;

        StartCoroutine(FleeToSafety());
    }

    private void GoAway()
    {
        // Move toward the point without modifying it
        angle += circleSpeed * Time.deltaTime;
        if (angle >= 360f) angle -= 360f;

        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        Vector3 targetPosition = point + offset + Vector3.up * height;

        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * circleSpeed);
        transform.LookAt(targetPosition);

        if (Vector3.Distance(player.position, transform.position) > 100f)
        {
            Destroy(gameObject);
        }
    }

    private void Die()
    {
        // Handle death logic here
        Debug.Log("Crow has died.");
    }

    #endregion

    #region Helper Methods

    private IEnumerator FindCrop()
    {
        coroutineRunning = true;

        List<FarmLand> availableLands = new List<FarmLand>();
        foreach (StructureBehaviorScript structure in StructureManager.Instance.allStructs)
        {
            FarmLand potentialFarmTile = structure as FarmLand;
            if (potentialFarmTile && desiredCrops.Contains(potentialFarmTile.crop))
            {
                availableLands.Add(potentialFarmTile);
            }
        }

        if (availableLands.Count > 0)
        {
            int r = Random.Range(0, availableLands.Count);
            currentFarmTile = availableLands[r];
            currentState = CreatureState.FlyToCrop;
        }
        else
        {
            SetPointForGoAway();
            currentState = CreatureState.GoAway;
        }

        coroutineRunning = false;
        yield return null;
    }

    private IEnumerator DecideToLand()
    {
        coroutineRunning = true;
        yield return new WaitForSeconds(3f); // Circle for a few seconds
        currentState = CreatureState.LandNextToCrop;
        coroutineRunning = false;
    }

    private IEnumerator EatCrop()
    {
        coroutineRunning = true;
        yield return new WaitForSeconds(3f); // Time to eat the crop

        if (currentFarmTile && currentFarmTile.crop)
        {
            currentFarmTile.CropDestroyed();
            cropsEaten++;

            if (cropsEaten >= maxCropsToEat || currentFarmTile == null)
            {
                SetPointForGoAway();
                currentState = CreatureState.GoAway;
            }
            else
            {
                currentState = CreatureState.CircleCrop;
            }
        }

        coroutineRunning = false;
    }

    private IEnumerator FleeToSafety()
    {
        while (Vector3.Distance(transform.position, point) > 1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, point, 0.3f);
            transform.LookAt(point);
            yield return null;
        }

        isFleeing = false;
        currentState = CreatureState.Idle;
    }

    private void SetPointForGoAway()
    {
        Vector3 farOffset = new Vector3(
            Random.Range(150f, 200f) * (Random.Range(0, 2) == 0 ? -1 : 1),
            20f,
            Random.Range(150f, 200f) * (Random.Range(0, 2) == 0 ? -1 : 1)
        );

        point = transform.position + farOffset;
    }

    #endregion
}
