using UnityEngine;
using Controller.States.SuperStates;

namespace Controller.States.SubStates
{
	public class FallingState : InAirState
	{
		public override void OnStateFixedUpdate()
		{
			Vector3 velocity = Controller.GetMovement(Settings.MaxAirSpeed, 
				Settings.MaxAirAcceleration, InputHandler.MovementInput);
			
			Controller.Velocity += velocity + (Controller.Gravity * (Time.deltaTime * Settings.FallMultiplier));

			base.OnStateFixedUpdate();
		}
		
		public override string ToString() => "Falling";
	}
}