using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PetStatsUI : MonoBehaviour //Not finished yet lmao
{
    public bool isActive = false;
    public enum CreatureType
    {
        Pet,
        Critter
    }
    public CreatureType creatureType = CreatureType.Pet;
    [SerializeField] private CropStatsRework cropStatsRework;
    public GameObject statsContainer, petsObject, crittersObject;
    [SerializeField] private float reach = 8f;
    [SerializeField] private UILerpHandler lerpHandler;
    [Header("Pet Stats References")]
    [SerializeField] private Image petIcon;
    [SerializeField] private TextMeshProUGUI petNameText;
    [SerializeField] private TextMeshProUGUI petTypeText;
    [SerializeField] private TextMeshProUGUI petHungerText;
    [SerializeField] private TextMeshProUGUI petThirstText;
    [SerializeField] private TextMeshProUGUI petFriendshipText;
    [SerializeField] private Image petHeartIcon;
    [SerializeField] private Slider petHungerSlider, petThirstSlider;
    [Header("Critter Stats References")]
    [SerializeField] private Image critterIcon;
    [SerializeField] private TextMeshProUGUI critterNameText;
    [SerializeField] private TextMeshProUGUI critterTypeText;
    [SerializeField] private TextMeshProUGUI critterHealthText;
    [SerializeField] private TextMeshProUGUI critterHungerText;
    [SerializeField] private TextMeshProUGUI critterThirstText;
    [SerializeField] private TextMeshProUGUI critterFriendshipText;
    [SerializeField] private Image critterHomeIcon, critterHeartIcon;
    [SerializeField] private Slider critterHealthSlider, critterHungerSlider, critterThirstSlider;
    private Camera mainCam;
    public delegate void PetStatsShown();
    public event PetStatsShown OnPetStatsShown;
    private int layerMask = (1 << 6 | 1 << 9);
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        StartCoroutine(CheckTimer());
    }

    // Update is called once per frame
    void Update()
    {
        PetCheck();

        switch (creatureType)
        {
            case CreatureType.Pet:
                petsObject.SetActive(true);
                crittersObject.SetActive(false);
                break;
            case CreatureType.Critter:
                crittersObject.SetActive(true);
                petsObject.SetActive(false);
                break;
        }

        lerpHandler.lerpToStartArray[1] = isActive; //This is stupid but it works
    }

    IEnumerator CheckTimer()
    {
        do
        {
            PetCheck();
            yield return new WaitForSeconds(0.2f);
        }
        while(gameObject.activeSelf);
    }
    
    void PetCheck()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, layerMask))
        {
            PetBehaviorScript pet = hit.collider.GetComponentInParent<PetBehaviorScript>();
            CritterBehaviorScript critter = hit.collider.GetComponentInParent<CritterBehaviorScript>();
            if (pet != null)
            {
                isActive = true;
                creatureType = CreatureType.Pet;
                OnPetStatsShown?.Invoke();
                petNameText.text = pet.name;
                petTypeText.text = pet.petType.ToString();
                petHungerText.text = pet.hunger + "/" + pet.maxHunger;
                petThirstText.text = pet.thirst + "/" + pet.maxThirst;
                petHungerSlider.value = pet.hunger / pet.maxHunger;
                petThirstSlider.value = pet.thirst / pet.maxThirst;
                petFriendshipText.text = pet.friendshipLevel.ToString();
            }
            else if (critter != null)
            {
                isActive = true;
                creatureType = CreatureType.Critter;
                OnPetStatsShown?.Invoke();
                critterNameText.text = critter.name;
                critterHomeIcon.gameObject.SetActive(!critter.IsCritterHomeless());
                critterTypeText.text = critter.creatureData.name.ToString();
                critterHealthText.text = critter.health + "/" + critter.maxHealth;
                critterHungerText.text = critter.hunger + "/" + critter.maxHunger;
                critterThirstText.text = critter.thirst + "/" + critter.maxThirst;
                petHungerSlider.value = critter.health / critter.maxHealth;
                petHungerSlider.value = critter.hunger / critter.maxHunger;
                critterThirstSlider.value = critter.thirst / critter.maxThirst;
                critterFriendshipText.text = critter.friendshipLevel.ToString();
            }
            else
            {
                isActive = false;
            }
            }
        else
        {
            isActive = false;
        }
    }
    
}
