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
    Blacksmith,
    BellTower,
    Windmill,
    FanaticHouse,
    LumberjackHouse,
    BotanistHouse,
    BarkeepHouse,
    ApothecaryHouse,
    GravediggerHouse,
    Fountain,
    Spring
}

public enum Action
{
    Working,
    Walking,
    Idle
}

[System.Serializable]
public class Sublocation
{
    public string name; //name of sublocation
    public Transform transform; 
    public bool isForWorkers; // is this spot for workers 
    public bool isOccupied; 
    public NPCMovement occupant; //The current NPC occupying the spot
}

[System.Serializable]
public class DestinationData
{
    public Destination destination; // general destination
    public Transform mainTransform; // main location for fallback
    public List<Sublocation> sublocations = new List<Sublocation>(); // all of the sub locations per location
}

public class NPCMovementManager : MonoBehaviour
{
    [SerializeField] private List<DestinationData> destinations = new List<DestinationData>();

    
    public Transform GetDestination(Destination destination)
    {
        DestinationData destinationData = destinations.Find(d => d.destination == destination);
        return destinationData?.mainTransform;
    }

    // Get a random sublocation based on availability and NPC type
    public Sublocation GetRandomSublocation(Destination destination, bool isWorker)
    {
        DestinationData destinationData = destinations.Find(d => d.destination == destination);

        if (destinationData == null) return null;

        // Filter for available sublocations
        List<Sublocation> availableSublocations = destinationData.sublocations.FindAll(s =>
            s.isForWorkers == isWorker && !s.isOccupied);

        if (availableSublocations.Count == 0) return null;

        // Pick a random available sublocation
        return availableSublocations[Random.Range(0, availableSublocations.Count)];
    }
}
