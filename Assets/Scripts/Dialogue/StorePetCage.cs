using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorePetCage : MonoBehaviour
{
    public List<GameObject> petObjects;
    GameObject activeObject;

    public void RefreshCage(CritterType type)
    {
        ClearCage();
        switch(type)
        {
            case CritterType.Hog:
            activeObject = petObjects[3];
            break;

            case CritterType.Mimic:
            activeObject = petObjects[4];
            break;

            case CritterType.Hen:
            activeObject = petObjects[2];
            break;
        }
        if(activeObject) activeObject.SetActive(true);
    }

    public void RefreshCage(PetType type)
    {
        ClearCage();
        switch(type)
        {
            case PetType.Cat:
            activeObject = petObjects[0];
            break;

            case PetType.Grub:
            activeObject = petObjects[1];
            break;

            case PetType.Dog:
            activeObject = petObjects[5];
            break;
        }
        if(activeObject) activeObject.SetActive(true);
    }

    public void ClearCage()
    {
        if(!activeObject) return;
        activeObject.SetActive(false);
        activeObject = null;
    }
}
