using UnityEngine;

public class TimeOfDayScript : MonoBehaviour
{
    public GameObject watchHand;

    void Update()
    {
        UpdateWatch();
        
    }

    void UpdateWatch()
    {
       watchHand.transform.rotation = Quaternion.Euler(0,0,(TimeManager.Instance.currentHour - 6) * 15);
    }
}
