using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PetRock : PetBehaviorScript, IInteractable
{
    public List<CreatureObject> targettableCreatures;

    public InventoryItemData rocks;

    public GameObject model;

    public Vector3 positionToClear;

    public LayerMask groundMask;

    void Start()
    {
        base.Start();
        agent.enabled = false;
        StartCoroutine(ChangePosition());
    }

    IEnumerator ChangePosition()
    {
        while(true)
        {
            yield return new WaitForSeconds(Random.Range(15f, 90));

            Vector3 dir = (PlayerInteraction.Instance.playerFeet.position - transform.position).normalized;
            float dot = Vector3.Dot(dir, PlayerInteraction.Instance.mainCam.transform.forward);


            if(dot >= 0f)
            {
                Vector3 newPos = Vector3.zero;
                int x = 0;
                while(x < 20 && newPos == Vector3.zero)
                {
                    x++;
                    //newPos = GetRandomPointAround(player.position, 25);

                    newPos = StructureManager.Instance.GetRandomClearTile();

                    dir = (player.position - newPos).normalized;
                    dot = Vector3.Dot(dir, PlayerInteraction.Instance.mainCam.transform.forward);

                    if(dot > 0f || (StructureManager.Instance.CheckTile(newPos) == Vector3.zero && StructureManager.Instance.ValidateGridType(newPos, GridType.Any))) newPos = Vector3.zero;
                }
                if(newPos != Vector3.zero)
                {
                    print(newPos);
                    if(positionToClear != Vector3.zero) StructureManager.Instance.ClearTile(positionToClear);

                    transform.position = newPos;
                    StructureManager.Instance.SetTile(newPos);
                    positionToClear = newPos;

                    transform.localEulerAngles = new Vector3(0, Random.Range(0,360), 0);
                    ParticlePoolManager.Instance.MoveAndPlayParticle(transform.position, ParticlePoolManager.Instance.dirtParticle);

                    HurtEnemies();
                }
            }
        }
    }

    void HurtEnemies()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, 5f, 1 << 9);
        foreach(Collider collider in hitEnemies)
        {
            var creature = collider.GetComponentInParent<CreatureBehaviorScript>();
            if (creature != null && creature.shovelVulnerable)
            {
                creature.TakeDamage(50);
                creature.PlayHitParticle(creature.transform.position);
            }
        }
    }


    /////IInteractable nonsense/////

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact(PlayerInteraction interactor, out bool interactSuccessful)
    {

        if(!alreadyPet)
        {
            alreadyPet = true;
            FriendPointsChange(20, true);
        }
        interactSuccessful = true;
    }

    public void InteractWithItem(PlayerInteraction interactor, out bool interactSuccessful, InventoryItemData item)
    {
        interactSuccessful = false;
        if((item.ID == 2 || item.ID == 270) && PlayerInteraction.Instance.waterHeld > 0)
        {
            PlayerInteraction.Instance.waterHeld--;
            interactSuccessful = true;
            //effectsHandler.MiscSound();
            StopCoroutine(DripEffects());
            StartCoroutine(DripEffects());
            thirst = maxThirst;
            return;
        }
        if(hunger < 100)
        {
            if(!foodDiet.Contains(item))
            {
                //thoughtBubbleScript.PlayEmotion(2);
                return;
            }
            HotbarDisplay.currentSlot.AssignedInventorySlot.RemoveFromStack(1);
            PlayerInventoryHolder.Instance.UpdateInventory();
            EatFood(item);
            interactSuccessful = true;
            return;
        }
        interactSuccessful = false;
    }
    
    public void EndInteraction(){}

    public void ToggleHighlight(bool enabled)
    {
        //showStats = enabled;
    }

    public void ReturnFocalPoint(out Transform point)
    {
        if(focalPoint) point = focalPoint;
        else point = transform;
    }
}
