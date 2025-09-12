using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;


public class StructureStatsUI : MonoBehaviour
{
    public GameObject structureStatContainer;
    [SerializeField] private UILerpHandler lerpHandler;
    [SerializeField] private LayerMask layer;
    [SerializeField] private float reach = 8;
    [SerializeField] private TextMeshProUGUI structureNameText;
    [SerializeField] private Image structureIcon;
    [SerializeField] private List<StructureStatObjects> statList = new List<StructureStatObjects>();

    private bool isActive;
    private Camera mainCam;
    private StructureBehaviorScript hitStructure;
    public delegate void StructureStatsShown();
    public event StructureStatsShown OnStructureStatsShown;

    private void Start()
    {
        mainCam = Camera.main;
        StartCoroutine(CheckTimer());
    }


    void Update()
    {
        lerpHandler.lerpToStartArray[2] = isActive; //This is stupid but it works
    }

    IEnumerator CheckTimer()
    {
        do
        {
            StructureCheck();
            yield return new WaitForSeconds(0.2f);
        }
        while (gameObject.activeSelf);
    }
    void StructureCheck()
    {
        Vector3 fwd = mainCam.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(mainCam.transform.position, fwd, out hit, reach, layer))
        {

            hitStructure = hit.collider.GetComponentInParent<StructureBehaviorScript>();

            if (hitStructure == null) return;

            var structureStats = hitStructure.GetStructureUIValues();
            var structureItemData = hitStructure.itemForm;

            if (structureStats == null || !hitStructure.structureUIVariables.enableUI)
            {
                isActive = false;
                return;
            }

            var iterations = 0;

            for (int i = 0; i < structureStats.Count; i++)
            {
                //Debug.Log(structureStats[i].name, hitStructure);

                structureNameText.text = structureItemData.displayName;
                structureIcon.sprite = structureItemData.icon;

                statList[i].statNameText.text = structureStats[i].name;
                statList[i].statIcon.sprite = structureStats[i].icon;
                statList[i].statSlider.value = structureStats[i].value / structureStats[i].maxValue;
                statList[i].statValueText.text = structureStats[i].value + "/" + structureStats[i].maxValue;
                statList[i].fillImage.color = structureStats[i].barColor;

                statList[i].baseObject.SetActive(true);

                iterations++;
            }

            for (int i = 0; i < 3; i++)
            {
                if (i < iterations) continue;

                statList[i].baseObject.SetActive(false);
            }
            OnStructureStatsShown?.Invoke();
            isActive = true;
            //Debug.Log(hitStructure, hitStructure);

        }
        else
        {
            isActive = false;
        }
    }
}

[System.Serializable]
public class StructureStatObjects
{
    public GameObject baseObject;
    public TextMeshProUGUI statNameText, statValueText;
    public Image statIcon;
    public Slider statSlider;
    public Image fillImage;
}
