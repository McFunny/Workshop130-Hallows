using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DaysSince : FurnitureBehaviorScript
{
    public TextMeshPro onesPlace;
    public TextMeshPro tensPlace;
    public TextMeshPro hundredsPlace;
    public GameObject hundredsPlaceStickyNote;


    void Start()
    {
        base.Start();
        FurnitureStart();
        UpdateDays();
        
    }

    private void OnEnable()
    {
        PlayerInteraction.OnPlayerDeath += UpdateDays;
    }

    private void OnDisable()
    {
        PlayerInteraction.OnPlayerDeath -= UpdateDays;
    }


    void UpdateDays()
    {
        int days = PlayerInteraction.Instance.daysSinceDeath;
        days = Mathf.Clamp(days, 0, 999);

        string numStr;

       
        if (days < 100)
        {
            numStr = days.ToString("D2");

           hundredsPlaceStickyNote.SetActive(false);
            hundredsPlace.gameObject.SetActive(false);

          
            tensPlace.text = numStr[0].ToString();
            onesPlace.text = numStr[1].ToString();
            tensPlace.gameObject.SetActive(true);
            onesPlace.gameObject.SetActive(true);
        }
        else 
        {
            numStr = days.ToString();
            hundredsPlaceStickyNote.SetActive(true);
            hundredsPlace.text = numStr[0].ToString();
            tensPlace.text = numStr[1].ToString();
            onesPlace.text = numStr[2].ToString();

            hundredsPlace.gameObject.SetActive(true);
            tensPlace.gameObject.SetActive(true);
            onesPlace.gameObject.SetActive(true);
        }
    }

}
