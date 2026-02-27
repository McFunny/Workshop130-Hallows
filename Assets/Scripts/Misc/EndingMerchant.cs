using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class EndingMerchant : MonoBehaviour
{
    bool triggered;

    public Transform moveBackPoint, focalPoint;

    void OnTriggerEnter(Collider other)
    {
        if(triggered) return;
        triggered = true;
        StartCoroutine(MoveBack());
    }

    IEnumerator MoveBack()
    {
        PlayerMovement.restrictMovementTokens++;
        PlayerCam.Instance.NewObjectOfInterest(focalPoint.position);
        yield return new WaitForSeconds(3);
        transform.DOMove(moveBackPoint.position, 2.5f);
        yield return new WaitForSeconds(3);
        PlayerMovement.restrictMovementTokens--;
        PlayerCam.Instance.ClearObjectOfInterest();
    }
}
