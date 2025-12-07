using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class NutrientTesterScript : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject statsParent, seedParent, nutrientsParent, radarParent;
    [SerializeField] private TextMeshProUGUI gloamText, terraText, ichorText, waterText;
    [SerializeField] private Image seedImage, checkmarkImage;
    [SerializeField] private RawImage staticVideo;
    [SerializeField] private float minStatic, maxStatic, staticAlphaSpeed;
    [SerializeField] private PopupEvents popup;
    [SerializeField] private GameObject circleImage;
    [Header("Settings")]
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

    //New Radar
    [Header("Radar")]
    private Transform player;
    public float radarRange = 50f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private RectTransform radarPanel;
    [SerializeField] private GameObject radarBar;
    private GameObject radarObject;
    private RadarHandler radarHandler;
    public float rotationSpeed = 5f;
    public LayerMask include, exclude;
    [Header("Pooling")]
    
    public RadarIcon iconPrefab;
    public int preloadAmount = 20;

    private Queue<RadarIcon> pool = new Queue<RadarIcon>();
    private Dictionary<CreatureBehaviorScript, RadarIcon> activeIcons = new();
    
    #region Unity Functions
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

        for (int i = 0; i < preloadAmount; i++)
        {
            CreateIconToPool();
        }

        player = PlayerMovement.Instance.orientation.transform;
        radarObject = new GameObject("RadarParent");
        radarObject.transform.SetParent(player, false);
        radarHandler = radarObject.AddComponent<RadarHandler>();
        radarHandler.circleImage = circleImage;

        radarObject.transform.localRotation = quaternion.Euler(Vector3.zero);
        radarHandler.enabled = false;
    }

    private void Start()
    {
        UpdateTile(null);
        UpdateSeed(null);

        HandleWildernessExit();
    }

    private void OnEnable()
    {
        staticColor.a = maxStatic;
        if(radarObject != null) radarObject.transform.localRotation = quaternion.Euler(Vector3.zero);
        WildernessManager.OnWildernessEnter += HandleWildernessEnter;
        WildernessManager.OnWildernessLeave += HandleWildernessExit;
    }

    private void OnDisable()
    {
        WildernessManager.OnWildernessEnter -= HandleWildernessEnter;
        WildernessManager.OnWildernessLeave -= HandleWildernessExit;
    }

    private void Update()
    {
        HandleStatic();
        if(mode == TesterMode.Radar) NewRadar();
    }

    private void LateUpdate()
    {
        HandleMissingCreatures();
    }

    #endregion

    #region Misc
    private void HandleWildernessEnter()
    {
        //return;
        mode = TesterMode.Radar;
        radarHandler.enabled = true;
        nutrientsParent.SetActive(false);
        radarParent.SetActive(true);
    }

    private void HandleWildernessExit()
    {
        mode = TesterMode.Nutrient;
        radarHandler.enabled = false;
        nutrientsParent.SetActive(true);
        radarParent.SetActive(false);
    }

    private void HandleStatic()
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
    }

    public TesterMode ReturnMode()
    {
        return mode;
    }

    #endregion

    #region Nutrient Tester

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

    #endregion
    
    #region Radar
    private void NewRadar()
    {
        /*Vector3 rotationAmount = new Vector3(0f, rotationSpeed, 0f);
        Vector3 barRotationAmount = new Vector3(0f, 0f, -rotationSpeed);
        radarObject.transform.Rotate(rotationAmount * Time.deltaTime);
        radarBar.transform.Rotate(barRotationAmount * Time.deltaTime);*/

        foreach (var kvp in activeIcons) //kvp == Key Value Pair
        {
            UpdateIconPosition(kvp.Key, kvp.Value);
        }
        
    }

    private RadarIcon CreateIconToPool()
    {
        RadarIcon icon = Instantiate(iconPrefab, radarPanel);
        icon.Init(this);
        icon.gameObject.SetActive(false);
        pool.Enqueue(icon);
        return icon;
    }

    private RadarIcon GetIcon()
    {
        if (pool.Count == 0) CreateIconToPool();
            
        RadarIcon icon = pool.Dequeue();
        icon.ResetIcon();
        return icon;
    }

    public void ReturnToPool(RadarIcon icon)
    {
        icon.gameObject.SetActive(false);
        pool.Enqueue(icon);
    }

    private void UpdateIconPosition(CreatureBehaviorScript creature, RadarIcon icon)
    {
        if(creature == null) return;
        // Compute position relative to player look direction
        Vector3 relativePos = player.InverseTransformPoint(creature.transform.position);

        // Ignore vertical difference
        relativePos.y = 0f;

        // Clamp distance to radar range
        float distance = Mathf.Min(relativePos.magnitude, radarRange);

        if (distance < 0.001f)
        {
            icon.image.rectTransform.anchoredPosition = Vector2.zero;
            return;
        }

        // Normalize and scale
        Vector2 dir = new Vector2(relativePos.x, relativePos.z).normalized;
        float radarRadius = Mathf.Min(radarPanel.rect.width, radarPanel.rect.height) * 0.5f;
        Vector2 uiPos = dir * (distance / radarRange * radarRadius);

        icon.image.rectTransform.anchoredPosition = uiPos;
    }

    public void TriggerEnter(Collider other)
    {
        if (!other.transform.root.TryGetComponent(out CreatureBehaviorScript creature))return;
        if (activeIcons.ContainsKey(creature)) return;
            
        RadarIcon icon = GetIcon();
        activeIcons.Add(creature, icon);
    }

    public void TriggerExit(Collider other)
    {
        if (!other.transform.root.TryGetComponent(out CreatureBehaviorScript creature)) return;

        if (activeIcons.TryGetValue(creature, out var icon))
        {
            activeIcons.Remove(creature);
            icon.FadeOut(fadeDuration);
        }
    }

    public void OnCreatureDestroyed(CreatureBehaviorScript creature)
    {
        if (activeIcons.TryGetValue(creature, out var icon))
        {
            icon.FadeOut(fadeDuration);
            activeIcons.Remove(creature);
        }
    }

    private void HandleMissingCreatures()
    {
        var toRemove = new List<CreatureBehaviorScript>();

        foreach (var kvp in activeIcons)
        {
            if (kvp.Key == null) // creature destroyed
            {
                // Return icon to pool
                ReturnToPool(kvp.Value);
                toRemove.Add(kvp.Key);
            }
            else
            {
                UpdateIconPosition(kvp.Key, kvp.Value);
            }
        }

        // Remove destroyed creatures from dictionary
        foreach (var key in toRemove) activeIcons.Remove(key);
    }

    #endregion
}