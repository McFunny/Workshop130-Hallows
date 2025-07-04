using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BarterSign : MonoBehaviour
{
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI tradeDetails;

    public ParticleSystem changeParticle;
    public Rigidbody signRB;

    private void Start()
    {
        itemName.text = "";
        tradeDetails.text = "";
    }


    public void ResetDisplay()
    {
        itemName.text = "";
        tradeDetails.text = "";
        if(changeParticle) changeParticle.Play();
    
    }

    public void LeaveShop()
    {
        itemName.text = "";
        tradeDetails.text = "";
        if(changeParticle) changeParticle.Play();
    }

    public void DisplayTrade(StoreItem storeItem)
    {
        if(storeItem.barterCost.Count == 0)
        {
            ResetDisplay();
            return;
        }

        itemName.text = storeItem.itemData.displayName;
        //tradeDetails.text = itemData.description;
        string barterString = "Required item(s) to complete trade: ";
        for(int i = 0; i < storeItem.barterCost.Count; i++)
        {
            if(i != 0) barterString +=", ";
            barterString += storeItem.barterCost[i].amount + " " + storeItem.barterCost[i].item.displayName;
        }

        tradeDetails.text = barterString;

        if(changeParticle) changeParticle.Play();
        if(signRB) signRB.AddForce(signRB.transform.forward * -60, ForceMode.Impulse);
    }

}
