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
    List<GameObject> highlights = new List<GameObject>();

    float speed = 240;
    float bulletSpread = 0.2f;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown) return;
        if (!player) player = _player;

        if(PlayerInteraction.Instance.waterHeld == 0) return;

        //code for refilling gun
        
        tool = _tool;
        usingPrimary = true;

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
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, 0.5f));
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || PlayerInteraction.Instance.stamina < 5) return;
        if (!player) player = _player;
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
                    PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.8f, 1.3f));
                    PlayerMovement.restrictMovementTokens++;
                    PlayerInteraction.Instance.StaminaChange(-2);
                    usingSecondary = true;
                }

            }
        }

    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            HandItemManager.Instance.StartCoroutine(ChargeTimer());
            HandItemManager.Instance.StartCoroutine(ShootGun());
        }
        if (usingSecondary)
        {
            usingSecondary = false;
            PlayerMovement.restrictMovementTokens--;
        }

    }

    public IEnumerator ChargeTimer()
    {
        bulletCount = 1;
        yield return new WaitForSeconds(0.7f);
        if(InputManager.isCharging)
        {
            HandItemManager.Instance.StartCoroutine(ShowRange());
        }
    }

    public IEnumerator ShowRange()
    {
        Vector3 currentPos = new Vector3(0,0,0);
        Vector3 newPos;

        Direction currentDirection = Direction.North;
        Direction newDirection;
        List<Vector3> targets = new List<Vector3>();
        while(InputManager.isCharging && usingPrimary)
        {
            newPos = StructureManager.Instance.GetTileCenter(player.position);
            newDirection = GetDirection();
            if((currentPos != newPos || currentDirection != newDirection) && newPos != new Vector3(0,0,0))
            {
                currentPos = newPos;
                currentDirection = newDirection;
                Debug.Log(currentDirection);
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
            else if(newPos == new Vector3(0,0,0)) foreach(GameObject light in highlights)
            {
                bulletCount = 1;
                light.SetActive(false);
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
        yield return new WaitForSeconds(0.01f);
        HandItemManager.Instance.StopCoroutine(ChargeTimer());
        if(!bulletStart)
        {
            bulletStart = HandItemManager.Instance.bulletStart;
        }
        PlayerInteraction.Instance.waterHeld--;
        GameObject newBullet;
        Vector3 dir;
        float extraForce = 0;

        for (int i = 0; i < bulletCount; i++)
        {
            Debug.Log(bulletCount);
            HandItemManager.Instance.toolSource.PlayOneShot(shoot);
            /*if(bulletCount == 1)*/ newBullet = ProjectilePoolManager.Instance.GrabLargeWater();
            //else GameObject newBullet = ProjectilePoolManager.Instance.GrabSmallWater();
            newBullet.transform.position = bulletStart.position;
            newBullet.transform.rotation = Quaternion.identity;
            if(highlights[i] != null && highlights[i].activeSelf)
            {
                newBullet.GetComponent<WaterProjectileScript>().homing = true;
                newBullet.GetComponent<WaterProjectileScript>().target = highlights[i].transform.position;
                dir = bulletStart.forward  + new Vector3(Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread));
                newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 50);
                newBullet.GetComponent<Rigidbody>().AddForce(dir * (10 + extraForce));
                extraForce += 20f;
            } 
            else
            {
                dir = bulletStart.forward;
                newBullet.GetComponent<Rigidbody>().AddForce(Vector3.up * 30);
                newBullet.GetComponent<Rigidbody>().AddForce(dir * speed);
            } 
            yield return new WaitForSeconds(0.2f);
        }
        foreach(GameObject light in highlights)
        {
            light.SetActive(false);
        }
        yield return new WaitForSeconds(0.2f);
        usingPrimary = false;
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
