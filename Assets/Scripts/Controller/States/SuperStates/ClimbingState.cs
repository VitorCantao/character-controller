using UnityEngine;

namespace Controller.States.SuperStates
{
    [System.Serializable]
    public class ClimbingState : ControllerState
    {
        public Vector3 ClimbNormal { get; private set; }
        public Vector3 LastClimbNormal { get; private set; }
        

        public override bool CheckStateEnter()
        {
            SetClimbNormal();

            return ClimbNormal != Vector3.zero;
        }

        public override void OnStateUpdate()
        {
            if (!InputHandler.ClimbInput)
            {
                ChangeState();
            }
            
            if (InputHandler.JumpPressedInput)
            {
                WallClimb();
            }
        }

        private void WallClimb()
        {
            SetClimbNormal();
            Controller.SetContactNormal(ClimbNormal);
            Controller.SetState<SubStates.JumpingState>();
        }

        private void ChangeState()
        {
            if (Controller.OnGround)
                Controller.SetState<GroundedState>();
            else
                Controller.SetState<InAirState>();
        }

        public override void OnStateFixedUpdate()
        {
            Controller.PreventSnapToGround();
            SetClimbNormal();
            
            if (ClimbNormal == Vector3.zero)
                ChangeState();
            
            HandleClimbingMovement();
            LastClimbNormal = ClimbNormal;
        }

        private void HandleClimbingMovement()
        {
            Vector3 climbingGrip = ClimbNormal * (Settings.MaxClimbingAcceleration * 0.9f * Time.fixedDeltaTime);

            Vector3 upAxis = Controller.UpAxis;
            Vector3 rightAxis = Vector3.Cross(ClimbNormal, upAxis);
            
            Controller.SetMovementAxis(rightAxis, upAxis);
            Controller.SetContactNormal(ClimbNormal);

            Vector3 movementVelocity = Controller.GetMovement(
                Settings.MaxClimbingSpeed, 
                Settings.MaxClimbingAcceleration,
                InputHandler.MovementInput
            );
            
            Controller.Velocity += movementVelocity - climbingGrip;
        }

        private void SetClimbNormal()
        {
            ClimbNormal = Vector3.zero;

            foreach (ControllerCollisionInfo collisionInfo in Controller.CollisionsInfo)
            {
                if (!IsClimbMask(collisionInfo)) continue;
                
                for (var i = 0; i < collisionInfo.Normals.Length; i++)
                    if (IsClimbingAngle(collisionInfo.Angles[i]))
                        ClimbNormal += collisionInfo.Normals[i];
            }
            
            ClimbNormal = ClimbNormal.normalized;
        }
        
        private bool IsClimbMask(ControllerCollisionInfo collisionInfo)
        {
            return (Settings.ClimbMask & (1 << collisionInfo.Layer)) != 0;
        }

        private bool IsClimbingAngle(float angle)
        {
            return angle <= Settings.MaxClimbAngle;
        }
        
        public override string ToString() => "Climbing";
    }
}
