using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/WaterGun")]
public class WaterGunBehavior : ToolBehavior
{
    public AudioClip shoot, charge, refill;
    int bulletCount = 1;

    Transform bulletStart;

    public GameObject targetHighlight;
    public int range = 3;
    List<GameObject> highlights = new List<GameObject>(); //Does this cause an issue if the game is exited midcharge?

    float speed = 240;
    float bulletSpread = 0.2f;
    bool maxCharge = false;
    Coroutine shootingGunCoroutine;
    Coroutine chargingCoroutine;

    //Do we want the multishot to only consume a unit of water or water for each shot?


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();

        if(PlayerInteraction.Instance.waterHeld == 0) return;
        
        tool = _tool;
        usingPrimary = true;
        maxCharge = false;

        if(highlights.Count != range)
        {
            int dif = range - highlights.Count;
            for(int i = 0; i < dif; i++)
            {
                highlights.Add(Instantiate(targetHighlight));
            }
        }
        for(int i = 0; i < highlights.Count; i++)
        {
            if(highlights[i] == null) highlights[i] = Instantiate(targetHighlight);
            highlights[i].SetActive(false);
        }
        //Shoot
        //HandItemManager.Instance.PlayPrimaryAnimation();
        toolAnim.SetBool("Charging", true);
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.0f, 0.5f));
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5) return;
        if (!player) player = _player;
        toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        tool = _tool;

        //GainWater
        Vector3 fwd = player.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(player.position, fwd, out hit, 8, mask))
        {
            var structure = hit.collider.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                //play success anim
                bool playAnim = false;
                structure.ToolInteraction(tool, out playAnim);
                if(playAnim)
                {
                    //HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.8f, 1.3f));
                    PlayerMovement.restrictMovementTokens++;
                    PlayerInteraction.Instance.StaminaChange(-2);
                    usingSecondary = true;
                    toolAnim.Play("Reload");
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                } 
            }

            var interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.InteractWithItem(PlayerInteraction.Instance, out bool interactSuccessful, HotbarDisplay.currentSlot.AssignedInventorySlot.ItemData);
                if(interactSuccessful)
                {
                    //HandItemManager.Instance.PlayPrimaryAnimation();
                    HandItemManager.Instance.toolSource.PlayOneShot(refill);
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.8f, 1.8f));
                    PlayerMovement.restrictMovementTokens++;
                    PlayerInteraction.Instance.StaminaChange(-2);
                    usingSecondary = true;
                    toolAnim.Play("Reload");
                    PlayerCam.Instance.NewObjectOfInterest(hit.transform.position);
                    return;
                }

            }
        }

    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            if(shootingGunCoroutine == null) 
            {
                shootingGunCoroutine = HandItemManager.Instance.StartCoroutine(ShootGun());
                chargingCoroutine = HandItemManager.Instance.StartCoroutine(ChargeTimer());
            }
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            PlayerMovement.restrictMovementTokens--;
            PlayerCam.Instance.ClearObjectOfInterest();
        }

    }

    public IEnumerator ChargeTimer()
    {
        bulletCount = 0;
        Debug.Log("0");
        yield return new WaitForSeconds(0.35f);
        toolAnim.SetBool("EarlyFire", true);
        bulletCount = 1;
        Debug.Log("1");
        yield return new WaitForSeconds(0.65f);
        if(InputManager.isCharging)
        {
            Debug.Log("Showing Range");
            HandItemManager.Instance.StartCoroutine(ShowRange());
            maxCharge = true;
        }
    }

    public IEnumerator ShowRange()
    {
        Vector3 currentPos = new Vector3(0,0,0);
        Vector3 newPos;

        Direction currentDirection = Direction.Null;
        Direction newDirection;
        List<Vector3> targets = new List<Vector3>();
        while(InputManager.isCharging && usingPrimary)
        {
            newPos = StructureManager.Instance.GetTileCenter(player.position);
            newDirection = GetDirection();
            //Debug.Log(player.eulerAngles.x);
            if((currentPos != newPos || currentDirection != newDirection) && newPos != new Vector3(0,0,0) && (player.eulerAngles.x <= 90 && player.eulerAngles.x >= 10))
            {
                currentPos = newPos;
                currentDirection = newDirection;
                //Debug.Log(currentDirection);
                targets = StructureManager.Instance.WaterGunTargets(currentPos, currentDirection, range);
                if(targets.Count > 0)
                {
                    for(int i = 0; i < targets.Count; i++)
                    {
                        bulletCount = range;
                        highlights[i].SetActive(true);
                        highlights[i].transform.position = targets[i];
                    }
                }
            }
            else if(newPos == new Vector3(0,0,0) || (player.eulerAngles.x > 90 || player.eulerAngles.x < 10)) 
            {
                currentPos = new Vector3(0,0,0);
                currentDirection = Direction.Null;

                foreach(GameObject light in highlights)
                {
                    bulletCount = 1;
                    light.SetActive(false);
                } 
            }
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitUntil(() => !usingPrimary);
        foreach(GameObject light in highlights)
        {
            light.SetActive(false);
        }
    }

    public IEnumerator ShootGun()
    {
        yield return new WaitUntil(() => !InputManager.isCharging);
        //yield return new WaitForSeconds(0.01f);
        HandItemManager.Instance.StopCoroutine(chargingCoroutine);
        chargingCoroutine = null;

        if(bulletCount == 1) toolAnim.SetTrigger("Fire1");
        if(bulletCount == 3) toolAnim.SetTrigger("Fire3");
        if(bulletCount == 5) toolAnim.SetTrigger("Fire5");
        toolAnim.SetBool("Charging", false);

        if(bulletCount == 0)
        {
            usingPrimary = false;
            shootingGunCoroutine = null;
            
            yield break; //exit the coroutine
        }


        if(HandItemManager.Instance.waterBulletStart)
        {
            if(maxCharge) bulletStart = HandItemManager.Instance.waterBulletCloseStart;
            else bulletStart = HandItemManager.Instance.waterBulletStart;
        } 
        else bulletStart = HandItemManager.Instance.bulletStart;

        if(bulletCount > 1) PlayerMovement.restrictMovementTokens++;

        PlayerInteraction.Instance.waterHeld--;
        GameObject newBullet;
        Vector3 dir;
        float extraForce = 15;

        ParticleSystem[] particles = HandItemManager.Instance.waterGun.GetComponentsInChildren<ParticleSystem>();

        for (int i = 0; i < bulletCount; i++)
        {
            //Debug.Log(bulletCount);
            HandItemManager.Instance.toolSource.PlayOneShot(shoot);
            /*if(bulletCount == 1)*/ newBullet = ProjectilePoolManager.Instance.GrabLargeWater();
            //else GameObject newBullet = ProjectilePoolManager.Instance.GrabSmallWater();
            newBullet.transform.position = bulletStart.position;
            newBullet.transform.rotation = bulletStart.rotation;
            if(highlights[i] != null && highlights[i].activeSelf)
            {
                newBullet.GetComponent<WaterProjectileScript>().homing = true;
                newBullet.GetComponent<WaterProjectileScript>().target = highlights[i].transform.position;
                dir = bulletStart.forward  ;//+ new Vector3(Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread));
                newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * (50 + (extraForce)));
                newBullet.GetComponent<Rigidbody>().AddForce(dir * (30 + extraForce));
                extraForce += 45;
            } 
            else
            {
                dir = bulletStart.forward;
                if(maxCharge)
                {
                    newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 10);
                    newBullet.GetComponent<Rigidbody>().AddForce(dir * (speed * 2));
                }
                else
                {
                    newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 30);
                    newBullet.GetComponent<Rigidbody>().AddForce(dir * speed);
                }
            } 
            for(int p = 0; p < particles.Length; p++) particles[p].Play();
            yield return new WaitForSeconds(0.25f);
        }
        foreach(GameObject light in highlights)
        {
            light.SetActive(false);
        }
        toolAnim.SetBool("EarlyFire", false);
        yield return new WaitForSeconds(0.1f);
        usingPrimary = false;
        shootingGunCoroutine = null;
        if(bulletCount > 1) PlayerMovement.restrictMovementTokens--;
    }

    public Direction GetDirection()
    {
        Direction newDirection;

        float rotation = player.eulerAngles.y; 

        Debug.Log(rotation);

        if(rotation <= 45 || rotation >= 315) newDirection = Direction.South;

        else if(rotation >= 45 && rotation <= 135) newDirection = Direction.East;

        else if(rotation >= 135 && rotation <= 225) newDirection = Direction.North;

        else newDirection = Direction.West; 

        return newDirection;
    }

  
}
