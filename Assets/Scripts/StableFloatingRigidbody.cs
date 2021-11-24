using System;
using Gravity;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StableFloatingRigidbody  : MonoBehaviour
{
	[SerializeField] private bool floatToSleep = false;

	[SerializeField] private bool safeFloating = false;

	[SerializeField] private float submergenceOffset = 0.5f;

	[SerializeField, Min(0.1f)] private float submergenceRange = 1f;

	[SerializeField, Min(0f)] private float buoyancy = 1f;
	
	[SerializeField, Range(0f, 10f)]
	private float waterDrag = 2f;

	[SerializeField] private Vector3[] buoyancyOffsets = default;

	[SerializeField] private LayerMask waterMask = 0;
	
	private Rigidbody body;

	private float floatDelay;

	private float[] submergence;

	private Vector3 gravity;

	private void Awake()
	{
		body = GetComponent<Rigidbody>();
		body.useGravity = false;
		submergence = new float[buoyancyOffsets.Length];
	}

	private void FixedUpdate()
	{
		if (CheckSleep()) return;

		ApplyForces();
	}

	private bool CheckSleep()
	{
		if (!floatToSleep) return false;
		
		if (body.IsSleeping())
		{
			floatDelay = 0f;
			return true;
		}

		if (IsBodyResting())
		{
			floatDelay += Time.deltaTime;

			if (floatDelay >= 1f)
				return true;
		}
		else
			floatDelay = 0f;

		return false;
	}

	private void ApplyForces()
	{
		gravity = CustomGravity.GetGravity(body.position);
		ApplyForceAtBuoyancyOffsets();

		body.AddForce(gravity, ForceMode.Acceleration);
	}

	private void ApplyForceAtBuoyancyOffsets()
	{
		float dragFactor = waterDrag * Time.deltaTime / buoyancyOffsets.Length;
		float buoyancyFactor = -buoyancy / buoyancyOffsets.Length;

		for (var i = 0; i < buoyancyOffsets.Length; i++)
		{
			if (submergence[i] > 0f)
			{
				float drag = Mathf.Max(0f, 1f - dragFactor * submergence[i]);
				body.velocity *= drag;
				body.angularVelocity *= drag;

				Vector3 worldPosition = transform.TransformPoint(buoyancyOffsets[i]);
				Vector3 force = gravity * (buoyancyFactor * submergence[i]);
				
				body.AddForceAtPosition(force, worldPosition, ForceMode.Acceleration);

				submergence[i] = 0f;
			}
		}
	}

	private bool IsBodyResting()
	{
		return body.velocity.sqrMagnitude < 0.0001f;
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((waterMask & (1 << other.gameObject.layer)) != 0) EvaluateSubmergence();
	}

	private void OnTriggerStay(Collider other)
	{
		if (!body.IsSleeping() && (waterMask & (1 << other.gameObject.layer)) != 0) EvaluateSubmergence();
	}

	private void EvaluateSubmergence()
	{
		Vector3 down = gravity.normalized;
		Vector3 offset = down * -submergenceOffset;

		for (var i = 0; i < buoyancyOffsets.Length; i++)
		{
			Vector3 buoyancyPosition = offset + transform.TransformPoint(buoyancyOffsets[i]);

			if (Physics.Raycast(buoyancyPosition, down, out RaycastHit hit,
				submergenceRange + 1f, waterMask, QueryTriggerInteraction.Collide))
			{
				submergence[i] = 1f - hit.distance / submergenceRange;
			}
			else if (!safeFloating || Physics.CheckSphere(buoyancyPosition, 0.01f, waterMask, 
				QueryTriggerInteraction.Collide))
			{
				submergence[i] = 1f;
			}
		}
	}
}
