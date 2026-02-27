using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tool Behavior", menuName = "Tool Behavior/Flintlock")]
public class FlintlockBehavior : ToolBehavior
{
    public List<InventoryItemData> acceptableAmmo;

    public List<PelletValues> pelletValues;

    //PelletValues currentPellet;
    public AudioClip[] shootSFX;

    public AudioClip shoot, hit_Dirt, hit_Creature, hit_Structure, headShot, jamClick;
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

    Coroutine lagRoutine;


    public override void PrimaryUse(Transform _player, ToolType _tool)
    {
        if (usingPrimary || usingSecondary || TownGate.Instance.location == PlayerLocation.InTown) return;
        if (!player) player = _player;

        if(EndingManager.Instance.endingPlaying)
        {
            HandItemManager.Instance.toolSource.PlayOneShot(jamClick);
            return;
        }

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
                HandItemManager.Instance.toolSource.PlayOneShot(jamClick);
                return;
            }
        }
        
        tool = _tool;
        if(toolAnim == null) toolAnim = HandItemManager.Instance.AccessCurrentAnimator();
        usingPrimary = true;
        //Shoot
        HandItemManager.Instance.pistolParticles.Play();

        //HandItemManager.Instance.PlayPrimaryAnimation();
        toolAnim.Play("pistolshoot", -1, 0f);
        HandItemManager.Instance.toolSource.PlayOneShot(shootSFX[Random.Range(0, shootSFX.Length)]);
        PlayerInteraction.Instance.ToolUseToggle(true);
        ItemUsed();

        //float cooldown = 0.25f;
        //PlayerInteraction.Instance.StartCoroutine(PlayerInteraction.Instance.ToolUse(this, 0.1f, cooldown));
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
            return;
        }

        usingPrimary = false;
        PlayerInteraction.Instance.ToolUseToggle(false);

        if (usingSecondary)
        {
            usingSecondary = false;
        }

    }

    public IEnumerator ShootGun()
    {
        if(lagRoutine != null) HandItemManager.Instance.StopCoroutine(lagRoutine);
        PlayerInteraction.Instance.ShakeScreen(0.7f);
        if(!bulletStart)
        {
            bulletStart = HandItemManager.Instance.bulletStart;
        }
        ShootBullets();
        //ShootShrapnel();
        yield return new WaitForSeconds(0.25f);
        lagRoutine = HandItemManager.Instance.StartCoroutine(ExtraLag());
        yield return new WaitForSeconds(0.05f);
        usingPrimary = false;
    }

    public IEnumerator ExtraLag()
    {
        yield return new WaitForSeconds(0.7f);
        PlayerInteraction.Instance.ToolUseToggle(false);
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
            //ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hit.point;
        }
        float xRecoil = Random.Range(-50f, 50f);
        if(xRecoil < 0 && xRecoil > -25) xRecoil = -25;
        if(xRecoil > 0 && xRecoil < 25) xRecoil = 25;

        float yRecoil = Random.Range(20f, 75f);
        //if(yRecoil < 0 && yRecoil > -40) yRecoil = -40;
        //if(yRecoil > 0 && yRecoil < 40) yRecoil = 40;

        PlayerCam.Instance.AddCameraRecoil(xRecoil, yRecoil);
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
                //HandItemManager.Instance.toolSource.PlayOneShot(hit_Structure);
                AudioPoolManager.Instance.PlayClipAtPosition(hit_Structure, hitPos, HandItemManager.Instance.toolSource.volume, 40);
                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
                ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = hitPos;

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
                    //HandItemManager.Instance.toolSource.PlayOneShot(hit_Structure);
                    AudioPoolManager.Instance.PlayClipAtPosition(hit_Structure, hitPos, HandItemManager.Instance.toolSource.volume, 40);
                    
                    ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
                    ParticlePoolManager.Instance.MoveAndPlayParticle(hitPos, ParticlePoolManager.Instance.dirtParticle);
                    ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = hitPos;

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
                ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = hitPos;
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
                    ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hitPos;
                    HandItemManager.Instance.toolSource.PlayOneShot(headShot);
                    AudioPoolManager.Instance.PlayClipAtPosition(headShot, hitPos, 0.8f, 100);
                }
                else creature.TakeDamage(currentBulletDamage);
                //playsound
                //HandItemManager.Instance.toolSource.PlayOneShot(hit_Creature);
                if(creature.corpseType == CorpseParticleType.Metal && creature.corpseType == CorpseParticleType.Stone) 
                    AudioPoolManager.Instance.PlayClipAtPosition(hit_Structure, hitPos, HandItemManager.Instance.toolSource.volume, 40);
                else AudioPoolManager.Instance.PlayClipAtPosition(hit_Creature, hitPos, HandItemManager.Instance.toolSource.volume, 40);


                ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
                ParticlePoolManager.Instance.GrabOrangeHitParticle().transform.position = hitPos;
                creature.PlayHitParticle(hitPos);
                return;
            }
        }

        if(other.gameObject.layer == 0 || other.gameObject.layer == 7 || other.gameObject.layer == 19)
        {
            //HandItemManager.Instance.toolSource.PlayOneShot(hit_Dirt);
            AudioPoolManager.Instance.PlayClipAtPosition(hit_Dirt, hitPos, HandItemManager.Instance.toolSource.volume, 40);
            ParticlePoolManager.Instance.GrabImpactParticle().transform.position = hitPos;
            ParticlePoolManager.Instance.GrabWhiteHitParticle().transform.position = hitPos;
            ParticlePoolManager.Instance.MoveAndPlayParticle(hitPos, ParticlePoolManager.Instance.dirtParticle);
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
