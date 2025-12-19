using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrockPot : FurnitureBehaviorScript
{
    public List<SpriteRenderer> itemSockets = new List<SpriteRenderer>();

    public ParticleSystem boilParticles, oilSplashParticles, finishPoof;

    public GameObject oilObject, fireObject;

    bool hasOil, isLit;
}
