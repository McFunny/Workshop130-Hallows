using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollinatorPost : StructureBehaviorScript
{
    public SpriteRenderer r;

    public List<Pottable> potItems;

    public GameObject fogChimeLight;
    public InventoryItemData fogChime;

    public void Awake()
    {
        base.Awake();
        fogChimeLight.SetActive(false);
        savedItems.Add(null);
        r.sprite = null;
    }

    public void Start()
    {
        base.Start();
        LoadVariables();
    }
}
