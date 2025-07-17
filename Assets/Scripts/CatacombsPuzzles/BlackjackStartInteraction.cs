using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BlackjackStartInteraction : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public GameObject structureUI;
    public TextMeshProUGUI structureUItext;


    public BlackjackBettingBox bettingBox;

    public void EndInteraction()
    {
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (!BlackjackManager.Instance.activeGame && bettingBox.itemsDeposited > 0)
        {
           StartCoroutine(BlackjackManager.Instance.StartGame());
        }
        else if (BlackjackManager.Instance.activeGame && !BlackjackManager.Instance.coroutineRunning)
        {
            StartCoroutine(BlackjackManager.Instance.Hit());
        }

    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
    }

    public void ReturnFocalPoint(out Transform point)
    {
        throw new System.NotImplementedException();
    }

    public void ToggleHighlight(bool enable)
    {
        if (highlight.Count == 0) return;
        if (highlightMaterial.Count == 0)
        {
            foreach (GameObject thing in highlight)
                highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if (enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach (GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
            structureUI.SetActive(true);
        }

        if (!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach (GameObject thing in highlight) thing.SetActive(false);
            structureUI.SetActive(false);
        }
    }

    IEnumerator HightlightFlash()
    {
        float power = 1;
        while (highlightEnabled)
        {
            do
            {
               
                yield return new WaitForSeconds(0.1f);
                power -= 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while (power > 0.7f && highlightEnabled);
            do
            {
               
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            }
            while (power < 1.9f && highlightEnabled);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (highlightEnabled)
        {
            if (!BlackjackManager.Instance.activeGame && bettingBox.itemsDeposited == 0)
                structureUItext.text = "Place Bet";
            else if (!BlackjackManager.Instance.activeGame && bettingBox.itemsDeposited > 0)
                structureUItext.text = "Play Game";
            else if (BlackjackManager.Instance.activeGame)
                structureUItext.text = "Hit";

        }
    }

}
