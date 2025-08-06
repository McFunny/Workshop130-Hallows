using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CritterPen : StructureBehaviorScript
{
    //Should probably have text to show which creatures reside here

    public PenType type;
    //public int currentOccupents = 0;
    public List<CreatureBehaviorScript> housedCritters;
    public int maxOccupency = 2;
}

public enum PenType
{
    Pen,
    Coop,
    Hive
}
