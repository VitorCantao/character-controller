using UnityEngine;

namespace Gravity
{
	[RequireComponent(typeof(Rigidbody))]
	public class CustomGravityRigidbody  : MonoBehaviour
	{
		[SerializeField] private bool floatToSleep = false;

		[SerializeField] private float submergenceOffset = 0.5f;

		[SerializeField, Min(0.1f)] private float submergenceRange = 1f;

		[SerializeField, Min(0f)] private float buoyancy = 1f;
	
		[SerializeField, Range(0f, 10f)]
		private float waterDrag = 2f;

		[SerializeField] private Vector3 buoyancyOffset = Vector3.zero;

		[SerializeField] private LayerMask waterMask = 0;
		
	
		private Rigidbody body;

		private float floatDelay;

		private float submergence;

		private Vector3 gravity;
		

		private void Awake()
		{
			body = GetComponent<Rigidbody>();
			body.useGravity = false;
		}

		private void FixedUpdate()
		{
			if (CheckSleep()) return;

			ApplyGravity();
		}

		private void ApplyGravity()
		{
			gravity = CustomGravity.GetGravity(body.position);

			if (submergence > 0f)
			{
				float drag = Mathf.Max(0f, 1f - waterDrag * submergence * Time.deltaTime);
				body.velocity *= drag;
				body.angularVelocity *= drag;

				Vector3 worldPosition = transform.TransformPoint(buoyancyOffset);
				Vector3 force = gravity * -(buoyancy * submergence);
				
				body.AddForceAtPosition(force, worldPosition, ForceMode.Acceleration);

				submergence = 0f;
			}

			body.AddForce(gravity, ForceMode.Acceleration);
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
			Vector3 upAxis = -gravity.normalized;

			if (Physics.Raycast(body.position + upAxis * submergenceOffset, -upAxis, out RaycastHit hit,
				submergenceRange + 1f, waterMask, QueryTriggerInteraction.Collide))
			{
				submergence = 1f - hit.distance / submergenceRange;
			}
			else
			{
				submergence = 1f;
			}
		}
	}
}
