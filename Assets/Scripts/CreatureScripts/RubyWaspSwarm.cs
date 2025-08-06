using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubyWaspSwarm : CreatureBehaviorScript
{
    public List<GameObject> wasps = new List<GameObject>();

    public int waspMin, waspMax;
    public GameObject waspPrefab;

    // Start is called before the first frame update
    void Start()
    {
        base.Start();

        int waspsToSpawn = Random.Range(waspMin, waspMax + 1);

        for(int i = 0; i < waspsToSpawn; i++)
        {
            RubyWasp wasp = Instantiate(waspPrefab, transform.position, Quaternion.identity).GetComponentInParent<RubyWasp>();
            wasp.homeSwarm = this;
            wasps.Add(wasp.gameObject);
        }

        StartCoroutine(MoveAround());
    }

    IEnumerator MoveAround()
    {
        while(wasps.Count > 0)
        {
            yield return new WaitForSeconds(Random.Range(10, 20));
            transform.position = StructureManager.Instance.GetRandomNearbyTile(GridType.Farm, 25, transform.position);
        }
        Destroy(gameObject);
    }
}
