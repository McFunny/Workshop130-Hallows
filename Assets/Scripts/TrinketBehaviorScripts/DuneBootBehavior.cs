using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trinket Behavior/Dune Boots")]
public class DuneBootBehavior : TrinketBehavior
{
    public float speedMod = 0.7f;
    public float duration = 1f;

    public override void TriggerEffect(out float durabilityCost)
    {
        durabilityCost = 2;
        PlayerInteraction.Instance.StartCoroutine(BurrowPenalty());
    }


    public IEnumerator BurrowPenalty()
    {
        PlayerMovement.Instance.ApplySpeedMod(new MovementSpeedModifiers(TrinketInventoryHandler.Instance.gameObject, speedMod, "DuneBoots", true));
        yield return new WaitForSeconds(duration);
        PlayerMovement.Instance.RemoveSpeedMod("DuneBoots");
    }
}
