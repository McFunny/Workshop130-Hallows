using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePoolManager : MonoBehaviour
{
    public static ProjectilePoolManager Instance;

    public GameObject bulletPrefab, largeWaterPrefab, seedBulletPrefab, fireBallPrefab;

    List<GameObject> bulletPool = new List<GameObject>();
    List<GameObject> largeWaterPool = new List<GameObject>();
    List<GameObject> seedPool = new List<GameObject>();
    List<GameObject> fireBallPool = new List<GameObject>();

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

        for(int i = 0; i < 12; i++)
        {
            GameObject newBullet = Instantiate(largeWaterPrefab);
            largeWaterPool.Add(newBullet);
            newBullet.SetActive(false);
        }

        for(int i = 0; i < 12; i++)
        {
            GameObject newBullet = Instantiate(seedBulletPrefab);
            seedPool.Add(newBullet);
            newBullet.SetActive(false);
        }

        for(int i = 0; i < 12; i++)
        {
            GameObject newBullet = Instantiate(fireBallPrefab);
            fireBallPool.Add(newBullet);
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

        //No available projectiles, must make a new one
        GameObject newBullet = Instantiate(bulletPrefab);
        bulletPool.Add(newBullet);
        newBullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        return newBullet;
    }

    public GameObject GrabLargeWater()
    {
        foreach (GameObject bullet in largeWaterPool)
        {
            if(!bullet.activeSelf)
            {
                bullet.SetActive(true);
                bullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                return bullet;
            }
        }

        //No available projectiles, must make a new one
        GameObject newBullet = Instantiate(largeWaterPrefab);
        largeWaterPool.Add(newBullet);
        newBullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        return newBullet;
    }

    public GameObject GrabSeedBullet()
    {
        foreach (GameObject bullet in seedPool)
        {
            if(!bullet.activeSelf)
            {
                bullet.SetActive(true);
                bullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                return bullet;
            }
        }

        //No available projectiles, must make a new one
        GameObject newBullet = Instantiate(seedBulletPrefab);
        seedPool.Add(newBullet);
        newBullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        return newBullet;
    }

    public GameObject GrabFireBall()
    {
        foreach (GameObject bullet in fireBallPool)
        {
            if(!bullet.activeSelf)
            {
                bullet.SetActive(true);
                bullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
                return bullet;
            }
        }

        //No available projectiles, must make a new one
        GameObject newBullet = Instantiate(fireBallPrefab);
        fireBallPool.Add(newBullet);
        newBullet.GetComponent<Rigidbody>().velocity = new Vector3(0,0,0);
        return newBullet;
    }
}
