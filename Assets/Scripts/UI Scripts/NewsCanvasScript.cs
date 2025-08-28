using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NewsCanvasScript : MonoBehaviour
{
    [SerializeField] private GameObject button;
    //[SerializeField] private TextMeshProUGUI descriptionObj;
    //[TextArea(5,1)]
    //[SerializeField] private string description;

    // Start is called before the first frame update
    void Start()
    {
        //descriptionObj.text = description;
    }

    // Update is called once per frame
    void Update()
    {
        if (ControlManager.isController) EventSystem.current.SetSelectedGameObject(button);
    }
}
