using UnityEngine;

public class TimeOfDayScript : MonoBehaviour
{
    public GameObject watchHand, darkenObject;
    private TimeManager timeManager;

    void Start()
    {
        timeManager = FindFirstObjectByType<TimeManager>();
    }

    void Update()
    {
        UpdateWatch();
        darkenObject.SetActive(timeManager.clockDarkenEffect);
        
    }

    void UpdateWatch()
    {
       watchHand.transform.rotation = Quaternion.Euler(0,0,(TimeManager.Instance.currentHour - 6) * 15);
    }
}
