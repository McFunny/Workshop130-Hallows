using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thumper : StructureBehaviorScript
{
    public Animator anim;
    public Renderer panel1, panel2, panel3;

    public ParticleSystem smallPulse, mediumPulse, largePulse;

    public float smallRange, mediumRange, largeRange;

    public int charge = 0;
    int maxCharge = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
