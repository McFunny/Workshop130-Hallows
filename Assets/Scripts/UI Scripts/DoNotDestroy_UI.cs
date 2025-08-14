using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoNotDestroy_UI : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
