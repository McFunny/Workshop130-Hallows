using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CabinBed : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public PopupScript sleepConfirm, cantSleep;

    public bool canSleep = false;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    private void Start()
    {
        //
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;

        if(SleepCheck())
        {
            if(canSleep)
            {
                Sleep();
            }
            else StartCoroutine(SleepTimer());
        }
        else PopupHandler.Instance.AddToQueue(cantSleep);
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        //
        interactSuccessful = false;
    }
    
    public void EndInteraction()
    {
       
    }

    bool SleepCheck()
    {
        if(TimeManager.Instance.stopTime || TimeManager.Instance.currentHour < 8 || TimeManager.Instance.currentHour >= 19) return false;
        else return true;
    }

    IEnumerator SleepTimer()
    {
        canSleep = true;
        PopupHandler.Instance.AddToQueue(sleepConfirm);
        yield return new WaitForSeconds(3);
        canSleep = false;
    }

    void Sleep()
    {
        StartCoroutine(TimeManager.Instance.Sleep());
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
