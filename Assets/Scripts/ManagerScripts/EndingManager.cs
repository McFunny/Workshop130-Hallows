using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EndingManager : MonoBehaviour
{
    public delegate void BeginEnding();
    public static event BeginEnding OnEndingStarted;

    public static EndingManager Instance;

    public GameObject endingObjects;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
    }

    void InitializeEnding()
    {
        endingObjects.SetActive(true);
        PlayerInteraction.Instance.transform.position = TimeManager.Instance.playerRespawn.position;

        OnEndingStarted?.Invoke();
    }
}
