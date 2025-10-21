using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager instance;
    [SerializeField] private List<Levels> levels = new List<Levels>();
    public delegate void OnLevelUp();
    public delegate void OnXpGain();
    public event OnLevelUp onLevelUp;
    public event OnXpGain onXpGain;

    private int currentXP = 0;
    private int totalXP = 0;
    private int currentLevel = 1;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentXP = 0;
        currentLevel = 1;
    }

    public void AddXP(int amount)
    {
        totalXP += amount;
        
        if (currentLevel == levels.Count)
        {
            Debug.Log("Max level reached. No more XP can be added.");
            return;
        }
        
        currentXP += amount;
        onXpGain?.Invoke();

        if (currentXP >= levels[currentLevel - 1].xpToNextLevel)
        {
            currentXP -= levels[currentLevel - 1].xpToNextLevel;
            currentLevel++;
            onLevelUp?.Invoke();
        } 
    }

    public int ReturnXP()
    {
        return currentXP;
    }

    public int ReturnLevel()
    {
        return currentLevel;
    }

    public int ReturnTotalXP()
    {
        return totalXP;
    }

    public ReadOnlyCollection<Levels> LevelsList //WOW READ ONLY LIST!!! SO AWESOME SAUCE!!!
    {
        get { return levels.AsReadOnly(); }
    }
}

[System.Serializable]
public class Levels
{
    public int level = 0;
    public int xpToNextLevel = 100;
}
