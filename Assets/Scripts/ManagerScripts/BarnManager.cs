using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BarnManager : MonoBehaviour
{
    /////THIS CLASS WILL HOLD ALL REFERENCES TO FARM CRITTERS, AND IDENTIFY IF THEY ARE WITHIN THE BARN OR THE FARM, AS WELL AS OTHER FUNCTIONS/////
    /// 
    public static BarnManager Instance;

    public List<ICritter> allCritters = new List<ICritter>();

    public Transform barnSource; //The point used to identify distance

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

    public bool WithinBarn(Vector3 pos)
    {
        if(Vector3.Distance(pos, barnSource.position) > 80) return false;
        else return true;
    }
}
