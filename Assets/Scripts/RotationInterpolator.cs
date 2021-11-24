using System;
using UnityEngine;

public class RotationInterpolator : MonoBehaviour
{
    private Rigidbody body;

    [SerializeField] private bool x, y, z;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }

    public void Interpolate(float t)
    {
        Quaternion rotation = Quaternion.Euler(
            x ? 360 * t : 0, 
            y ? 360 * t : 0, 
            z ? 360 * t : 0);

        body.MoveRotation(rotation);
    }
}
