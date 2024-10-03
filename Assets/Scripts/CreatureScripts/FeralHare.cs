using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeralHare : CreatureBehaviorScript
{
    public List<CropData> desiredCrops; // what crops does this creature want to eat

    FarmLand foundFarmTile;

    Vector3 jumpPos;

    bool isFleeing = false;
    bool jumpCooldown = false;
    bool isEating = false;
    bool inEatingRange = false;

    float eatingTimeLeft = 5; //how many seconds does it take to eat a crop
    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        StartCoroutine("CropCheck");
    }

    // Update is called once per frame
    void Update()
    {
        //GET RID OF THIS AFTER POC
        if(Input.GetKeyDown("b"))
        {
            gameObject.SetActive(false);
        }

        base.Update();

        if(!jumpCooldown && !isEating)
        {
            StartCoroutine("JumpCooldownTimer");
            float r = Random.Range(0,10f);
            if(r > 2 && foundFarmTile) Hop(foundFarmTile.transform.position);
            else Hop(jumpPos);
        }
        
        float distance;
        distance = Vector3.Distance (player.position, transform.position);
        if(distance <= sightRange) playerInSightRange = true;
        else
        {
            playerInSightRange = false;
        }

        if(foundFarmTile)
        {
            if(foundFarmTile.crop == null)
            {
                foundFarmTile = null;
                return;
            }
            distance = Vector3.Distance (foundFarmTile.transform.position, transform.position);
            if(distance <= 1.5f && !isEating)
            {
                isEating = true;
                StartCoroutine("EatCrop");
            }
            if(distance > 2f) inEatingRange = false;
        } 
        

        if(inEatingRange && eatingTimeLeft > 0 && foundFarmTile)
        {
            var lookPos = foundFarmTile.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2);

            eatingTimeLeft -= Time.deltaTime;
        } 

    }

    IEnumerator CropCheck()
    {
        //search for crops
        yield return new WaitForSeconds(2);
        do
        {
            yield return new WaitForSeconds(10);
            if((foundFarmTile && foundFarmTile.crop) || structManager.allStructs.Count == 0) yield return new WaitForSeconds(5);
            else
            {
                List<FarmLand> availableLands = new List<FarmLand>();
                foreach( StructureBehaviorScript structure in structManager.allStructs)
                {
                    FarmLand potentialFarmTile = structure as FarmLand;
                    if(potentialFarmTile && desiredCrops.Contains(potentialFarmTile.crop)) availableLands.Add(potentialFarmTile);
                }
                if(availableLands.Count > 0)
                {
                    int r = Random.Range(0, availableLands.Count);
                    foundFarmTile = availableLands[r];
                }
            }
        }
        while(gameObject.activeSelf);
    }

    public void Hop(Vector3 destination)
    {
        //hare will jump toward a random direction using physics, using rb.addforce to a random vector3 position in addition to a vector3.up force
        Vector3 jumpDirection = (transform.position - destination).normalized;
        jumpDirection *= -1;
        //ad force yadadada
        float r = Random.Range(170,210f);
        rb.AddForce(Vector3.up * 100);
        rb.AddForce(jumpDirection * r);
        transform.LookAt(destination);

        anim.SetTrigger("IsHopping");


        SearchWanderPoint();

        effectsHandler.OnMove(0.8f);
    }

    public void SearchWanderPoint()
    {
        float x = Random.Range(-5f, 5f);
        float z = Random.Range(-5f, 5f);

        jumpPos = new Vector3(transform.position.x + x, transform.position.y, transform.position.z + z);

        
    }

    IEnumerator JumpCooldownTimer()
    {
        jumpCooldown = true;
        float time = Random.Range(1.5f, 2.2f);
        if(playerInSightRange) yield return new WaitForSeconds(time/2);
        else yield return new WaitForSeconds(time);
        jumpCooldown = false;
    }

    IEnumerator EatCrop()
    {
        inEatingRange = true;
        anim.SetBool("IsDigging", true);
        eatingTimeLeft = 5;
        transform.LookAt(foundFarmTile.transform.position);
        yield return new WaitUntil(() => !inEatingRange || eatingTimeLeft <= 0 || foundFarmTile.crop == null);
        if(inEatingRange && foundFarmTile.crop != null)
        {
            foundFarmTile.CropDestroyed();
            foundFarmTile = null;
            inEatingRange = false;
        }
        anim.SetBool("IsDigging", false);
        isEating = false;
    }
}
