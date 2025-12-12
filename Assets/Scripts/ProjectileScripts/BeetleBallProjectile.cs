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
        // Set initial horizontal velocity (keeps Y = 0)
        Vector3 startVel = transform.forward * moveSpeed;
        rb.velocity = startVel;

        StartCoroutine(LifeTime());
    }

    void OnEnable()
    {
        StartCoroutine(LifeTime());
    }

    void OnDisable()
    {
        StopCoroutine(LifeTime());
    }

    private void FixedUpdate()
    {
        // Maintain constant horizontal speed while allowing gravity
        Vector3 horizontal = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (horizontal.sqrMagnitude > 0.001f)
            horizontal = horizontal.normalized * moveSpeed;

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

    private void Bounce(Vector3 approxNormal)
    {
        // Current horizontal direction
        Vector3 incoming = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;

        // Reflect using the normal
        Vector3 reflected = Vector3.Reflect(incoming, approxNormal).normalized;

        // Add randomness
        float randomAngle = Random.Range(-bounceRandomness, bounceRandomness);
        reflected = Quaternion.AngleAxis(randomAngle, Vector3.up) * reflected;

        // Apply new velocity, keeping the current vertical velocity
        rb.velocity = new Vector3(reflected.x * moveSpeed, rb.velocity.y, reflected.z * moveSpeed);
    }

    // --- COLLISION SUPPORT ---
    private void OnCollisionEnter(Collision collision)
    {
        int otherLayer = collision.gameObject.layer;

        // If the layer is inside the no-bounce layer mask → skip bounce
        if ((noBounceLayers.value & (1 << otherLayer)) != 0)
            return;

        if (collision.contacts.Length == 0) return;

        Vector3 normal = collision.contacts[0].normal;
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
                creature.TakeDamage(10);
                AudioPoolManager.Instance.PlayClipAtPosition(hitEnemy, transform.position, 0.5f, 30);
                ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = creature.transform.position;
            }
            else return;
        }
        else if(other.gameObject.layer != 14) return;

        // Approximate the surface normal manually
        Vector3 directionToOther = (transform.position - other.ClosestPoint(transform.position)).normalized;

        // If the collider doesn’t support ClosestPoint (rare), fallback
        if (directionToOther == Vector3.zero)
            directionToOther = (transform.position - other.transform.position).normalized;

        Bounce(directionToOther);
    }

    IEnumerator LifeTime()
    {
        yield return new WaitForSeconds(20);
        ParticlePoolManager.Instance.GrabDirtPixelParticle().transform.position = transform.position;
        AudioPoolManager.Instance.PlayClipAtPosition(bounce, transform.position, 0.5f, 30);
        //gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
