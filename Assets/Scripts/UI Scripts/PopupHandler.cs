using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupHandler : MonoBehaviour
{
    public static Queue<PopupScript> popupQueue = new Queue<PopupScript>();
    public PopupScript testPopup, testPopup2, testPopup3;
    private PopupScript currentPopup;
    public GameObject popupContainer;
    public TMP_Text popupText;

    // For Lerps
    public Transform lerpStart, lerpEnd, popupTransform;
    float moveProgress = 0;
    float maxMoveProgress = 0.5f;

    private Coroutine queueChecker;
    private bool isActive, conditionMet;

    private void Start()
    {
        PopupEvents.current.OnTillGround += OnTillGround;
        PopupEvents.current.OnShovelSwing += OnShovelSwing;
        //popupContainer.SetActive(false);
        conditionMet = false;
        popupTransform.position = lerpStart.position;
    }

    private void OnDestroy()
    {
        PopupEvents.current.OnTillGround -= OnTillGround;
        PopupEvents.current.OnShovelSwing -= OnShovelSwing;
    }

    void Update()
    {
        // Debug Inputs to force additions to the Queue
        if (Input.GetKeyDown(KeyCode.B))
        {
            AddToQueue(testPopup);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            AddToQueue(testPopup2);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddToQueue(testPopup3);
        }

        if(isActive && moveProgress < maxMoveProgress)
        {
            moveProgress += Time.deltaTime;
            popupTransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
        }

        if(!isActive && moveProgress > 0)
        {
            moveProgress -= Time.deltaTime;
            popupTransform.position = Vector3.Lerp(lerpStart.position, lerpEnd.position, moveProgress/maxMoveProgress);
        }
    }

    public void AddToQueue(PopupScript popup)
    {
        popupQueue.Enqueue(popup);
        print("Added " + popup + " to Queue");
        if (queueChecker == null) queueChecker = StartCoroutine(CheckQueue());
    }

    private void ShowPopup(PopupScript popup)
    {
        isActive = true;
        //popupContainer.SetActive(true);
        popupText.text = popup.text;
        currentPopup = popup;
    }

    IEnumerator CheckQueue()
    {
        // Check Queue until there are no more popups
        while (popupQueue.Count > 0)
        {
            PopupScript popup = popupQueue.Dequeue();
            ShowPopup(popup);

            yield return StartCoroutine(CloseCondition(popup));
        }

        // Clean up
        print("All popups processed");
        isActive = false;
        //popupContainer.SetActive(false);
        queueChecker = null;
    }

    IEnumerator CloseCondition(PopupScript popup)
    {
        if (popup.endCondition == PopupScript.EndCondition.TimeBased)
        {
            // Wait for the specified time
            yield return new WaitForSeconds(popup.endTimeInSeconds);
            print("Popup Timer Ended");
        }
        else if (popup.endCondition == PopupScript.EndCondition.TillGround)
        {
            yield return new WaitUntil(() => conditionMet);
            print("Ground Tilled");
            conditionMet = false; // Reset
        }
        else if (popup.endCondition == PopupScript.EndCondition.ShovelSwing)
        {
            yield return new WaitUntil(() => conditionMet);
            print("SHOVEL!!!");
            conditionMet = false; // Reset
        }
    }

    private void OnTillGround()
    {
        if (isActive && currentPopup.endCondition == PopupScript.EndCondition.TillGround)
        {
            conditionMet = true;
        }
    }

    private void OnShovelSwing()
    {
        if (isActive && currentPopup.endCondition == PopupScript.EndCondition.ShovelSwing)
        {
            conditionMet = true;
        }
    }
}

