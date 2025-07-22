using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gachapon : MonoBehaviour
{
   public List<Winnings> winnings = new List<Winnings>();
   public int winIndex = 0;

    public static Gachapon Instance;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }
    public void OnPuzzleCompletion()
    {

    }
}

public class Winnings
{
    public List<InventoryItemData> data;
    public int mintsReward;
}
