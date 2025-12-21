using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSplatPool : MonoBehaviour
{
    [SerializeField] ScreenSplat splatPrefab;
    [SerializeField] int initialSize = 10;

    readonly Queue<ScreenSplat> pool = new();

    void Awake()
    {
        for (int i = 0; i < initialSize; i++)
            CreateNew();
    }

    ScreenSplat CreateNew()
    {
        ScreenSplat splat = Instantiate(splatPrefab, transform);
        splat.gameObject.SetActive(false);
        splat.Init(this);
        pool.Enqueue(splat);
        return splat;
    }

    public ScreenSplat Get()
    {
        if (pool.Count == 0)
            CreateNew();

        ScreenSplat splat = pool.Dequeue();
        splat.gameObject.SetActive(true);
        return splat;
    }

    public void Release(ScreenSplat splat)
    {
        splat.gameObject.SetActive(false);
        pool.Enqueue(splat);
    }
}
