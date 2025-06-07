using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NutMachine : MonoBehaviour, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    bool canBeUsed = true;
    public AudioSource source;
    public AudioClip nutCut;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    [Header("Nut Stuff")]
    public Transform nutSpawn;
    public Animator nutMachineAnim;
    public InventoryItemData[] possibleNutItems;
    public float[] nutItemWeight;
    public InventoryItemData treeNut;

    Vector3 lNutPos, rNutPos;
    Quaternion lNutRot, rNutRot;
    public GameObject lNut, rNut;
    public Rigidbody lRB, rRB;

    public GameObject lNutPrefab, rNutPrefab;
    public ParticleSystem abnerParticles; //im going to beat you with many hammers


    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        if(item == treeNut && canBeUsed)
        {
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            StartCoroutine(ChopNut());
            interactSuccessful = true;
            return;
        }
        interactSuccessful = false;
    }
    
    public void EndInteraction()
    {
       
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
    }

    public void Start()
    {
        lNutRot = lNut.transform.rotation;
        rNutRot = rNut.transform.rotation;

        lNutPos = lNut.transform.position;
        rNutPos = lNut.transform.position;

        lNut.SetActive(false);
        rNut.SetActive(false);
    }

    public void Update()
    {
        if(!canBeUsed && highlightEnabled) ToggleHighlight(false);
    }

    IEnumerator ChopNut()
    {
        //put player focal point on the machine, do the machine anim stuff, spawn item, then break focal point
        canBeUsed = false;
        if(!lNut || !rNut) InstantiateNuts();
        else
        {
            lNut.SetActive(true);
            rNut.SetActive(true);
        }

        nutMachineAnim.Play("nutcracker");
        yield return new WaitForSeconds(1.1f);
        int iterations = Random.Range(1, 3);
        for(int i = 0; i < iterations; i++)
        {
            GameObject droppedItem = ItemPoolManager.Instance.GrabItem(RandomNutItem());
            Rigidbody itemRB = droppedItem.GetComponent<Rigidbody>();
            source.PlayOneShot(nutCut);
            itemRB = droppedItem.GetComponent<Rigidbody>();
            itemRB.AddForce(Vector3.forward * 40);
            itemRB.AddForce(Vector3.up * 20);
            lRB.isKinematic = false;
            rRB.isKinematic = false;
        
            droppedItem.transform.position = new Vector3(nutSpawn.position.x, nutSpawn.position.y, nutSpawn.position.z);
            abnerParticles.Play();
        }
        yield return new WaitForSeconds(0.7f);
        lNut = null;
        rNut = null;
        //lNut.SetActive(false);
        //rNut.SetActive(false);
        //ResetNutPosition();
        canBeUsed = true;
    }

    InventoryItemData RandomNutItem()
    {
        int x = 0;
        while(x < 10)
        {
            int i = Random.Range(0, possibleNutItems.Length);
            float r = Random.Range(0f,1f);
            if(r < nutItemWeight[i]) return possibleNutItems[i];
            x++;
        }
        return possibleNutItems[0];
    }

    void ResetNutPosition()
    {
        lRB.isKinematic = true;
        rRB.isKinematic = true;

        lNut.transform.position = lNutPos;
        rNut.transform.position = rNutPos;

        lNut.transform.rotation = lNutRot;
        rNut.transform.rotation = rNutRot;
    }

    void InstantiateNuts()
    {
        lNut = Instantiate(lNutPrefab, lNutPos, lNutRot);
        rNut = Instantiate(rNutPrefab, rNutPos, rNutRot);
        lRB = lNut.GetComponent<Rigidbody>();
        rRB = rNut.GetComponent<Rigidbody>();
    }



    public void ToggleHighlight(bool enable)
    {
        if(highlight.Count == 0) return;
        if(highlightMaterial.Count == 0)
        {
            foreach(GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if(enable && !highlightEnabled && canBeUsed)
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
