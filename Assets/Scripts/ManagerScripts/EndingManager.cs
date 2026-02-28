using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EndingManager : MonoBehaviour
{
    public delegate void BeginEnding();
    public static event BeginEnding OnEndingStarted;

    public static EndingManager Instance;

    public bool endingPlaying = false;
    public bool enteredBurningTown = false;

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

    public void EnteredTown()
    {
        if(enteredBurningTown) return;

        enteredBurningTown = true;
        AmbientAudioManager.Instance.ImmediateMusicRefresh();
    }

    public void InitializeEnding()
    {
        endingPlaying = true;
        EndCutsceneObjectToggler.Instance.OnEndCutscene();
        PlayerInteraction.Instance.transform.position = TimeManager.Instance.playerRespawn.position;

        OnEndingStarted?.Invoke();

        TimeManager.Instance.currentHour = 6;
        TimeManager.Instance.RefreshSkybox();
        PlayerMovement.Instance.disableSprint = true;
    }
}
