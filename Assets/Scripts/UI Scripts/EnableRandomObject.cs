using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableRandomObject : MonoBehaviour
{
    public List<GameObject> gameObjects;
    public bool enableOnStart;
    public Camera camera;

    void Start()
    {
        if (!enableOnStart) return;
        if (gameObjects == null) return;

        for (int i = 0; i < gameObjects.Count; i++)
        {
            gameObjects[i].SetActive(false);
        }

        int r = Random.Range(0, gameObjects.Count);
        gameObjects[r].SetActive(true);
    }
}
