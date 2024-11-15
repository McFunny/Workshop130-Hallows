using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance;

    public GameObject bulletPrefab;

    public List<GameObject> bulletPool = new List<GameObject>();

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }

        PopulateBulletPool();
    }

    void PopulateBulletPool()
    {
        //

        for(int i = 0; i < 12; i++)
        {
            GameObject newBullet = Instantiate(bulletPrefab);
            bulletPool.Add(newBullet);
            newBullet.SetActive(false);
        }
    }

    public GameObject GrabBullet()
    {
        foreach (GameObject bullet in bulletPool)
        {
            if(!bullet.activeSelf)
            {
                bullet.SetActive(true);
                bullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                return bullet;
            }
        }

        //No available items, must make a new one
        GameObject newBullet = Instantiate(bulletPrefab);
        bulletPool.Add(newBullet);
        newBullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        return newBullet;
    }
}
