using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Flintlock")]
public class FlintlockBehavior : ToolBehavior
{
    public List<InventoryItemData> acceptableAmmo;

    public List<PelletValues> pelletValues;

    //PelletValues currentPellet;

    public AudioClip shoot, hit_Dirt, hit_Creature, hit_Structure, headShot;
    int bulletCount = 1;
    int shrapnelCount = 8;

    Transform bulletStart;

    float speed = 400;
    float bulletSpread = 0.0001f;
    float shrapnelSpread = 0.075f;

    float currentBulletDamage = 20;
    float currentBulletStructureDamage = 2;
    float headShotMult = 3f;

    Vector3 origin;
    Vector3 direction;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || PlayerInteraction.Instance.toolCooldown || TownGate.Instance.location == PlayerLocation.InTown) return;
        if (!player) player = _player;

        var inventory = PlayerInventoryHolder.Instance.PrimaryInventorySystem;
        if (inventory.ContainsItems(acceptableAmmo, out List<InventorySlot> invSlot))
        {
            foreach (PelletValues p in pelletValues)
            {
                if(invSlot[0].ItemData == p.pelletItem)
                {
                    currentBulletDamage = p.damage;
                    inventory.RemoveItemsFromInventory(p.pelletItem, 1);
                    break;
                }
            }
        }
        else 
        {
            Debug.Log("No Bullet In Primary");
            inventory = PlayerInventoryHolder.Instance.secondaryInventorySystem;
            if (inventory.ContainsItems(acceptableAmmo, out List<InventorySlot> invSlot2))
            {
                foreach (PelletValues p in pelletValues)
                {
                    if(invSlot[0].ItemData == p.pelletItem)
                    {
                        currentBulletDamage = p.damage;
                        inventory.RemoveItemsFromInventory(p.pelletItem, 1);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("No Bullet In Secondary");
                return;
            }
        }
        
        tool = _tool;
        usingPrimary = true;
        //Shoot
        HandItemManager.Instance.pistolParticles.Play();

        HandItemManager.Instance.PlayPrimaryAnimation();
        HandItemManager.Instance.toolSource.PlayOneShot(shoot);
        float cooldown = 0.25f;
        PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, cooldown));
    }

    public override void SecondaryUse(Transform _player, ToolType _tool)
    {
        //

    }

    public override void ItemUsed()
    {
        if (usingPrimary)
        {
            HandItemManager.Instance.StartCoroutine(ShootGun());
        }
        if (usingSecondary)
        {
            usingSecondary = false;
        }

    }

    public IEnumerator ShootGun()
    {
        PlayerInteraction.Instance.ShakeScreen(0.7f);
        if(!bulletStart)
        {
            bulletStart = HandItemManager.Instance.bulletStart;
        }
        ShootBullets();
        //ShootShrapnel();
        yield return new WaitForSeconds(0.65f);
        usingPrimary = false;
    }

    void ShootBullets()
    {
        ///hitscan bullets
        Ray camRay = PlayerInteraction.Instance.mainCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        origin = camRay.origin;
        direction = camRay.direction;
        RaycastHit hit;

        if (Physics.Raycast(origin, direction, out hit, 200, mask))
        {
            RaycastBulletHit(hit.collider.gameObject, hit.point);
        }
        float xRecoil = Random.Range(-50f, 50f);
        if(xRecoil < 0 && xRecoil > -25) xRecoil = -25;
        if(xRecoil > 0 && xRecoil < 25) xRecoil = 25;

        float yRecoil = Random.Range(-50f, 50f);
        if(yRecoil < 0 && yRecoil > -25) yRecoil = -25;
        if(yRecoil > 0 && yRecoil < 25) yRecoil = 25;

        PlayerCam.Instance.AddCameraRecoil(xRecoil, yRecoil);

        /*
        for (int i = 0; i < bulletCount; i++)
        {
            GameObject newBullet = ProjectilePoolManager.Instance.GrabFlintBulletRock();
            newBullet.transform.position = bulletStart.position;
            newBullet.transform.rotation = Quaternion.identity;
            Vector3 dir;
            if(i == 0) dir = bulletStart.forward + new Vector3(Random.Range(-0.02f,+0.02f), Random.Range(-0.02f,0.02f), Random.Range(-0.02f,0.02f));
            else dir = bulletStart.forward + new Vector3(Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread), Random.Range(-bulletSpread,bulletSpread));
            newBullet.GetComponent<Rigidbody>().AddForce(dir * speed);
 
        }
        */
    }

    void ShootShrapnel()
    {
        for (int i = 0; i < shrapnelCount; i++)
        {
            GameObject newBullet = ProjectilePoolManager.Instance.GrabFlintBulletShrapnel();
            newBullet.transform.position = bulletStart.position;
            newBullet.transform.rotation = Quaternion.identity;
            Vector3 dir = bulletStart.forward + new Vector3(Random.Range(-shrapnelSpread,shrapnelSpread), Random.Range(-shrapnelSpread,shrapnelSpread), Random.Range(-shrapnelSpread,shrapnelSpread));
            newBullet.GetComponent<Rigidbody>().AddForce(dir * speed);
 
        }
    }

    void RaycastBulletHit(GameObject other, Vector3 hitPos)
    {
        if(other.gameObject.layer == 18)
        {
            var armor = other.GetComponent<CreatureArmor>();
            if(armor)
            {
                armor.TakeDamage(2);
                HandItemManager.Instance.toolSource.PlayOneShot(hit_Structure);
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;

                //GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
                //if(particles) particles.transform.position = transform.position;
                return;
            }
        }

        if(other.gameObject.layer == 6)
        {
            //break
            var structure = other.GetComponent<StructureBehaviorScript>();
            if (structure != null)
            {
                if(currentBulletStructureDamage > 0)
                {
                    structure.TakeDamage(currentBulletStructureDamage);
                    HandItemManager.Instance.toolSource.PlayOneShot(hit_Structure);
                    ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
                    ParticlePoolManager.Instance.MoveAndPlayParticle(hitPos, ParticlePoolManager.Instance.dirtParticle);

                    //GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
                    //if(particles) particles.transform.position = transform.position;
                    return;
                }
                
            }    
        }

        if (other.gameObject.layer == 17)
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                float bulletSpeed = 200;
                float forceMultiplier = 0.1f;
                rb.AddForce(direction * bulletSpeed * forceMultiplier, ForceMode.Impulse);
            }
        }

        if (other.gameObject.layer == 9)
        {
            var creature = other.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                if(other.tag == "Head")
                {
                    creature.TakeDamage(headShotMult * currentBulletDamage);
                    ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
                    ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
                    HandItemManager.Instance.toolSource.PlayOneShot(headShot);
                }
                else creature.TakeDamage(currentBulletDamage);
                //playsound
                HandItemManager.Instance.toolSource.PlayOneShot(hit_Creature);

                //GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
                //if(particles) particles.transform.position = transform.position;
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
                creature.PlayHitParticle(hitPos);
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7 || other.gameObject.layer == 19)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(hit_Dirt);
            ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
            ParticlePoolManager.Instance.MoveAndPlayParticle(hitPos, ParticlePoolManager.Instance.dirtParticle);

            //GameObject particles = ParticlePoolManager.Instance.GrabDestructionParticle(particleType);
            //if(particles) particles.transform.position = transform.position;
            return;
        }

        var bug = other.GetComponent<BugBehaviorScript>();
        if(bug) bug.Struck();

    }
}

[System.Serializable]
public class PelletValues
{
    public InventoryItemData pelletItem;
    public float damage = 20;
}
