using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetleBallProjectile : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;         
    public float bounceRandomness = 10f;   

    [Header("Visual Model")]
    public Transform model;     // Child object that should visually roll
    public float rollSpeed = 360f;  // Degrees per second at moveSpeed

    public LayerMask noBounceLayers;

    private Rigidbody rb;

    public AudioClip bounce, hitEnemy;

    bool justBounced = false;
    bool isBouncing;

    [Header("Homing Settings")]
    public float homingRadius = 15f;
    public float homingStrength = 3f; // low value = subtle nudge, higher = stronger pull

    private CreatureBehaviorScript homingTarget;

    public float damage = 25;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.drag = 0;
        rb.angularDrag = 0;

        // Prevent tumbling
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        Vector3 startVel = transform.forward * moveSpeed;
        rb.velocity = startVel;
        StartCoroutine(FindHomingTarget());
    }

    void OnEnable()
    {
        StartCoroutine(LifeTime());
        StartCoroutine(FindHomingTarget());
    }

    void OnDisable()
    {
        StopCoroutine(LifeTime());
    }

    private void FixedUpdate()
    {
        if (justBounced) { justBounced = false; return; }

        Vector3 horizontal = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (horizontal.sqrMagnitude > 0.001f)
            horizontal = horizontal.normalized * moveSpeed;
        else
            horizontal = transform.forward * moveSpeed;

        // Apply homing nudge toward target
        if (homingTarget != null && !homingTarget.isDead)
        {
            Vector3 toTarget = (homingTarget.transform.position - transform.position);
            toTarget.y = 0;
            toTarget = toTarget.normalized;

            Vector3 currentDir = horizontal.normalized;

            // Only home if ball is moving toward the target (dot > 0 = same general direction)
            // Increase the threshold (e.g. 0.5f) to require a more direct angle
            if (Vector3.Dot(currentDir, toTarget) > 0.3f)
            {
                horizontal = Vector3.Lerp(currentDir, toTarget,
                            homingStrength * Time.fixedDeltaTime).normalized * moveSpeed;
            }
        }

        rb.velocity = new Vector3(horizontal.x, rb.velocity.y, horizontal.z);

        // --- Rotate model to appear as rolling ---
        if (model != null)
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.sqrMagnitude > 0.001f)
            {
                float distanceThisFrame = flatVel.magnitude * Time.fixedDeltaTime;

                // Rotate around axis perpendicular to movement
                Vector3 rollAxis = Vector3.Cross(flatVel.normalized, Vector3.up);

                model.Rotate(rollAxis, (distanceThisFrame / 1f) * rollSpeed, Space.World);
            }
        }
    }

    IEnumerator FindHomingTarget()
    {
        while (gameObject.activeSelf)
        {
            yield return new WaitForSeconds(0.5f); // re-evaluate every half second

            if (homingTarget != null && !homingTarget.isDead) continue; // keep current target

            CreatureBehaviorScript nearest = null;
            float nearestDist = homingRadius;

            Collider[] hits = Physics.OverlapSphere(transform.position, homingRadius, 1 << 9);
            foreach (Collider hit in hits)
            {
                CreatureBehaviorScript creature = hit.GetComponentInParent<CreatureBehaviorScript>();
                if (creature == null || creature.isDead || !creature.shovelVulnerable) continue;

                float dist = Vector3.Distance(transform.position, creature.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = creature;
                }
            }

            homingTarget = nearest;
        }
    }

    private void Bounce(Vector3 approxNormal)
    {
        if (isBouncing) return; // prevent corner multi-bounce in single frame
        isBouncing = true;
        justBounced = true;

        // Get current horizontal direction
        Vector3 incoming = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;

        // Reverse the direction entirely (bounce back the way it came)
        Vector3 reversed = -incoming;

        // Add slight random variance so it doesn't perfectly retrace
        float randomAngle = Random.Range(-bounceRandomness, bounceRandomness);
        reversed = Quaternion.AngleAxis(randomAngle, Vector3.up) * reversed;

        // If the reversed direction would still push into the surface, nudge it out
        if (Vector3.Dot(reversed, approxNormal) < 0)
            reversed = Vector3.Reflect(reversed, approxNormal).normalized;

        rb.velocity = new Vector3(reversed.x * moveSpeed, rb.velocity.y, reversed.z * moveSpeed);

        // Physically push the ball out of the surface to prevent corner sticking
        transform.position += approxNormal * 0.15f;

        StartCoroutine(ResetBounceLock());
    }

    IEnumerator ResetBounceLock()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate(); // two ticks to fully clear the collider
        isBouncing = false;
    }

    // --- COLLISION SUPPORT ---
    private void OnCollisionEnter(Collision collision)
    {
        int otherLayer = collision.gameObject.layer;

        if(otherLayer == 9)
        {
            var creature = collision.gameObject.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                CollideWithEnemy(creature);
            }
        }

        // If the layer is inside the no-bounce layer mask → skip bounce
        if ((noBounceLayers.value & (1 << otherLayer)) != 0)
            return;

        if (collision.contacts.Length == 0) return;

        Vector3 normal = Vector3.zero;
        foreach (ContactPoint contact in collision.contacts) normal += contact.normal;
        normal = normal.normalized;

        //Vector3 normal = collision.contacts[0].normal;
        ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = transform.position;
        AudioPoolManager.Instance.PlayClipAtPosition(bounce, transform.position, 0.5f, 30);
        Bounce(normal);
    }

    // --- TRIGGER SUPPORT ---
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 9)
        {
            var creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                CollideWithEnemy(creature);
            }
            else return;
        }
        else if(other.gameObject.layer != 14) return; //that way it does not collider with trigger colliders on structures

        // Approximate the surface normal manually
        Vector3 directionToOther = (transform.position - other.ClosestPoint(transform.position)).normalized;

        // If the collider doesn’t support ClosestPoint (rare), fallback
        if (directionToOther == Vector3.zero)
            directionToOther = (transform.position - other.transform.position).normalized;

        Bounce(directionToOther);
    }

    void CollideWithEnemy(CreatureBehaviorScript c)
    {
        c.TakeDamage(damage);
        AudioPoolManager.Instance.PlayClipAtPosition(hitEnemy, transform.position, 0.5f, 30);
        ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = c.transform.position;
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(Random.Range(60f, 90f));
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        AudioPoolManager.Instance.PlayClipAtPosition(bounce, transform.position, 0.5f, 30);
        //gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
