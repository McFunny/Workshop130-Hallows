using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MistMerchantTownTrigger : MonoBehaviour
{
    public WagonMerchantNPC merchant;
    void OnTriggerEnter(Collider other)
    {
        merchant.PlayerEnteredTown();
    }
}
