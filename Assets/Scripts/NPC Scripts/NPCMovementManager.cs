using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Destination
{
    Tavern,
    ShopStall1,
    ShopStall2,
    ShopStall3,
    ShopStall4,
}

public enum Action
{
   Working,
   Walking,
   Idle
}
public class NPCMovementManager : MonoBehaviour
{
    [SerializeField] private Transform tavern;
    [SerializeField] private Transform shopStall1;
    [SerializeField] private Transform shopStall2;
    [SerializeField] private Transform shopStall3;
    [SerializeField] private Transform shopStall4;
    

    public Transform GetDestination(Destination destination)
    {
        switch (destination)
        {
            case Destination.Tavern:
                return tavern;
            case Destination.ShopStall1:
                return shopStall1;
            case Destination.ShopStall2:
                return shopStall2;
            case Destination.ShopStall3:
                return shopStall3;
            case Destination.ShopStall4:
                return shopStall4;

            default:
                return tavern;
        }
    }
}
