using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PopupHandler : MonoBehaviour
{
    public Queue<PopupScript> popupQueue = new Queue<PopupScript>();
    public PopupScript testPopup, testPopup2;
    private PopupScript currentPopup;
    public GameObject popupContainer;
    public TMP_Text popupText;

    // For Lerps
    public Transform lerpStart, lerpEnd, popupTransform;
    float timeSpendAnimating = 0;
    float moveProgress = 0;
    float maxMoveProgress = 0.5f;

    private Coroutine queueChecker;
    private bool isActive, groundTilled;

    private void Start()
    {
        PopupEvents.current.OnTillGround += OnTillGround;
        popupContainer.SetActive(false);
        groundTilled = false;
    }

    private void OnDestroy()
    {
        PopupEvents.current.OnTillGround -= OnTillGround;
    }

    void Update()
    {
        // Debug Inputs to force additions to the Queue
        if (Input.GetKeyDown(KeyCode.N))
        {
            AddToQueue(testPopup);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddToQueue(testPopup2);
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
        popupContainer.SetActive(true);
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
        popupContainer.SetActive(false);
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
            yield return new WaitUntil(() => groundTilled); // Wait groundTilled is set to true
            print("Ground Tilled");
            groundTilled = false; // Reset
        }
    }

    private void OnTillGround()
    {
        if (isActive && currentPopup.endCondition == PopupScript.EndCondition.TillGround)
        {
            groundTilled = true;
        }
    }
}

