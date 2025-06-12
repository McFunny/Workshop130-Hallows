using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildActivator : MonoBehaviour
{
    public System.Action onChildActivated;

    private void OnEnable()
    {
        onChildActivated?.Invoke();
        print(this + " is enabled");
    }
}
