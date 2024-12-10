using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    public delegate void FloatChange(float f);
    public delegate void ColliderChange(bool b);
    public event FloatChange OnFloatChange;
    public event ColliderChange OnColliderChange;

    public CreatureBehaviorScript creatureScript;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FloatChangeEvent(float f)
    {
        OnFloatChange?.Invoke(f);
    }

    public void ColliderChangeEvent(string b)
    {
        bool value = b.ToLower() == "true";
        OnColliderChange?.Invoke(value);
    }

    public void FinishedDying()
    {
        if (creatureScript) creatureScript.canCorpseBreak = true;
    }
}
