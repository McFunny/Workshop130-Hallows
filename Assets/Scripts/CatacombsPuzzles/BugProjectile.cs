using System.Collections;
using UnityEngine;

public class BugProjectile : MonoBehaviour
{
    [HideInInspector] public float shootingVelocity;

    private SphereCollider myCollider;
    private void OnTriggerEnter(Collider other)
    {
       /* if (other.CompareTag("Win1"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(true, shootingVelocity, 1);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Win2"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(true, shootingVelocity, 2);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Win3"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(true, shootingVelocity, 3);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Win4"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(true, shootingVelocity, 4);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Win5"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(true, shootingVelocity, 5);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Win6"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(true, shootingVelocity, 6);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Win7"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(true, shootingVelocity, 7);
            Destroy(gameObject);
        }
        else if (other.CompareTag("LoseZone"))
        {
            PachinkoManager.Instance.NotifyBugDestroyed(false, shootingVelocity, 0);
            Destroy(gameObject);
        }*/

        if (other.gameObject.GetComponent<PachinkoWinHole>() != null)
        {
            PachinkoWinHole winHole = other.gameObject.GetComponent<PachinkoWinHole>();
            winHole.Payout();
            StartCoroutine(ShrinkBall(other.gameObject));
        }
    }

    private void Start()
    {
        myCollider = GetComponent<SphereCollider>();
        StartCoroutine(DestroyBall());

    }

    IEnumerator DestroyBall()
    {
        yield return new WaitForSeconds(15f);
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.name == "BouncyNail")
        {
            Rigidbody rb = GetComponent<Rigidbody>();

            // Reflect the velocity off the hit surface to simulate a sharp bounce
            Vector3 bounceDir = Vector3.Reflect(rb.velocity, collision.contacts[0].normal).normalized;
            float bounceForce = rb.velocity.magnitude;

            rb.velocity = bounceDir * bounceForce;

            // Optional: Add a bit more force to exaggerate
            float randomForce = Random.Range(8, 12);
            rb.AddForce(collision.contacts[0].normal * randomForce, ForceMode.VelocityChange);
        }
    }

    IEnumerator ShrinkBall(GameObject gameObject)
    {
        myCollider.enabled = false;
        Vector3 currentPosition = transform.position;
        Vector3 endPosition = gameObject.transform.position;
        Vector3 currentScale = transform.localScale;
        Vector3 endingScale = transform.localScale / 10;
        float elapsedTime = 0;
        float totalDuration = 0.7f;

        while (elapsedTime < totalDuration)
        {
            float t = elapsedTime / totalDuration;

            transform.localScale = Vector3.Lerp(currentScale, endingScale, t);
            transform.position = Vector3.Lerp(currentPosition, endPosition, t);


            elapsedTime += Time.deltaTime;
            yield return null;
        }
       transform.localScale = endingScale;
        Destroy(this.gameObject);
    }

}
