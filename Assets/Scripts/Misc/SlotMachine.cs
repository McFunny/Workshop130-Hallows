using SaveLoadSystem;
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

    private int moneySpent = 0, timesSpun; 
    public int cost = 50;

    public float speed, timePerSlot;

    public InventoryItemData bullet, bugItem;

    public List<InventoryItemData> allowedCrops = new List<InventoryItemData>();

    public Transform itemCollection;

    private GameObject droppedItem;

    private GameObject pyreflyEnemy;
    public GameObject pyreflyPrefab;

    private Animator animator;

    public bool coroutineRunning = false, broken = false, puzzleSolved = false, stayWithItem = false;

    public bool DebugMode = false;

    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    private AudioSource Slot1Source, Slot2Source, Slot3Source, audiosource;

    public AudioSource winAudioSource;

    public AudioClip mouthOpen, mouthClose, clickInPlace, win, brokenSound;

    public static SlotMachine Instance;

    public int debugNumber;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }


    public UnityAction<IInteractable> OnInteractionComplete { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    void Start()
    {
        animator = GetComponent<Animator>();
        TimeManager.OnHourlyUpdate += FixMachine;
        audiosource = GetComponent<AudioSource>();
        Slot1Source = Slot1.GetComponent<AudioSource>();
        Slot2Source = Slot2.GetComponent<AudioSource>();
        Slot3Source = Slot3.GetComponent<AudioSource>();
    }

    private void FixMachine()
    {
        if (TimeManager.Instance.currentHour == 8)
        {
            broken = false;
            timesSpun = 0;
        }
    }

    public void ReturnFocalPoint(out Transform focalPoint)
    {
        focalPoint = transform;
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
        ToggleHighlight(false);
        if (CheckIfBreaks())
        {
            SlotIndex1 = 4;
            SlotIndex2 = 4;
            SlotIndex3 = 4;
            yield return StartCoroutine(RotateSlots());

            yield return new WaitForSeconds(0.5f);

            StartCoroutine(Reward());
            coroutineRunning = false;
        }
        else if (DebugMode)
        {
            SlotIndex1 = debugNumber;
            SlotIndex2 = debugNumber;
            SlotIndex3 = debugNumber;
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
            winAudioSource.Play();
            switch (SlotIndex1)
            {
                case 1:
                    
                        animator.SetTrigger("OpenMouth");
                        audiosource.clip = mouthOpen;
                        audiosource.Play();
                        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                        yield return new WaitForSeconds(animLength);
                        droppedItem = ItemPoolManager.Instance.GrabItem(bugItem);
                        ItemPickup itemPU = droppedItem.GetComponent<ItemPickup>();
                        itemPU.stackSize = 10;
                        droppedItem.transform.position = itemCollection.position;
                        stayWithItem = true;
                        animator.SetTrigger("CloseMouth");
                        audiosource.clip = mouthClose;
                        audiosource.Play();
                        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                        while (stayWithItem)
                        {
                            droppedItem.transform.position = itemCollection.position;
                            yield return null;
                        }
                        yield return new WaitForSeconds(animLength);

                    
                    break;
                case 2:
                   
                    animator.SetTrigger("OpenMouth");
                    audiosource.clip = mouthOpen;
                    audiosource.Play();
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animLength);
                    //bugItem.maxStackSize = (cost);
                    droppedItem = ItemPoolManager.Instance.GrabItem(bugItem);
                    droppedItem.transform.position = itemCollection.position;
                    stayWithItem = true;
                    animator.SetTrigger("CloseMouth");
                    audiosource.clip = mouthClose;
                    audiosource.Play();
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
                    audiosource.clip = mouthOpen;
                    audiosource.Play();
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animLength);
                    droppedItem = ItemPoolManager.Instance.GrabItem(bullet);
                    droppedItem.transform.position = itemCollection.position;
                    stayWithItem = true;
                    animator.SetTrigger("CloseMouth");
                    audiosource.clip = mouthClose;
                    audiosource.Play();
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    while (stayWithItem)
                    {
                        droppedItem.transform.position = itemCollection.position;
                        yield return null;
                    }
                    yield return new WaitForSeconds(animLength);

                    break;
                case 4:
                        yield return StartCoroutine(SummonPyreFly());
                        broken = true;
                    break;
                case 5:
                   
                   
                    int r = Random.Range(1, allowedCrops.Count);
                    animator.SetTrigger("OpenMouth");
                    audiosource.clip = mouthOpen;
                    audiosource.Play();
                    animLength = animator.GetCurrentAnimatorStateInfo(0).length;
                    yield return new WaitForSeconds(animLength);
                    droppedItem = ItemPoolManager.Instance.GrabItem(allowedCrops[r]);
                    droppedItem.transform.position = itemCollection.position;
                    stayWithItem = true;
                    animator.SetTrigger("CloseMouth");
                    audiosource.clip = mouthClose;
                    audiosource.Play();
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
            audiosource.clip = mouthClose;
            audiosource.Play();
            animLength = animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animLength);
        }
    }

    private bool CheckIfBreaks()
    {
        float baseChance = 0.05f;
        float incrementalChance = 0.05f;
        float breakchance = baseChance + (timesSpun * incrementalChance);

        breakchance = Mathf.Clamp01(breakchance);

        float roll = Random.Range(0f, 1f);

        return roll < breakchance;
    }

        IEnumerator SummonPyreFly()
    {
        float animLength;
        animator.SetTrigger("OpenMouth");
        audiosource.clip = mouthOpen;
        audiosource.Play();
        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        pyreflyEnemy = Instantiate(pyreflyPrefab, new Vector3(itemCollection.position.x, itemCollection.position.y - 1f, itemCollection.position.z), itemCollection.rotation);
        stayWithItem = true;
        animator.SetTrigger("CloseMouth");
        audiosource.clip = mouthClose;
        audiosource.Play();
        animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        while (stayWithItem)
        {
            pyreflyEnemy.transform.position = new Vector3 (itemCollection.position.x, itemCollection.position.y - 1f, itemCollection.position.z);
            yield return null;
        }
        yield return new WaitForSeconds(animLength);
        PyreFly pyreFlyScript = pyreflyEnemy.GetComponent<PyreFly>();
        pyreFlyScript.TakeDamage(999);
        audiosource.clip = brokenSound;
        audiosource.Play();

    }

        IEnumerator RotateSlots()
    {
        animator.SetTrigger("OpenEyes");
        audiosource.clip = mouthOpen;
        audiosource.Play();
        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        audiosource.Stop();
        Slot1Source.Play();
        Slot2Source.Play();
        Slot3Source.Play();
        StartCoroutine(SpinSlot(Slot1.transform, timePerSlot)); 
        StartCoroutine(SpinSlot(Slot2.transform, timePerSlot*2)); 
        StartCoroutine(SpinSlot(Slot3.transform, timePerSlot*3));
        yield return new WaitForSeconds(timePerSlot);
        Slot1.transform.localRotation = Quaternion.Euler(0, 0, SlotIndex1 * 60f);
        audiosource.clip = clickInPlace;
        Slot1Source.Stop();
        audiosource.Play();
        yield return new WaitForSeconds(timePerSlot);
        Slot2.transform.localRotation = Quaternion.Euler(0, 0, SlotIndex2 * 60f);
        Slot2Source.Stop();
        audiosource.Play();
        yield return new WaitForSeconds(timePerSlot);
        Slot3.transform.localRotation = Quaternion.Euler(0, 0, SlotIndex3 * 60f);
        Slot3Source.Stop();
        audiosource.Play();
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
        
    }

        public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if (item == bugItem)
        {
            if (!broken)
            {
                if (!coroutineRunning)
                {
                    HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
                    interactor.playerInventoryHolder.UpdateInventory();
                    timesSpun++;
                    moneySpent += cost;
                    StartCoroutine(LetsGamble());
                    interactSuccessful = true;
                }
            }
            else if (broken)
            {
                animator.SetTrigger("Broken");
                interactSuccessful = true;
            }
        }
    }

    public void EndInteraction()
    {
       
    }

    public void ToggleHighlight(bool enable)
    {
        if (coroutineRunning && enable == true) return;
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

    public SlotMachineSaveData ExportSaveData()
    {

        return new SlotMachineSaveData
        {
            _moneySpent = moneySpent,
            _puzzleSolved = puzzleSolved,
            _broken = broken
        };
    }

    public void ImportSaveData(SlotMachineSaveData data)
    {
        puzzleSolved = data._puzzleSolved;
        moneySpent = data._moneySpent;
        broken = data._broken;
    }



}

[System.Serializable]
public struct SlotMachineSaveData
{
    public int _moneySpent;
    public bool _puzzleSolved;
    public bool _broken;
}