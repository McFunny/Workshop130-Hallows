using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class PachinkoManager : MonoBehaviour, IInteractable
{
    public static PachinkoManager Instance;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public List<GameObject> highlight = new List<GameObject>();
    List<Material> highlightMaterial = new List<Material>();
    bool highlightEnabled;

    public int bugsInserted;
    public GameObject activeBug;
    public int totalWinnings;

    public Transform bugSpawnPoint;
    public GameObject bugPrefab;

    public List<float> forceAmounts = new List<float>();
    public float chargeDuration = 3f;
    public float returnDuration = 0.3f;
    public float pullDistance = 0.8f;

    private AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip releaseSound;

    private bool isCharging = false;
    private float chargeStartTime;
    private Vector3 originalPosition;

    private int lastPlayedIndex = -1;

    public TextMeshProUGUI bugsInsertedText;
    public TextMeshProUGUI PointsAwardedText;
    public TextMeshProUGUI totalWinningsText;

    private bool bugReadyToLaunch = false;
    private bool interacting = false;

    public Transform _focalPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        originalPosition = transform.localPosition;
        audioSource = GetComponent<AudioSource>();
        UpdateText();
    }

    private void Update()
    {
        if (activeBug && bugReadyToLaunch && interacting && InputManager.isHoldingInteract && !isCharging)
        {
            StartCharging();
        }

        if (isCharging)
        {
            if (InputManager.isHoldingInteract)
            {
                float chargeTime = Mathf.Clamp(Time.time - chargeStartTime, 0, chargeDuration);
                float t = chargeTime / chargeDuration;
                transform.localPosition = originalPosition + new Vector3(-pullDistance * t, 0f, 0f);

                int currentIndex = Mathf.Clamp(Mathf.FloorToInt(t * forceAmounts.Count), 0, forceAmounts.Count - 1);

                if (currentIndex != lastPlayedIndex)
                {
                    lastPlayedIndex = currentIndex;
                    if (audioSource != null && clickSound != null)
                    {
                        audioSource.pitch = 1f + (currentIndex / (float)forceAmounts.Count);
                        audioSource.PlayOneShot(clickSound);
                    }
                }
            }
            else if (bugReadyToLaunch)
            {
                float chargeTime = Mathf.Clamp(Time.time - chargeStartTime, 0, chargeDuration);
                float t = chargeTime / chargeDuration;
                FireBug(t);
                if (audioSource != null && releaseSound != null)
                {
                    audioSource.pitch = 1f;
                    audioSource.PlayOneShot(releaseSound);
                }
                lastPlayedIndex = -1;
            }
        }
    }

    private void StartCharging()
    {
        isCharging = true;
        chargeStartTime = Time.time;
        lastPlayedIndex = -1;
    }

    private void FireBug(float chargeT)
    {
        isCharging = false;
        bugReadyToLaunch = false;

        if (activeBug)
        {
            int forceIndex = Mathf.Clamp(Mathf.FloorToInt(chargeT * forceAmounts.Count), 0, forceAmounts.Count - 1);
            float selectedForce = forceAmounts[forceIndex];

            Rigidbody bugRB = activeBug.GetComponent<Rigidbody>();
            Vector3 direction = activeBug.transform.forward;
            bugRB.AddForce(direction.normalized * selectedForce, ForceMode.Impulse);

            StartCoroutine(ReturnLauncher());
            StartCoroutine(FailsafeTimer());

            Debug.Log($"Fired bug with force index {forceIndex} ({selectedForce})");
        }
    }

    private IEnumerator ReturnLauncher()
    {
        Vector3 start = transform.localPosition;
        Vector3 end = originalPosition;
        float elapsed = 0f;

        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            transform.localPosition = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.localPosition = end;
    }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
        interacting = true;
    }

    public void EndInteraction()
    {
        interacting = false;
    }

    public void SpawnBug()
    {
        if (activeBug) return;

        activeBug = Instantiate(bugPrefab, bugSpawnPoint.position, Quaternion.identity);
        bugsInserted--;
        bugReadyToLaunch = true;
        UpdateText();
    }

    public void NotifyBugDestroyed(bool wasWinning, int rewardAmount = 0)
    {
        activeBug = null;

        if (wasWinning)
        {
            StartCoroutine(FlashPointsAwarded(rewardAmount));
        }
        else
        {
            ContinueIfBacklog();
        }
    }

    private IEnumerator FlashPointsAwarded(int rewardAmount)
    {
        for (int i = 0; i < 3; i++)
        {
            PointsAwardedText.text = "+" + rewardAmount.ToString();
            PointsAwardedText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.2f);
            PointsAwardedText.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
        }

        ContinueIfBacklog();
    }

    private IEnumerator FailsafeTimer()
    {
        yield return new WaitForSeconds(17f);
        if (activeBug == null)
        {
            NotifyBugDestroyed(false);
        }
    }

    private void ContinueIfBacklog()
    {
        if (bugsInserted > 0)
        {
            SpawnBug();
        }
        else
        {
            UpdateText();
        }
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
    }

    public void ReturnFocalPoint(out Transform point)
    {
        point = _focalPoint;
    }

    public void ToggleHighlight(bool enable)
    {
        if (highlight.Count == 0) return;
        if (highlightMaterial.Count == 0)
        {
            foreach (GameObject thing in highlight) highlightMaterial.Add(highlight[0].GetComponentInChildren<MeshRenderer>().material);
        }
        if (enable && !highlightEnabled)
        {
            highlightEnabled = true;
            foreach (GameObject thing in highlight) thing.SetActive(true);
            StartCoroutine(HightlightFlash());
        }

        if (!enable && highlightEnabled)
        {
            interacting = false;
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
            } while (power > 0.7f && highlightEnabled);

            do
            {
                yield return new WaitForSeconds(0.1f);
                power += 0.05f;
                foreach (Material mat in highlightMaterial) mat.SetFloat("_Fresnel_Power", power);
            } while (power < 1.9f && highlightEnabled);
        }
    }

    public void UpdateText()
    {
        bugsInsertedText.text = bugsInserted.ToString();
        totalWinningsText.text = totalWinnings.ToString();
    }
}
