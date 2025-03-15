using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlotMachine : MonoBehaviour
{
    [SerializeField] GameObject Slot1;
    [SerializeField] GameObject Slot2;
    [SerializeField] GameObject Slot3;

    public int SlotIndex1;
    public int SlotIndex2;
    public int SlotIndex3;

    public float speed;

    public bool coroutineRunning = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.G) && !coroutineRunning)
        {
            StartCoroutine(LetsGamble());
        }    
    }

    IEnumerator LetsGamble()
    {
        coroutineRunning = true;
        int r = Random.Range(0,2);
        SlotIndex1 = Random.Range(1, 7);
        if (r == 1) { SlotIndex2 = Random.Range(1, 7);}
        else if (r == 0) { SlotIndex2 = SlotIndex1;}
        r = Random.Range(0, 2);
        if (r == 0 && SlotIndex1 == SlotIndex2) { SlotIndex3 = SlotIndex2; }
        else if (r == 1) { SlotIndex3 = Random.Range(1, 7); }

        yield return StartCoroutine(RotateSlots());

        yield return new WaitForSeconds(r);
        coroutineRunning = false;
    }

    IEnumerator RotateSlots()
    {
        StartCoroutine(SpinSlot(Slot1.transform, 2f)); 
        StartCoroutine(SpinSlot(Slot2.transform, 4f)); 
        StartCoroutine(SpinSlot(Slot3.transform, 6f));
        yield return new WaitForSeconds(2f);
        Slot1.transform.rotation = Quaternion.Euler(0, 0, SlotIndex1 * 60f);
        yield return new WaitForSeconds(2f);
        Slot2.transform.rotation = Quaternion.Euler(0, 0, SlotIndex2 * 60f);
        yield return new WaitForSeconds(2f);
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
}

