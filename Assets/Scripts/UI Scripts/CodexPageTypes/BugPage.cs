using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BugPage : CodexPage
{
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI amountCaughtText, spawnMethodText, activeHoursText, spawnLocationsText;
    
    public override void UpdatePage(CodexEntries entry, Quest quest)
    {
        spawnMethodText.text = "Spawn Method: <br>";
        activeHoursText.text = "Active Hours: <br>";
        spawnLocationsText.text = "Spawn Locations: <br>";

        title.text = entry.entryName;
        image.sprite = entry.mainImage;
        descriptionText.text = entry.rightText;
        
        var bugData = entry.bugData;
        amountCaughtText.text = "Amount Caught: " + bugData.amountCaught.ToString();

        if (bugData.spawnMethod.Count > 0)
        {
            for (int i = 0; i < bugData.spawnMethod.Count; i++)
            {
                if( i == bugData.spawnMethod.Count - 1)
                {
                    spawnMethodText.text += bugData.spawnMethod[i].ToString() + " ";
                }
                else
                {
                    spawnMethodText.text += bugData.spawnMethod[i].ToString() + ", ";
                }
            }
            spawnMethodText.gameObject.SetActive(true);
        }
        else spawnMethodText.gameObject.SetActive(false);

        if (bugData.activeHours.Count > 0)
        {
            for (int i = 0; i < bugData.activeHours.Count; i++)
            {
                if( i == bugData.activeHours.Count - 1)
                {
                    activeHoursText.text += bugData.activeHours[i].ToString() + " ";
                }
                else
                {
                    activeHoursText.text += bugData.activeHours[i].ToString() + ", ";
                }
            }
            activeHoursText.gameObject.SetActive(true);
        }
        else activeHoursText.gameObject.SetActive(false);

        if(bugData.spawnLocations.Count > 0)
        {
            for (int i = 0; i < bugData.spawnLocations.Count; i++)
            {
                if( i == bugData.spawnLocations.Count - 1)
                {
                    spawnLocationsText.text += bugData.spawnLocations[i].ToString() + " ";
                }
                else
                {
                    spawnLocationsText.text += bugData.spawnLocations[i].ToString() + ", ";
                }
            }
            spawnLocationsText.gameObject.SetActive(true);
        }
        else spawnLocationsText.gameObject.SetActive(false);
    }
}
