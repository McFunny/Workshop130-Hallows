using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class SlotMachine : MonoBehaviour,IInteractable
{
    [SerializeField] GameObject Slot1;
    [SerializeField] GameObject Slot2;
    [SerializeField] GameObject Slot3;

    public int SlotIndex1;
    public int SlotIndex2;
    public int SlotIndex3;

    public int moneySpent = 0;
    public int cost = 50;

    public float speed, timePerSlot;

    public InventoryItemData bullet, mints;

    public List<InventoryItemData> randomPlant = new List<InventoryItemData>();
    public List<InventoryItemData> bannedCrops = new List<InventoryItemData>();
    public InventoryItemData carrotSeed;

    public Transform itemCollection;

    public GameObject droppedItem;

    private GameObject pyreflyEnemy;
    public GameObject pyreflyPrefab;

    private Animator animator;

    public bool coroutineRunning = false, broken = false, puzzleSolved = false, stayWithItem = false;

    public bool DebugMode = false;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;



    public UnityAction<IInteractable> OnInteractionComplete { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
     /* if (Input.GetKeyUp(KeyCode.G) && !coroutineRunning)
        {
            if (!broken)
            {
                if (PlayerInteraction.Instance.currentMoney >= cost && !coroutineRunning)
                {
                    PlayerInteraction.Instance.currentMoney -= cost;
                    moneySpent += cost;
                    StartCoroutine(LetsGamble());
                }
            }
        }*/
    }

    IEnumerator LetsGamble()
    {
        coroutineRunning = true;
        if (!puzzleSolved && moneySpent >= 500)
        {
            SlotIndex1 = 1;
            SlotIndex2 = 1;
            SlotIndex3 = 1;
            yield return StartCoroutine(RotateSlots());
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Reward());
            coroutineRunning = false;
        }
        else if (DebugMode)
        {
            SlotIndex1 = 2;
            SlotIndex2 = 2;
            SlotIndex3 = 2;
            yield return StartCoroutine(RotateSlots());

            yield return new WaitForSeconds(0.5f);

            StartCoroutine(Reward());
            coroutineRunning = false;

        }
        else
        {
            int r = Random.Range(0, 2);
            SlotIndex1 = Random.Range(1, 7);
            if (r == 1) { SlotIndex2 = Random.Range(1, 7); }
            else if (r == 0) { SlotIndex2 = SlotIndex1; }
            r = Random.Range(0, 2);
            if (r == 0 && SlotIndex1 == SlotIndex2) { SlotIndex3 = SlotIndex2; }
            else if (r == 1) { SlotIndex3 = Random.Range(1, 7); }

            yield return StartCoroutine(RotateSlots());

            yield return new WaitForSeconds(0.5f);

            StartCoroutine(Reward());
            coroutineRunning = false;
        }
    }

    IEnumerator Reward()
    {
        float animLength;
        if (SlotIndex1 == SlotIndex2 && SlotIndex2 == SlotIndex3)
        {
            switch (SlotIndex1)
            {
                case 1:
                    if (!puzzleSolved)
                    {
                        puzzleSolved = true;
                        //play sound effect
                        animator.SetTrigger("OpenMouth");
                        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                        yield return new WaitForSeconds(animLength);
                        mints.maxStackSize = (cost * 5);
                        droppedItem = ItemPoolManager.Instance.GrabItem(mints);
                        droppedItem.transform.position = itemCollection.position;
                        stayWithItem = true;
                        animator.SetTrigger("CloseMouth");
                        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                        while (stayWithItem)
                        {
                            droppedItem.transform.position = itemCollection.position;
                            yield return null;
                        }
                        yield return new WaitForSeconds(animLength);
                    }
                    else
                    {
                        animator.SetTrigger("OpenMouth");
                        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                        yield return new WaitForSeconds(animLength);
                        mints.maxStackSize = (cost * 5);
                        droppedItem = ItemPoolManager.Instance.GrabItem(mints);
                        droppedItem.transform.position = itemCollection.position;
                        stayWithItem = true;
                        animator.SetTrigger("CloseMouth");
                        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                        while (stayWithItem)
                        {
                            droppedItem.transform.position = itemCollection.position;
                            yield return null;
                        }
                        yield return new WaitForSeconds(animLength);

                    }
                    break;
                case 2:

                    animator.SetTrigger("OpenMouth");
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animLength);
                    mints.maxStackSize = (cost * 2);
                    droppedItem = ItemPoolManager.Instance.GrabItem(mints);
                    droppedItem.transform.position = itemCollection.position;
                    stayWithItem = true;
                    animator.SetTrigger("CloseMouth");
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    while (stayWithItem)
                    {
                        droppedItem.transform.position = itemCollection.position;
                        yield return null;
                    }
                    yield return new WaitForSeconds(animLength);
                    break;
                case 3:
                    animator.SetTrigger("OpenMouth");
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animLength);
                    droppedItem = ItemPoolManager.Instance.GrabItem(bullet);
                    droppedItem.transform.position = itemCollection.position;
                    stayWithItem = true;
                    animator.SetTrigger("CloseMouth");
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    while (stayWithItem)
                    {
                        droppedItem.transform.position = itemCollection.position;
                        yield return null;
                    }
                    yield return new WaitForSeconds(animLength);

                    break;
                case 4:
                    if (puzzleSolved)
                    {
                        yield return StartCoroutine(SummonPyreFly());
                        broken = true;
                    }
                    else 
                    {
                        yield return StartCoroutine(SummonPyreFly());

                        //play broken sound effect
                    }
                    break;
                case 5:
                    List<InventoryItemData> randomPlant = Database.Instance.GetAllCrops().Cast<InventoryItemData>().ToList();
                    int r = Random.Range(1, randomPlant.Count);
                    for (int i = 0; i < bannedCrops.Count; i++)
                    {
                        if (randomPlant[r] == bannedCrops[i])
                        {
                            randomPlant[r] = carrotSeed;
                        }
                    }
                    animator.SetTrigger("OpenMouth");
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animLength);
                    droppedItem = ItemPoolManager.Instance.GrabItem(randomPlant[r]);
                    droppedItem.transform.position = itemCollection.position;
                    stayWithItem = true;
                    animator.SetTrigger("CloseMouth");
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    while (stayWithItem)
                    {
                        droppedItem.transform.position = itemCollection.position;
                        yield return null;
                    }
                    yield return new WaitForSeconds(animLength);
                    break;
                case 6:
                    yield return StartCoroutine(SummonPyreFly());
                    break;
            }
        }
        else
        {
            animator.SetTrigger("CloseEyes");
            animLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);
        }
    }

        IEnumerator SummonPyreFly()
    {
        float animLength;
        animator.SetTrigger("OpenMouth");
        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        pyreflyEnemy = Instantiate(pyreflyPrefab, new Vector3(itemCollection.position.x, itemCollection.position.y - 1f, itemCollection.position.z), itemCollection.rotation);
        stayWithItem = true;
        animator.SetTrigger("CloseMouth");
        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        while (stayWithItem)
        {
            pyreflyEnemy.transform.position = new Vector3 (itemCollection.position.x, itemCollection.position.y - 1f, itemCollection.position.z);
            yield return null;
        }
        yield return new WaitForSeconds(animLength);
        PyreFly pyreFlyScript = pyreflyEnemy.GetComponent<PyreFly>();
        pyreFlyScript.OnDestroy();
    }

        IEnumerator RotateSlots()
    {
        animator.SetTrigger("OpenEyes");
        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        StartCoroutine(SpinSlot(Slot1.transform, timePerSlot)); 
        StartCoroutine(SpinSlot(Slot2.transform, timePerSlot*2)); 
        StartCoroutine(SpinSlot(Slot3.transform, timePerSlot*3));
        yield return new WaitForSeconds(timePerSlot);
        Slot1.transform.rotation = Quaternion.Euler(0, 0, SlotIndex1 * 60f);
        yield return new WaitForSeconds(timePerSlot);
        Slot2.transform.rotation = Quaternion.Euler(0, 0, SlotIndex2 * 60f);
        yield return new WaitForSeconds(timePerSlot);
        Slot3.transform.rotation = Quaternion.Euler(0, 0, SlotIndex3 * 60f);
    }

    IEnumerator SpinSlot(Transform slot, float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            slot.rotation = Quaternion.Lerp(slot.rotation, Quaternion.Euler(slot.eulerAngles.x, slot.eulerAngles.y, slot.eulerAngles.z + 30f), Time.deltaTime * speed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;
        if (!broken)
        {
            if (PlayerInteraction.Instance.currentMoney >= cost && !coroutineRunning)
            {
                PlayerInteraction.Instance.currentMoney -= cost;
                moneySpent += cost;
                StartCoroutine(LetsGamble());
                interactSuccessful = true;
            }
        }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
    }

    public void EndInteraction()
    {
       
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
        }

        if (!enable && highlightEnabled)
        {
            highlightEnabled = false;
            foreach (GameObject thing in highlight) thing.SetActive(false);
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

    public void SetStayWithItemFalse()
    {
        stayWithItem = false;
    }
}

