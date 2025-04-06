using System.Collections.Generic;
using UnityEngine;

public class GearManager : MonoBehaviour
{
    [Tooltip("List of gears to be rotated by this manager")]
    public List<GearRotation> gears = new List<GearRotation>();

    void Update()
    {
        float rotationDelta = Time.deltaTime;
        for (int i = 0; i < gears.Count; i++)
        {
            var gear = gears[i];
            Vector3 axis = gear.GetAxisVector();
            float angle = gear.rotationSpeed * gear.direction * rotationDelta;
            gear.transform.localRotation *= Quaternion.AngleAxis(angle, axis);
        }
    }
}