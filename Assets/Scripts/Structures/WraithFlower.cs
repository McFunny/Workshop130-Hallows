using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WraithFlower : StructureBehaviorScript
{
    public Wraith assignedWraith;
    public Animator anim;

    public SpriteRenderer r;

    public float appearRange = 5;

    public Collider collider; //Disable once burning to prevent being extinguished

    bool isBurning = false;

    //Potential feature is applying fertalizer to the rose for various effects
    //Use small circle particles from mancer particle for this

    void Awake()
    {
        base.Awake();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        UpdateTransparency();

        if(onFire && !isBurning)
        {
            isBurning = true;
            collider.enabled = false;
            StartCoroutine(BurningRose());
        }
    }

    IEnumerator BurningRose()
    {
        yield return new WaitForSeconds(2);
        if(assignedWraith)
        {
            assignedWraith.canCorpseBreak = true;
            assignedWraith.TakeDamage(999);
        }
        Destroy(this.gameObject);
    }

    void UpdateTransparency()
    {
        //calculate player distance
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
    }
}
