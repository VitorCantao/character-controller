using System;
using Controller;
using Controller.States;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class DebugText : MonoBehaviour
{
    [SerializeField] private SphereController controller;
    private TextMeshProUGUI textMesh;

    private Rigidbody body;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        body = controller.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        ControllerState state = controller.CurrentState;

        double roundedMagnitude = Math.Round(body.velocity.magnitude, 2);
        textMesh.SetText(
            $@"Velocity: {controller.Velocity} ({roundedMagnitude})
State: {state}"
        );
    }
}
