using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class RadarHandler : MonoBehaviour
{
    private BoxCollider radarCollider;

    void Start()
    {
        var radarRange = NutrientTesterScript.Instance.radarRange;

        radarCollider = GetComponent<BoxCollider>();
        radarCollider.center = new Vector3(0, radarRange / 2f, radarRange / 2f);
        radarCollider.size = new Vector3(1f, radarRange, radarRange);
        radarCollider.isTrigger = true;
        radarCollider.includeLayers = NutrientTesterScript.Instance.include;
        radarCollider.excludeLayers = NutrientTesterScript.Instance.exclude;
    }

    void Update()
    {
        //creatures = WildernessManager.Instance.allCreatures.Concat(NightSpawningManager.Instance.allCreatures).ToList();
    }

    private void OnTriggerEnter(Collider other)
    {
        NutrientTesterScript.Instance.TriggerEnter(other);
    }

    private void OnTriggerExit(Collider other)
    {
        NutrientTesterScript.Instance.TriggerExit(other);
    }
}
