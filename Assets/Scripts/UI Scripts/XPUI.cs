using UnityEngine;
using TMPro;

public class XPUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI level;
    [SerializeField] private TextMeshProUGUI xp;
    private void Start()
    {
        UpdateXPUI();
        XPManager.instance.onLevelUp += UpdateXPUI;
        XPManager.instance.onXpGain += UpdateXPUI;
    }

    void OnEnable()
    {
        if (XPManager.instance == null) return;
        XPManager.instance.onLevelUp += UpdateXPUI;
        XPManager.instance.onXpGain += UpdateXPUI;
    }

    void OnDisable()
    {
        XPManager.instance.onLevelUp -= UpdateXPUI;
        XPManager.instance.onXpGain -= UpdateXPUI;
    }

    private void UpdateXPUI()
    {
        int currentLevel = XPManager.instance.ReturnLevel();
        int currentXP = XPManager.instance.ReturnXP();
        int xpToNextLevel = 0;

        if (currentLevel < XPManager.instance.LevelsList.Count)
        {
            xpToNextLevel = XPManager.instance.LevelsList[currentLevel - 1].xpToNextLevel;
        }
        else
        {
            xp.gameObject.SetActive(false);
        }  

        level.text = "Level: " + currentLevel.ToString();
        xp.text = "XP: " + currentXP.ToString() + "/" + xpToNextLevel.ToString();
    }

    
}
