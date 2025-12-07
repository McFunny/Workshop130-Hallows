using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class RadarHandler : MonoBehaviour
{
    private SphereCollider radarCollider;
    private float minSize = 0.5f;
    private float circleMax = 10f;
    private float maxSize;
    private float cooldown = 1f;
    private Coroutine scan;
    [HideInInspector] public GameObject circleImage;
    private Vector3 circleScale = new Vector3();

    void Start()
    {
        maxSize = NutrientTesterScript.Instance.radarRange;;

        radarCollider = GetComponent<SphereCollider>();
        //radarCollider.center = new Vector3(0, radarRange / 2f, radarRange / 2f);
        radarCollider.radius = minSize;
        radarCollider.isTrigger = true;
        radarCollider.includeLayers = NutrientTesterScript.Instance.include;
        radarCollider.excludeLayers = NutrientTesterScript.Instance.exclude;
    }

    void OnEnable()
    {
        if(radarCollider == null) radarCollider = GetComponent<SphereCollider>();
        radarCollider.enabled = true;
        scan = StartCoroutine(Scan());  
    }

    void OnDisable()
    {
        StopCoroutine(scan);
        radarCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        NutrientTesterScript.Instance.TriggerEnter(other);
    }

    private IEnumerator Scan()
    {
        while (true)
        {
            while (radarCollider.radius < maxSize)
            {
                radarCollider.radius = Mathf.MoveTowards(radarCollider.radius, maxSize, NutrientTesterScript.Instance.rotationSpeed * Time.deltaTime);
                var scanPercentage = radarCollider.radius / maxSize;

                var scanSize = Mathf.Lerp(minSize, circleMax, scanPercentage);

                circleScale = new Vector3(scanSize, scanSize, scanSize);
                circleImage.transform.localScale = circleScale;

                yield return null;
            }

            radarCollider.radius = minSize;
            yield return new WaitForSeconds(cooldown);
            
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NutrientTesterScript.Instance.TriggerExit(other);
    }
}
