using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TownGate : MonoBehaviour, IInteractable
{
    public GameObject townMist, farmMist;
    //public Transform townPos, farmPos;
    //public bool inTown = false;

    public PlayerLocation location;
    //bool transitioning = false;

    public static TownGate Instance;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

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

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        //print(enteringTown);
        if((location == PlayerLocation.InTown && enteringTown) || (location != PlayerLocation.InTown && !enteringTown)) return;
        if(enteringTown)
        {
            //PlayerInteraction.Instance.transform.position = farmPos.position;
            townMist.gameObject.SetActive(true);
            farmMist.gameObject.SetActive(false);
            //inTown = true;
            location = PlayerLocation.InTown;
        }
        else
        {
            //PlayerInteraction.Instance.transform.position = townPos.position;
            townMist.gameObject.SetActive(false);
            farmMist.gameObject.SetActive(true);
            location = PlayerLocation.InFarm;
        }
        PlayerMovement.restrictMovementTokens--;
        FadeScreen.coverScreen = false;
        transitioning = false;
    }

    public void GameOver()
    {
        location = PlayerLocation.InFarm;
        townMist.gameObject.SetActive(false);
        farmPos.gameObject.SetActive(true);
    }

    public void ToggleHighlight(bool enable)
    {
        if(highlight.Count == 0) return;
        if(highlightMaterial.Count == 0)
        {
            foreach(GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if(enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach(GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if(!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach(GameObject thing in highlight) thing.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while(highlightEnabled)
        {
            do
            {
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power > 0.7f && highlightEnabled);
            do
            {
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach(Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while(power < 1.9f && highlightEnabled);
        }
    }

}

public enum PlayerLocation
{
    InFarm,
    InTown,
    InCrypt,
    InWilderness
}
