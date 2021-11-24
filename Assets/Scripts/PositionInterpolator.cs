using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionInterpolator : MonoBehaviour
{
    [SerializeField] private Rigidbody body;

    [SerializeField] private Vector3 from, to;

    [SerializeField] private Transform relativeTo;

    public void Interpolate(float t)
    {
        Vector3 position;

        if (relativeTo)
            position = Vector3.LerpUnclamped(relativeTo.TransformPoint(from), relativeTo.TransformPoint(to), t);
        else
            position = Vector3.LerpUnclamped(from, to, t);
        
        body.MovePosition(position);
    }
}
