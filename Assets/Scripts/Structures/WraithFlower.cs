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

    Transform player;

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
        player = PlayerInteraction.Instance.transform;
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
        if(TimeManager.Instance.isDay)
        {
            if(assignedWraith) assignedWraith.TakeDamage(999);
            Destroy(this.gameObject);
        }
    }

    IEnumerator BurningRose()
    {
        assignedWraith.anim.SetTrigger("Burning");
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
        float distance = Vector3.Distance(player.position, transform.position);
        Color newColor = r.color;
        if(distance > appearRange)
        {
            newColor.a = 0;
            anim.SetBool("Revealed", false);
        }
        else
        {
            newColor.a = 1 - (distance/appearRange);
            anim.SetBool("Revealed", true);
        }
        r.color = newColor;
    }

    void OnDestroy()
    {
        base.OnDestroy();
        if (!gameObject.scene.isLoaded) return; 
    }
}
