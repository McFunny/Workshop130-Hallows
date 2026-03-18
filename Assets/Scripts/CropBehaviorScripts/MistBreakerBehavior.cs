using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crop Behavior", menuName = "Crop Behavior/MistBreaker")]
public class MistBreaker : CropBehavior
{
    public GameObject beamParticle, decalObject;
    public override void OnFullyGrown(FarmLand tile)
    {
        NightSpawningManager.Instance.FinaleComplete();
        tile.BecomeInvincible();
        GameObject beam = Instantiate(beamParticle, new Vector3(tile.transform.position.x, tile.transform.position.y + 1, tile.transform.position.z), Quaternion.identity);
        Instantiate(decalObject, tile.transform.position, Quaternion.identity);
        NightSpawningManager.Instance.StartCoroutine(FadeAway(beam, tile));
    }

    public override void OnCropDestroyed(FarmLand tile)
    {
        Debug.Log("Finale Turned Off");
        NightSpawningManager.Instance.DeactivateFinale();
    }

    public override void OnHour(FarmLand tile)
    {
        //Call Creatures to this
    }

    IEnumerator FadeAway(GameObject beam, FarmLand tile)
    {
        yield return new WaitForSeconds(8);
        beam.SetActive(false);
        if(tile) tile.gameObject.SetActive(false);
    }
}
