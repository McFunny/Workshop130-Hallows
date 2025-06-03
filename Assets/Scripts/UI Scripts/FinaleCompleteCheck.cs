using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FinaleCompleteCheck : MonoBehaviour
{
    //[SerializeField] private bool forceFinaleIncomplete;
    private int isFinaleCompleted; // Used to check if the finale has been completed, 0 = false, 1 = true
    // Start is called before the first frame update
    void Start()
    {
        isFinaleCompleted = PlayerPrefs.GetInt("FinaleCompleted", 0);

        print("Finale Completed: " + isFinaleCompleted);
    }

    public void ForceFinaleIncomplete()
    {
        PlayerPrefs.SetInt("FinaleCompleted", 1);
        isFinaleCompleted = PlayerPrefs.GetInt("FinaleCompleted", 0);
    }
    
    public void ForceFinaleComplete()
    {
        PlayerPrefs.SetInt("FinaleCompleted", 1);
        isFinaleCompleted = PlayerPrefs.GetInt("FinaleCompleted", 0);
    }
}


// Editor buttons :)
/*[CustomEditor(typeof(FinaleCompleteCheck), true)]
public class FinaleCompleteCheckEditor : Editor
{
    public override void OnInspectorGUI()
    {
        FinaleCompleteCheck finaleCompleteCheck = (FinaleCompleteCheck)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Force Finale Complete"))
        {
            finaleCompleteCheck.ForceFinaleComplete();
        }

        if (GUILayout.Button("Force Finale Incomplete"))
        {
            finaleCompleteCheck.ForceFinaleIncomplete();
        }
    }
}*/
