using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DetectionZone : MonoBehaviour
{
    [SerializeField] private UnityEvent onFirstEnter, onLastExit;
    
    private readonly List<Collider> colliders = new List<Collider>();

    private void Awake()
    {
        enabled = false;
    }

    private void FixedUpdate()
    {
        for (var i = 0; i < colliders.Count; i++)
        {
            Collider collider = colliders[i];

            if (!collider || !collider.gameObject.activeInHierarchy)
            {
                colliders.RemoveAt(i--);
                
                if (colliders.Count == 0)
                {
                    onLastExit.Invoke();
                    enabled = false;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        colliders.Add(other);
        
        if (colliders.Count == 1)
        {
            onFirstEnter.Invoke();
            enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        colliders.Remove(other);
        
        if (colliders.Count == 0)
        {
            onLastExit.Invoke();
            enabled = false;
        }
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        if (enabled && gameObject.activeInHierarchy)
            return;
#endif        

        if (colliders.Count > 0)
        {
            colliders.Clear();
            onLastExit.Invoke();
        }
    }
}
