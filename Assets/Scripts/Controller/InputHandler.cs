using UnityEngine;

namespace Controller
{
    public class InputHandler : MonoBehaviour
    {
        public Vector3 MovementInput { get; private set; }
        public Vector3 UpDownInput { get; private set; }
        public bool JumpPressedInput { get; private set; }
        public bool HoldJumpInput { get; private set; }
        public bool ClimbInput { get; private set; }

        private void Update()
        {
            MovementInput = new Vector3(
                Input.GetAxis("Horizontal"), 
                0f, 
                Input.GetAxis("Vertical")
                );
            MovementInput = Vector3.ClampMagnitude(MovementInput, 1f);
            
            UpDownInput = new Vector3(0f, Input.GetAxis("UpDown"), 0f);
            
            JumpPressedInput = Input.GetButtonDown("Jump");

            HoldJumpInput = Input.GetButton("Jump");
            ClimbInput = Input.GetButton("Climb");
        }
    }
}
