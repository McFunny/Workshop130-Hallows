using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public Camera mainCam;

    public GameObject testStructure;
    public InventoryItemData testItem;

    StructureManager structManager;

    public bool isInteracting { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        if(!mainCam) mainCam = FindObjectOfType<Camera>();
        structManager = FindObjectOfType<StructureManager>();
    }



    // Update is called once per frame
    void Update()
    {
        //ITEM INTERACTION
        if(Input.GetMouseButtonDown(0))
        {
            Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
            RaycastHit hit;

            if(Physics.Raycast(mainCam.transform.position, fwd, out hit, 10, 1 << 7))
            {
                Vector3 pos = structManager.CheckTile(hit.point);
                if(pos != new Vector3(0,0,0)) structManager.SpawnStructure(testStructure, pos);
            }
        }

        //STRUCT INTERACTION
        if(Input.GetMouseButtonDown(1))
        {
            StructureInteraction();
            //TO TEST CLEARING A STRUCTURE
            //DestroyStruct();
        }

        if(Input.GetKeyDown("f"))
        {
            Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
            RaycastHit hit;


            if (Physics.Raycast(mainCam.transform.position, fwd, out hit, 10, 1 << 6))
            {
                var structure = hit.collider.GetComponent<StructureBehaviorScript>();
                if (structure != null)
                {
                    structure.StructureInteraction();
                    Debug.Log("Tried to harvest plant");
                }
            }
        }

    }

    void StartInteraction(IInteractable interactable)
    {
        interactable.Interact(this, out bool interactSuccessful);
        isInteracting = true;
    }

    void EndInteraction()
    {
        isInteracting = false;
    }

    void DestroyStruct()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;

        if(Physics.Raycast(mainCam.transform.position, fwd, out hit, 10, 1 << 6))
        {
            Destroy(hit.collider.gameObject);
        }
    }

    void StructureInteraction()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;


        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, 10, 1 << 6))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();
            GameObject interactObject = hit.collider.gameObject;
            if (interactable != null)
            {
                StartInteraction(interactable);
                PlayerMovement.accessingInventory = true;  // Assuming this controls the inventory UI
                Debug.Log("Opened Inventory of Interactable Object");
                return;
            }

            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                structure.ItemInteraction(testItem);
                Debug.Log("Added crop to farmland");
                if (interactObject.tag ==  "NPC") // Also this teehee!!!
                {
                    StartInteraction(interactable);
                    Debug.Log("NPC Interacted");
                }  
                else
                {
                    StartInteraction(interactable);
                    PlayerMovement.accessingInventory = true;  // Assuming this controls the inventory UI
                    Debug.Log("Opened Inventory of Interactable Object");
                }
            }
        }
    }

    
}
