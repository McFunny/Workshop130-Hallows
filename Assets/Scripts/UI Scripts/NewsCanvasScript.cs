using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewsCanvasScript : MonoBehaviour
{
    [SerializeField] GameObject button;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(button);
    }
}
