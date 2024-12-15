using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectRefresher : MonoBehaviour
{
    public List<ToolBehavior> t_Behaviors;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (ToolBehavior t in t_Behaviors)
        {
            t.usingPrimary = false;
            t.usingSecondary = false;
        }
    }
}
