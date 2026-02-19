using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargeTurret : StructureBehaviorScript
{
    public Transform turretHead, bulletOrigin;

    public GameObject detector, light;

    public Animator anim;

    bool lockOntoTarget = false;
    bool shotCooldown;
    bool returningToCenter;
    bool activated;
    bool overHeating;
    bool shootingTarget;
    float projectileSpeed = 300;

    public List<CreatureObject> targettableCreatures;

    Transform currentTarget;

    float rotateSpeed = 50f;

    float RotAngleY;
    private Quaternion startRotation;

    //public Renderer skullRenderer;
    //public Material defaultMat, heatedMat;

    void Start()
    {
        detector.SetActive(false);
        base.Start();
        startRotation = turretHead.rotation;
        RotAngleY = turretHead.eulerAngles.y;

        StartCoroutine(PowerCheck());
    }

    void Update()
    {
        base.Update();

        if(currentTarget && lockOntoTarget)
        {
            RotateToTarget();
        }

        if(!currentTarget)
        {
            if(returningToCenter)
            {
                turretHead.rotation = Quaternion.RotateTowards(turretHead.rotation, startRotation, rotateSpeed * Time.deltaTime);
                //if(RotAngleY > turretHead.eulerAngles.y) turretHead.rotation = Quaternion.Euler(0,turretHead.eulerAngles.y + 0.4f,0);
                //else turretHead.rotation = Quaternion.Euler(0,turretHead.eulerAngles.y - 0.4f,0);
                if(turretHead.eulerAngles.y < RotAngleY + 0.01f && turretHead.eulerAngles.y > RotAngleY - 0.01f) returningToCenter = false;
            }
        }
    }

    void RotateToTarget()
    {
        Vector3 targetPosition;
        /*if(currentTarget.corpseParticleTransform) targetPosition = currentTarget.corpseParticleTransform.position;
        else*/ targetPosition = currentTarget.transform.position;

        Vector3 direction = targetPosition - turretHead.position;
        direction.y = 0;
        Quaternion toRotation = Quaternion.LookRotation(direction);

        turretHead.rotation = Quaternion.Slerp(turretHead.rotation, toRotation, rotateSpeed * Time.deltaTime);
    }

    IEnumerator Shoot()
    {
        Vector3 targetPosition;
        targetPosition = currentTarget.transform.position;

        lockOntoTarget = true;
        shootingTarget = true;

        yield return new WaitForSeconds(0.7f);
        //Open mouth
        //lockOntoTarget = false;
        audioHandler.PlaySound(audioHandler.miscSounds1[2]);
        anim.SetBool("OpenSkull", true);
        yield return new WaitForSeconds(0.3f);
        lockOntoTarget = false;


        //fire
        audioHandler.PlaySound(audioHandler.miscSounds1[3]);
        ParticlePoolManager.Instance.GrabElecZapParticle().transform.position = bulletOrigin.position; 

        GameObject newBullet = ProjectilePoolManager.Instance.GrabEnergyBullet();
        //Vector3 dir = (targetPosition - turretHead.position).normalized;

        newBullet.transform.position = bulletOrigin.position;
        newBullet.transform.rotation = turretHead.rotation;

        newBullet.GetComponent<Rigidbody>().AddForce(turretHead.forward * projectileSpeed);
        //print("PEW");

        ParticlePoolManager.Instance.MoveAndPlayVFX(bulletOrigin.position, ParticlePoolManager.Instance.hitEffect);
        ParticlePoolManager.Instance.GrabCloudParticle().transform.position = bulletOrigin.position;

        yield return new WaitForSeconds(2f);
        returningToCenter = true;
        currentTarget = null;

        //Close Mouth
        audioHandler.PlaySound(audioHandler.miscSounds1[2]);
        anim.SetBool("OpenSkull", false);
        yield return new WaitForSeconds(2f);
        shootingTarget = false;
        detector.SetActive(true);
    }

    public override void ToolInteraction(ToolType type, out bool success)
    {
        success = false;
        if(type == ToolType.Shovel)
        {
            //StartCoroutine(DugUpForItem());
            success = true;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if(currentTarget || !activated) return;
        CreatureBehaviorScript creature = other.gameObject.GetComponentInParent<CreatureBehaviorScript>();

        Vector3 direction = other.transform.position - turretHead.position;
        direction.y = 0;

        RaycastHit hit;
        if (Physics.Raycast(turretHead.position, direction, out hit, 10, 1 << 6, QueryTriggerInteraction.Ignore))
        {
            return;
            //structure in the way
        }

        if(other.gameObject.layer == 10 || (creature && targettableCreatures.Contains(creature.creatureData))) 
        {
            detector.SetActive(false);
            currentTarget = other.gameObject.transform;
            StartCoroutine(Shoot());
            audioHandler.PlaySound(audioHandler.miscSounds1[4]);
        }
    }

    void TogglePower(bool newPowerState)
    {
        if(activated == newPowerState || shootingTarget) return;
        activated = newPowerState;

        if(newPowerState)
        {
            detector.SetActive(true);
            light.SetActive(true);
            audioHandler.PlaySound(audioHandler.miscSounds1[0]);
        }
        else
        {
            detector.SetActive(false);
            light.SetActive(false);
            audioHandler.PlaySound(audioHandler.miscSounds1[1]);
        }
    }

    IEnumerator PowerCheck()
    {
        while(health > 0)
        {
            yield return new WaitForSeconds(2);


            if(activated)
            {
                if(nearbyFires.Count == 0) TogglePower(false);
            }
            else
            {
                if(nearbyFires.Count > 0) TogglePower(true);
            }
        }
    }
}
