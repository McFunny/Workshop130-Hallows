using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    int currentCoins = 0;
    public TextMeshProUGUI coinText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentCoins != PlayerInteraction.Instance.currentMoney)
        {
            currentCoins = PlayerInteraction.Instance.currentMoney;
            coinText.text = "X " + currentCoins;
        }
    }
}
