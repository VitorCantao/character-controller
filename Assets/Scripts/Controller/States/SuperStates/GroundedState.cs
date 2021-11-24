using UnityEngine;

namespace Controller.States.SuperStates
{
    public class GroundedState : ControllerState
    {
        private bool wantsToClimb = false;
        private int stepsOutsideGround = 0;

        private const int InAirStepsOffset = 2;

        public override void OnStateUpdate()
        {
            if (InputHandler.JumpPressedInput)
            {
                Controller.SetState<SubStates.JumpingState>();
            }

            wantsToClimb = InputHandler.ClimbInput;
        }

        public override void OnStateFixedUpdate()
        {
            CheckGrounded();

            if (Controller.GetColliderOfLayer(Settings.WaterMask))
                Controller.SetState<SwimmingState>();
            
            if (wantsToClimb)
                Controller.SetState<ClimbingState>();
            
            HandleMovement();
        }

        private void CheckGrounded()
        {
            if (!Controller.OnGround)
            {
                stepsOutsideGround++;

                if (stepsOutsideGround > InAirStepsOffset)
                    Controller.SetState<InAirState>();
            }
            else
            {
                stepsOutsideGround = 0;
            }
        }

        private void HandleMovement()
        {
            float speed = wantsToClimb ? Settings.MaxClimbingSpeed : Settings.MaxGroundSpeed;

            Vector3 velocity = Controller.GetMovement(speed, Settings.MaxGroundAcceleration, InputHandler.MovementInput);

            Vector3 downForce = GetDownForce();
            
            Controller.Velocity += velocity + downForce;
        }

        private Vector3 GetDownForce()
        {
            return Controller.ContactNormal *
                   (Vector3.Dot(Controller.Gravity, Controller.ContactNormal) * Time.deltaTime);
        }
        
        public override string ToString() => "Grounded";
    }
}
