using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NutrientTesterScript : MonoBehaviour
{
    [SerializeField] private GameObject statsParent, seedParent, nutrientsParent, radarParent;
    [SerializeField] private TextMeshProUGUI gloamText, terraText, ichorText, waterText;
    [SerializeField] private Image seedImage, checkmarkImage;
    [SerializeField] private RawImage staticVideo;
    [SerializeField] private float minStatic, maxStatic, staticAlphaSpeed;
    [SerializeField] private PopupEvents popup;
    [Header("Settings")]
    [SerializeField] private float radius;
    [SerializeField] private RectTransform radarPanel;
    [SerializeField] private float radarRange = 50f;
    [SerializeField] private RectTransform iconPrefab;
    public int initialPoolSize = 20;
    public static NutrientTesterScript Instance;
    private CropItem currentSeed = null;
    private NutrientStorage currentNutrients = null;
    private Color staticColor = new Color(1f,1f,1f,1f);
    public enum TesterMode
    {
        Nutrient,
        Radar
    }
    private TesterMode mode = TesterMode.Nutrient;
    private Transform player;
    

    private Dictionary<GameObject, RectTransform> iconMap = new Dictionary<GameObject, RectTransform>();
    private HashSet<GameObject> trackedObjects = new HashSet<GameObject>();
    private Queue<RectTransform> iconPool = new Queue<RectTransform>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogError("HELP!!! TOO MANY INSTANCES!!! HEEEEEEELP!!!");
        }

        Debug.Log("Nutrient Tester Instance: " + Instance);
        HandleWildernessExit();
    }

    private void Start()
    {
        UpdateTile(null);
        UpdateSeed(null);

        for (int i = 0; i < initialPoolSize; i++)
        {
            RectTransform icon = Instantiate(iconPrefab, radarPanel);
            icon.gameObject.SetActive(false);
            iconPool.Enqueue(icon);
        }

        player = PlayerMovement.Instance.orientation.transform;
    }

    private void OnEnable()
    {
        staticColor.a = maxStatic;
        WildernessManager.OnWildernessEnter += HandleWildernessEnter;
        WildernessManager.OnWildernessLeave += HandleWildernessExit;
    }

    private void OnDisable()
    {
        WildernessManager.OnWildernessEnter -= HandleWildernessEnter;
        WildernessManager.OnWildernessLeave -= HandleWildernessExit;
    }

    private void HandleWildernessEnter()
    {
        return;
        mode = TesterMode.Radar;
        nutrientsParent.SetActive(false);
        radarParent.SetActive(true);
    }

    private void HandleWildernessExit()
    {
        mode = TesterMode.Nutrient;
        nutrientsParent.SetActive(true);
        radarParent.SetActive(false);
    }

    private void Update()
    {
        staticVideo.color = staticColor;

        if (staticVideo.color.a > minStatic)
        {
            staticColor.a = Mathf.Lerp(staticVideo.color.a, minStatic, staticAlphaSpeed);
        }
        else
        {
            staticColor.a = minStatic;
        }

        if(mode != TesterMode.Radar) return;
        Radar();
    }  

    public void UpdateSeed(CropItem seed)
    {
        currentSeed = seed;
        
        if (seed == null)
        {
            seedParent.SetActive(false);
            return;
        }

        seedImage.sprite = seed.icon;

        //Debug.Log(currentSeed);
        //Debug.Log(seedData);

        if (currentNutrients != null)
        {
            bool _canFullyGrow = true;
            CropData seedData = currentSeed.cropData;

            if (seedData.gloamIntake * seedData.growthStages > currentNutrients.gloamLevel)
            {
                _canFullyGrow = false;
            }

            if (seedData.terraIntake * seedData.growthStages > currentNutrients.terraLevel)
            {
                _canFullyGrow = false;
            }

            if (seedData.ichorIntake * seedData.growthStages > currentNutrients.ichorLevel)
            {
                _canFullyGrow = false;
            }

            if (_canFullyGrow) checkmarkImage.color = Color.green;
            else checkmarkImage.color = Color.red;
        }
        else
        {
            checkmarkImage.color = Color.clear;
        }

        seedParent.SetActive(true);
    }

    public void UpdateTile(NutrientStorage nutrients)
    {
        staticColor.a = maxStatic;

        if (nutrients == null)
        {
            statsParent.SetActive(false);
            return;
        }

        currentNutrients = nutrients;

        gloamText.text = nutrients.gloamLevel + "/10";
        terraText.text = nutrients.terraLevel + "/10";
        ichorText.text = nutrients.ichorLevel + "/10";
        waterText.text = nutrients.waterLevel + "/10";
        statsParent.SetActive(true);

        UpdateSeed(currentSeed);
    }

    private void Radar()
    {
        radarPanel.localRotation = Quaternion.Euler(0, 0, -player.eulerAngles.y);
        List<CreatureBehaviorScript> creatures = WildernessManager.Instance.allCreatures.Concat(NightSpawningManager.Instance.allCreatures).ToList();

        foreach (CreatureBehaviorScript other in creatures)
        {
            var tracked = other.gameObject;
            float dist = Vector3.Distance(other.transform.position, player.position);
            bool inRange = dist <= radarRange;

            // Handle entering range
            if (inRange && !trackedObjects.Contains(tracked))
            {
                trackedObjects.Add(tracked);

                RectTransform icon = GetIconFromPool();
                icon.gameObject.SetActive(true);
                iconMap.Add(tracked, icon);
            }

            // Handle leaving range
            else if (!inRange && trackedObjects.Contains(tracked))
            {
                trackedObjects.Remove(tracked);

                if (iconMap.TryGetValue(tracked, out RectTransform oldIcon))
                {
                    ReturnIcon(oldIcon);
                    iconMap.Remove(tracked);
                }
            }

            // Update position if tracked
            if (inRange && iconMap.TryGetValue(tracked, out RectTransform iconToMove))
            {
                Vector3 offset = other.transform.position - player.position;

                float scaledX = (offset.x / radarRange) * (radarPanel.sizeDelta.x / 2);
                float scaledY = (offset.z / radarRange) * (radarPanel.sizeDelta.y / 2);

                iconToMove.anchoredPosition = new Vector2(scaledX, scaledY);
            }
        }
    }

    RectTransform GetIconFromPool()
    {
        if (iconPool.Count > 0) return iconPool.Dequeue();
            
        // This should be really rare lol
        RectTransform icon = Instantiate(iconPrefab, radarPanel);
        icon.gameObject.SetActive(false);
        return icon;
    }


    void ReturnIcon(RectTransform icon)
    {
        icon.gameObject.SetActive(false);
        iconPool.Enqueue(icon);
    }

    public TesterMode ReturnMode()
    {
        return mode;
    }
}
