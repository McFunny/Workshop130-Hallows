using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    private int currentCoins = 0;
    private float duration = 1f;
    private float elapsed = 0f;
    private int startCoins = 0;
    private int targetCoins = 0;
    private bool isUpdating = false;

    public TextMeshProUGUI coinText;
    private Animator coinAnimator;
    public AudioSource loopingSource;

    void Start()
    {
        coinAnimator = coinText.transform.parent.gameObject.GetComponent<Animator>();
        currentCoins = PlayerInteraction.Instance.currentMoney;
        targetCoins = currentCoins;
        coinText.text = currentCoins.ToString();
    }

    void Update()
    {
        int actualMoney = PlayerInteraction.Instance.currentMoney;

        // Would be easier to do this through an event but it's too late for allat lol
        if (targetCoins != actualMoney)
        {
            startCoins = currentCoins;
            targetCoins = actualMoney;
            elapsed = 0f;
            isUpdating = true;

            coinAnimator.SetBool("MoneyChanging", true);
            coinAnimator.SetTrigger("MoneyUpdate");
            if (!loopingSource.isPlaying) loopingSource.Play();
        }

        if (isUpdating)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            currentCoins = Mathf.RoundToInt(Mathf.Lerp(startCoins, targetCoins, t));
            coinText.text = currentCoins.ToString();

            if (t >= 1f)
            {
                currentCoins = targetCoins;
                coinAnimator.SetBool("MoneyChanging", false);
                if (loopingSource.isPlaying) loopingSource.Stop();
                isUpdating = false;
            }
        }
    }
}