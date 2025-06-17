using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LostNet : MonoBehaviour, IInteractable
{
    public InventoryItemData netItem;

    public Collider collider;
    public GameObject net;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    void Start()
    {
        TimeManager.OnHourlyUpdate += CheckData;
        StartCoroutine(DelayedCheck());
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(PlayerInventoryHolder.Instance.AddToInventory(netItem, 1))
        {
            GameSaveData.Instance.bugNetObtained = true;
            interactSuccessful = true;
            QuestManager.Instance.AddQuestProgress(1, QuestDatabase.Instance.GetTutorialQuest(303));
            Destroy(this.gameObject);
            return;
        }
        interactSuccessful = false;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
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


    void CheckData()
    {
        if(!GameSaveData.Instance.ras_askedForNet)
        {
            collider.enabled = false;
            net.SetActive(false);
            return;
        }
        else
        {
            collider.enabled = true;
            net.SetActive(true);
        }

        if(GameSaveData.Instance.bugNetObtained)
        {
            Destroy(this.gameObject);
        }
    }

    IEnumerator DelayedCheck()
    {
        yield return new WaitForSeconds(2);
        CheckData();
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= CheckData;
    }
}
