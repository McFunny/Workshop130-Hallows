using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimEvents : MonoBehaviour
{
    public delegate void FloatChange(float f);
    public event FloatChange OnFloatChange;
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
}
