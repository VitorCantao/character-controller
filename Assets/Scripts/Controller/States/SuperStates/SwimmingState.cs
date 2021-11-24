using UnityEngine;

namespace Controller.States.SuperStates
{
	public class SwimmingState : ControllerState
	{
		private float submergence = 0f;
		private Collider waterCollider;
		
		private const float MinimumSubmergence = 0.2f;

		public override bool CheckStateEnter()
		{
			waterCollider = Controller.GetColliderOfLayer(Settings.WaterMask);
			
			if (waterCollider == null) 
				return false;

			EvaluateSubmergence();
			return submergence > MinimumSubmergence;
		}

		public override void OnStateFixedUpdate()
		{
			if (!Controller.GetColliderOfLayer(Settings.WaterMask) || submergence <= MinimumSubmergence)
			{
				if (Controller.OnGround)
					Controller.SetState<GroundedState>();
				else
					Controller.SetState<InAirState>();
			}

			EvaluateSubmergence();
			HandleMovement();
		}
		
		private void HandleMovement()
		{
			Vector3 velocity = SwimVelocity();

			Controller.SetContactNormal(Controller.UpAxis);
			Controller.PreventSnapToGround();

			float waterDrag = 1 - Settings.WaterDrag * submergence * Time.fixedDeltaTime;
			Vector3 waterForce = Controller.Gravity * ((1f - Settings.Buoyancy * submergence) * Time.fixedDeltaTime);
			
			Controller.Velocity = (Controller.Velocity * waterDrag) + velocity + waterForce;
		}

		private Vector3 SwimVelocity()
		{
			float swimFactor = Mathf.Min(1, submergence / Settings.SwimThreshold);

			float acceleration = Mathf.LerpUnclamped(
				Controller.OnGround ? Settings.MaxGroundAcceleration : Settings.MaxAirAcceleration,
				Settings.MaxSwimmingAcceleration, swimFactor);
			
			float speed = Mathf.LerpUnclamped(Settings.MaxGroundSpeed, Settings.MaxSwimmingSpeed, swimFactor);

			Vector3 movementInput = InputHandler.MovementInput + InputHandler.UpDownInput;
			
			Vector3 velocity = Controller.GetMovement(speed, acceleration, movementInput, true);
			
			return velocity;
		}

		private void EvaluateSubmergence()
		{
			Vector3 origin = Controller.BodyPosition + Controller.UpAxis * Settings.SubmergenceOffset;
			if (Physics.Raycast(
				origin,
				-Controller.UpAxis,
				out RaycastHit hit, 
				Settings.SubmergenceRange + 1f, Settings.WaterMask, QueryTriggerInteraction.Collide))
			{
				submergence = 1f - hit.distance / Settings.SubmergenceRange;
			}
			else
			{
				submergence = 1f;
			}
		}
		
		public override string ToString() => "Swimming";
	}
}