using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingFurniture : MonoBehaviour
{
    public GameObject startingFurniture;
    // Start is called before the first frame update
    void Start()
    {
        if(!MainMenuScript.loadingData) startingFurniture.SetActive(true);
    }

}
