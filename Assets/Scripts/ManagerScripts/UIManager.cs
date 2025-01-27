using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{

    int currentCoins = 0;
    public TextMeshProUGUI coinText;
    private Animator coinAnimator;
    // Start is called before the first frame update
    void Start()
    {
        coinAnimator = coinText.transform.parent.gameObject.GetComponent<Animator>();
        currentCoins = PlayerInteraction.Instance.currentMoney;
        coinText.text = currentCoins.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        if(currentCoins != PlayerInteraction.Instance.currentMoney)
        {
            coinAnimator.SetBool("MoneyChanging", true);
            coinAnimator.SetTrigger("MoneyUpdate");
            if(currentCoins < PlayerInteraction.Instance.currentMoney) { currentCoins++; }
            if(currentCoins > PlayerInteraction.Instance.currentMoney) { currentCoins--; }
             //= PlayerInteraction.Instance.currentMoney;
            coinText.text = currentCoins.ToString();
        }
        else
        {
            coinAnimator.SetBool("MoneyChanging", false);
        }
    }
}
