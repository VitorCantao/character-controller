using Controller.States.SuperStates;
using UnityEngine;

namespace Controller.States.SubStates
{
	public class JumpingState : InAirState
    {
	    private int jumpCount = int.MaxValue;

	    private float lastJumpTime = float.MaxValue;
	    
	    private bool hasJumped = false;
	    private bool canCoyoteJump = false;

	    
	    public override bool CheckStateEnter()
	    {
		    if (Controller.OnGround || Controller.CheckSteepContacts() || CanWallJump())
			    jumpCount = 0;

		    canCoyoteJump = CheckCoyoteWindow();
		    
		    return jumpCount < Settings.MaxJumps || canCoyoteJump;
	    }

	    public override void OnStateEnter()
	    {
		    hasJumped = false;
	    }

	    public override void OnStateFixedUpdate()
	    {
		    float fallMultiplier = GetFallMultiplier();

		    Controller.PreventSnapToGround();
		    Vector3 velocity = Controller.GetMovement(Settings.MaxAirSpeed,
											    Settings.MaxAirAcceleration, 
											    InputHandler.MovementInput);
		    
		    if (!hasJumped)
		    {
			    velocity += GetJumpVelocity();
			    
			    hasJumped = true;
			    jumpCount = canCoyoteJump ? 1 : jumpCount + 1;
			    
			    lastJumpTime = Time.unscaledTime;
		    }
		    
		    Controller.Velocity += velocity + (Controller.Gravity * (Time.deltaTime * fallMultiplier));
		    
		    if (Controller.IsFalling) 
			    Controller.SetState<InAirState>();
	    }

	    private float GetFallMultiplier() => InputHandler.HoldJumpInput ? 1f : Settings.FallMultiplier;

	    private bool CanWallJump()
	    {
		    float dotUp = Vector3.Dot(Controller.UpAxis, Controller.SteepNormal);
		    return Controller.OnSteep && dotUp > -0.01f;
	    }

	    private Vector3 GetJumpVelocity()
	    {
		    Vector3 jumpDirection;

		    if (canCoyoteJump)
			    jumpDirection = Controller.LastContactNormal;
		    else if (Controller.OnGround)
			    jumpDirection = Controller.ContactNormal;
		    else
			    jumpDirection = Controller.SteepNormal;
		    
		    float jumpSpeed = Mathf.Sqrt(2f * Controller.Gravity.magnitude * Settings.JumpHeight);

		    jumpDirection = (jumpDirection + Controller.UpAxis).normalized;
        
		    float alignedJumpSpeed = Vector3.Dot(Controller.Velocity, jumpDirection);
		    if (alignedJumpSpeed > 0f)
		    {
			    jumpSpeed = Mathf.Max(jumpSpeed - alignedJumpSpeed, 0f);
		    }
        
		    return jumpDirection * jumpSpeed;
	    }

	    private bool CheckCoyoteWindow()
	    {
		    double timeSinceLastGrounded = Time.unscaledTime - Controller.LastGroundedTime;
		    float timeSinceLastJump = Time.unscaledTime - lastJumpTime;
		    
		    return timeSinceLastGrounded <= Settings.CoyoteJumpTimeWindow 
					&& !Controller.OnGround
					&& timeSinceLastJump > Settings.CoyoteJumpTimeWindow + Time.fixedDeltaTime;
	    }
	    
	    public override string ToString() => "Jumping";
    }
}
