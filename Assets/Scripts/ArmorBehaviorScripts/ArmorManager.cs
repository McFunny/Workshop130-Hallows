using UnityEngine;

public class ArmorManager : MonoBehaviour
{
    public static ArmorManager Instance;

    public GameObject miningLight;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        Instance = this;
    }
}
