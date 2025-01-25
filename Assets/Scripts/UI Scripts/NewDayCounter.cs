using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NewDayCounter : MonoBehaviour
{

    private int currentDayCount;
    private Animator animator;
    TimeManager timeManager;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private GameObject container;
    private Image containerImage;
    // Start is called before the first frame update
    void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
        currentDayCount = timeManager.dayNum;
        animator = GetComponent<Animator>();
        counterText.text = " " + timeManager.dayNum.ToString();
        containerImage = container.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Semicolon)) { timeManager.dayNum ++; }
        if(Input.GetKeyDown(KeyCode.Quote)) { timeManager.dayNum --; }

        if (currentDayCount != timeManager.dayNum && animator.GetCurrentAnimatorStateInfo(0).IsName("NewDayDisabled"))
        {
            StartCoroutine(NewDay());
        }
        else if(currentDayCount != timeManager.dayNum)
        {
            currentDayCount = timeManager.dayNum;
        }

        containerImage.color = new Color(1f,1f,1f,counterText.color.a);
    }
    IEnumerator NewDay()
    {
        animator.SetTrigger("NewDayTrigger");
        AnimatorReset(); // Makes sure the animation doesn't play twice, just in case
        yield return new WaitForSeconds(0.1f);
    }

    public void IncrementCounter()
    {
        currentDayCount = timeManager.dayNum;
        counterText.text = " " + currentDayCount.ToString();
    }

    public void AnimatorReset()
    {
        currentDayCount = timeManager.dayNum;
    }
}
