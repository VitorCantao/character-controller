using UnityEngine;

namespace Controller.States
{
    [CreateAssetMenu(fileName = "States Settings", menuName = "States/Settings")]
    public class StatesSettings : ScriptableObject
    {
        [Header("Ground Movement")]
        [SerializeField, Range(0f, 100f)] private float maxGroundSpeed = 9;
        [SerializeField, Range(0f, 100f)] private float maxGroundAcceleration = 30f;
        
        [Header("Air Movement")]
        [SerializeField, Range(0f, 100f)] private float maxAirSpeed = 9;
        [SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 30f;
        [SerializeField, Range(1f, 10f)] private float fallMultiplier = 2f;
        
        [Header("Jump")]
        [SerializeField, Range(0.1f, 10f)] private float jumpHeight = 2.1f;
        [SerializeField, Range(0, 2f)] private float coyoteJumpTimeWindow = 0.2f;
        [SerializeField, Range(0, 5)] private int maxJumps = 0;

        [Header("Climbing")] 
        [SerializeField, Range(0f, 100f)] private float maxClimbingSpeed = 2f;
        [SerializeField, Range(90, 180)] private float maxClimbAngle = 140f;
        [SerializeField, Range(0f, 100f)] private float maxClimbingAcceleration = 20f;
        [SerializeField] private LayerMask climbMask = -1;
        
        [Header("Swimming")]
        [SerializeField, Range(0f, 100f)] private float maxSwimmingSpeed = 5f;
        [SerializeField, Range(0f, 100f)] private float maxSwimmingAcceleration = 5f;
        [SerializeField, Range(0f, 10f)] private float waterDrag = 1f;
        [SerializeField] private float submergenceOffset = 0.5f;
        [SerializeField, Min(0.1f)] private float submergenceRange = 1f;
        [SerializeField, Range(0.01f, 1f)] private float swimThreshold = 0.5f;
        [SerializeField, Min(0)] private float buoyancy = 1f;
        [SerializeField] private LayerMask waterMask = -1;


        public float MaxGroundSpeed => maxGroundSpeed;
        public float MaxGroundAcceleration => maxGroundAcceleration;
        public float MaxAirSpeed => maxAirSpeed;
        public float MaxAirAcceleration => maxAirAcceleration;
        public float JumpHeight => jumpHeight;
        public float CoyoteJumpTimeWindow => coyoteJumpTimeWindow;
        public int MaxJumps => maxJumps;
        public float MaxClimbingSpeed => maxClimbingSpeed;

        public float MaxClimbAngle => maxClimbAngle;

        public float MaxClimbingAcceleration => maxClimbingAcceleration;

        public float MaxSwimmingSpeed => maxSwimmingSpeed;

        public float MaxSwimmingAcceleration => maxSwimmingAcceleration;

        public float WaterDrag => waterDrag;

        public float SubmergenceOffset => submergenceOffset;

        public float SubmergenceRange => submergenceRange;

        public float SwimThreshold => swimThreshold;

        public float Buoyancy => buoyancy;

        public LayerMask ClimbMask => climbMask;
        public LayerMask WaterMask => waterMask;

        public float FallMultiplier => fallMultiplier;
    }
}
