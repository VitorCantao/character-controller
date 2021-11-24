using UnityEngine;

namespace Controller.States.SuperStates
{
    public class InAirState : ControllerState
    {
        public override void OnStateUpdate()
        {
            if (InputHandler.JumpPressedInput)
            {
                Controller.SetState<SubStates.JumpingState>();
            }
        }

        public override void OnStateFixedUpdate()
        {
            if (InputHandler.ClimbInput)
                Controller.SetState<ClimbingState>();
            
            if (Controller.GetColliderOfLayer(Settings.WaterMask))
                Controller.SetState<SwimmingState>();
            
            else if (Controller.OnGround || Controller.CheckSteepContacts())
            {
                Controller.SetState<GroundedState>();
            }
            
            HandleMovement();
        }

        private void HandleMovement()
        {
            Vector3 velocity = Controller.GetMovement(Settings.MaxAirSpeed, 
                Settings.MaxAirAcceleration, InputHandler.MovementInput);
            
            float fallMultiplier = GetFallMultiplier();

            Vector3 gravity = Controller.Gravity;
            
            if (Controller.OnSteep)
                gravity = Controller.SlopeDirection * gravity.magnitude;
            
            Controller.Velocity += velocity + (gravity * (Time.deltaTime * fallMultiplier));
        }

        private float GetFallMultiplier()
        {
            return Controller.IsFalling || Controller.OnSteep ? Settings.FallMultiplier : 1f;
        }

        public override string ToString() => "In Air";
    }
}
