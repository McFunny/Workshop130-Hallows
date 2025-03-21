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

    bool hideCounter = true;
    bool fortnite = false; //teehee!!!

    // Start is called before the first frame update
    void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
        currentDayCount = timeManager.dayNum;
        animator = GetComponent<Animator>();
        counterText.text = " " + timeManager.dayNum.ToString();
        containerImage = container.GetComponent<Image>();

        StartCoroutine(DelayStart());
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Semicolon)) { timeManager.dayNum ++; }
        //if(Input.GetKeyDown(KeyCode.Quote)) { timeManager.dayNum --; }
        if (currentDayCount != timeManager.dayNum && animator.GetCurrentAnimatorStateInfo(0).IsName("NewDayDisabled") && !hideCounter)
        {
            StartCoroutine(NewDay());
        }
        else if(currentDayCount != timeManager.dayNum)
        {
            currentDayCount = timeManager.dayNum;
        }

        if(Input.GetKeyDown(KeyCode.Semicolon) && fortnite == false) { StartCoroutine(ForceAnim()); }

        containerImage.color = new Color(1f,1f,1f,counterText.color.a);
    }
    IEnumerator NewDay()
    {
        if(!hideCounter) animator.SetTrigger("NewDayTrigger");
        AnimatorReset(); // Makes sure the animation doesn't play twice, just in case
        yield return new WaitForSecondsRealtime(0.1f);
    }

    public IEnumerator ForceAnim()
    {
        if(!hideCounter) animator.SetTrigger("ForceAnim");
        AnimatorReset(); // Makes sure the animation doesn't play twice, just in case
        print("Daycounter Anim Forced");
        yield return new WaitForSecondsRealtime(0.1f);
        fortnite = true;
    }

    IEnumerator DelayStart()
    {
        yield return new WaitForEndOfFrame();
        currentDayCount = timeManager.dayNum;
        counterText.text = " " + currentDayCount.ToString();
        hideCounter = false;
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
