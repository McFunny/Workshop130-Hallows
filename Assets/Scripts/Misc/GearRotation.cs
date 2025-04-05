using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GearRotation : MonoBehaviour
{
    public float rotationSpeed = 30f;
    public int direction = 1; // use -1 to reverse rotation fo gears
    public Axis rotationAxis = Axis.Z;

    public enum Axis { X, Y, Z }

    public Vector3 GetAxisVector()
    {
        switch (rotationAxis)
        {
            case Axis.X: return Vector3.right;
            case Axis.Y: return Vector3.up;
            case Axis.Z: return Vector3.forward;
            default: return Vector3.forward;
        }

    }
}
