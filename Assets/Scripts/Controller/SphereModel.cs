using Controller.States.SuperStates;
using UnityEngine;

namespace Controller
{
	/// <summary>
	/// Animates the sphere model based on the controller velocity and state.
	/// </summary>
	[RequireComponent(typeof(SphereController))]
	public class SphereModel : MonoBehaviour
	{
		[SerializeField] private Transform model;

		[SerializeField, Min(0f)] private float alignSpeed = 180f;
		[SerializeField, Min(0f)] private float airRotation = 0.5f;
		[SerializeField, Min(0f)] private float swimRotation = 2f;
		[SerializeField, Min(0.1f)] private float radius = 0.5f;

		private SphereController controller;

		private Vector3 rotationPlaneNormal;
		private Vector3 movement;

		private void Awake()
		{
			controller = GetComponent<SphereController>();
		}

		private void Update()
		{
			UpdateModelRotation();
		}

		private void UpdateModelRotation()
		{
			SetRotationPlaneNormal();
			ComputeMovement();

			float distance = movement.magnitude;
			bool isDistanceInsignificant = distance < 0.001f;

			Quaternion rotation = model.localRotation;

			bool hasAngularVelocity = controller.AngularVelocity != Vector3.zero;
			if (hasAngularVelocity)
			{
				rotation = RotateWithAngularVelocity();

				if (isDistanceInsignificant)
				{
					model.localRotation = rotation;
					return;
				}
			}
			else if (isDistanceInsignificant)
				return;

			model.localRotation = GetMovementRotation(rotation);
		}

		private Quaternion GetMovementRotation(Quaternion rotation)
		{
			float distance = movement.magnitude;
			float rotationFactor = GetRotationFactor();
			float angle = rotationFactor * movement.magnitude * (180 / Mathf.PI) / radius;

			Vector3 rotationAxis = Vector3.Cross(rotationPlaneNormal, movement).normalized;

			Quaternion newRotation = Quaternion.Euler(rotationAxis * angle) * rotation;

			if (alignSpeed > 0f)
				newRotation = AlignRotation(rotationAxis, newRotation, distance);

			return newRotation;
		}

		private Quaternion RotateWithAngularVelocity()
		{
			return Quaternion.Euler(controller.AngularVelocity
			                        * (Mathf.Rad2Deg * Time.deltaTime)) * model.localRotation;
		}

		private void ComputeMovement()
		{
			movement = Vector3.zero;
			
			movement = controller.RelativeMovement 
			       - rotationPlaneNormal 
			       * Vector3.Dot(movement, rotationPlaneNormal);
		}

		private void SetRotationPlaneNormal()
		{
			if (controller.CurrentState is ClimbingState state)
			{
				rotationPlaneNormal = state.ClimbNormal;
			}
			else if (controller.ContactNormal == Vector3.zero)
			{
				rotationPlaneNormal = controller.UpAxis;
			}
			else
			{
				rotationPlaneNormal = !controller.OnGround && controller.OnSteep
					? controller.LastSteepNormal
					: controller.LastContactNormal;
			}
		}

		private float GetRotationFactor()
		{
			var rotationFactor = 1f;

			if (controller.CurrentState is SwimmingState)
			{
				rotationFactor = swimRotation;
			}
			else if (!controller.OnGround && !controller.OnSteep)
			{
				rotationFactor = airRotation;
			}

			return rotationFactor;
		}

		private Quaternion AlignRotation(Vector3 rotationAxis, Quaternion rotation, float traveledDistance)
		{
			Vector3 ballAxis = model.up;
			
			float dot = Mathf.Clamp(Vector3.Dot(ballAxis, rotationAxis), -1f, 1f);
			float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
			float maxAngle = alignSpeed * traveledDistance;

			Quaternion newAlignment = Quaternion.FromToRotation(ballAxis, rotationAxis) * rotation;

			return angle <= maxAngle 
				? newAlignment 
				: Quaternion.SlerpUnclamped(rotation, newAlignment, maxAngle / angle);
		}
	}
}