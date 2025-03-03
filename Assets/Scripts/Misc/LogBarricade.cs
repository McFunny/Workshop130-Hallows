using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LogBarricade : MonoBehaviour, IInteractable
{
    public LumberjackNPC person;

    public TreeID id;
    bool checkStart = true;
    bool isPapered;

    public InventoryItemData papers;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public bool forceCut = false;

    public GameObject paperSprite;

    public Transform itemDropTransform;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    void Start()
    {
        checkStart = true;
        CheckData();
        TimeManager.OnHourlyUpdate += CheckData;
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        if(isPapered)
        {
            isPapered = false;
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(papers);
            droppedItem.transform.position = itemDropTransform.position;
            paperSprite.SetActive(false);
            interactSuccessful = true;
            return;
        }
        interactSuccessful = false;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {

        if((item == papers && !isPapered))
        {
            paperSprite.SetActive(true);
            isPapered = true;
            interactSuccessful = true;
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            return;
        }
        interactSuccessful = false;
        
    }
    
    public void EndInteraction()
    {
       
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
        if(checkStart || TimeManager.Instance.currentHour == 8) checkStart = false;
        else return;
        
        switch(id)
        {
            case TreeID.TownTree:
            if(GameSaveData.Instance.townTreeCleared1 || isPapered)
            { //Complete Quest
                if(!GameSaveData.Instance.townTreeCleared1) GameSaveData.Instance.townTreeCleared1 = true;
                if(person) QuestManager.Instance.ForceCompleteQuest(person.treeQuest);
                Destroy(this.gameObject);
            }
            break;

            case TreeID.FarmTree:
            if(GameSaveData.Instance.townTreeCleared2 || isPapered)
            {
                if(!GameSaveData.Instance.townTreeCleared2) GameSaveData.Instance.townTreeCleared2 = true;
                Destroy(this.gameObject);
            }
            break;
        }
    }

    void OnDestroy()
    {
        TimeManager.OnHourlyUpdate -= CheckData;
    }
}

public enum TreeID
{
    TownTree,
    FarmTree
}
