using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Unity.PlasticSCM.Editor.WebApi;
using JetBrains.Annotations;

public class PlayingCard : MonoBehaviour
{
    public Card card;

    public TextMeshProUGUI valueText;

    public bool coroutineRunning = false;

    public Vector3 spawnPosition;
    public Quaternion spawnRotation;

    public void SetupCard(Card cardGiven, bool faceDown)
    {

        card = cardGiven;
        valueText.text = card.display;
        if (card.isAce) valueText.text = "A";

    }

    public void Update()
    {

    }

    public void FlipDealerCard()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(FlipCard());
        }
    }

    public void BackInDeck()
    {
        if (!coroutineRunning)
        {
            StartCoroutine(BackToTheDeck());
        }
    }
    public void MoveToPoint(Transform point, bool faceDown)
    {
        if (!coroutineRunning)
        {
            StartCoroutine(MoveToPointCoroutine(point, faceDown));
        }
    }

    IEnumerator FlipCard()
    {
        coroutineRunning = true;
        float totalDuration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startingPosition = transform.position;
        Quaternion startingRotation = transform.rotation;
        Vector3 halfwayFlipPosition = transform.position + new Vector3(0, 0.5f, 0);
        Quaternion halfwayFlipRotation = transform.rotation * Quaternion.Euler(-90f, 0, 0);
        Quaternion endFlipRotation = transform.rotation * Quaternion.Euler(-180f, 0, 0);

        while (elapsedTime < totalDuration)
        {
            float t = elapsedTime / totalDuration;

            if (t < 0.5f)
            {
                float phaseT = t / 0.5f;
                transform.position = Vector3.Lerp(startingPosition, halfwayFlipPosition, phaseT);
                transform.rotation = Quaternion.Slerp(startingRotation, halfwayFlipRotation, phaseT);
            }
            else
            {
                float phaseT = (t - 0.5f) / 0.5f;
                transform.position = Vector3.Lerp(halfwayFlipPosition, startingPosition, phaseT);
                transform.rotation = Quaternion.Slerp(halfwayFlipRotation, endFlipRotation, phaseT);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = startingPosition;
        transform.rotation = endFlipRotation;
        coroutineRunning = false;
    }

    IEnumerator MoveToPointCoroutine(Transform point, bool faceDown)
    {
        coroutineRunning = true;
        Vector3 startingPosition = transform.position;
        Quaternion startingRotation = transform.rotation;
        Vector3 endingPosition = point.position;
        Quaternion endingRotation = point.rotation;

        float totalDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < totalDuration)
        {
            float t = elapsedTime / totalDuration;

            transform.position = Vector3.Lerp(startingPosition, endingPosition, t);
            transform.rotation = Quaternion.Slerp(startingRotation, endingRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endingPosition;
        transform.rotation = endingRotation;

        coroutineRunning = false;
        if (!faceDown)
        {
            StartCoroutine(FlipCard());
        }


    }

    IEnumerator BackToTheDeck()
    {
        coroutineRunning = true;
        Vector3 startingPosition = transform.position;
        Quaternion startingRotation = transform.rotation;
        Vector3 endingPosition = spawnPosition;
        Quaternion endingRotation = spawnRotation;

        float totalDuration = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < totalDuration)
        {
            float t = elapsedTime / totalDuration;

            transform.position = Vector3.Lerp(startingPosition, endingPosition, t);
            transform.rotation = Quaternion.Slerp(startingRotation, endingRotation, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = endingPosition;
        transform.rotation = endingRotation;
    }

    private void OnDestroy()
    {
        
    }
}
