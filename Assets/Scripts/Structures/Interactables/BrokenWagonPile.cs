using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BrokenWagonPile : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public PopupScript repairInstructions;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    private void Start()
    {
        StartCoroutine(DelayedStart());
        TimeManager.OnHourlyUpdate += CheckData;
    }

    IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(5);
        if(GameSaveData.Instance.playerWagonUnlocked) Destroy(gameObject);
    }

    void CheckData()
    {
        if(TimeManager.Instance.currentHour != 8) return;
        if(GameSaveData.Instance.playerWagonUnlocked) Destroy(gameObject);
        StartCoroutine(DelayedStart());
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;

        if(!GameSaveData.Instance.playerWagonFound)
        {
            GameSaveData.Instance.playerWagonFound = true;
            QuestManager.Instance.AddQuest(QuestDatabase.Instance.GetMainQuest(14));
        }

        PopupHandler.Instance.AddToQueue(repairInstructions);
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        //
        interactSuccessful = false;
    }
    
    public void EndInteraction()
    {
       
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
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
