using System.Collections.Generic;
using System.Linq;
using Controller.States;
using Controller.States.SuperStates;
using Gravity;
using UnityEngine;

namespace Controller
{
    public struct ControllerCollisionInfo
    {
        public Vector3[] Normals;
        public float[] Angles;
        public LayerMask Layer;
    }
    
    public class SphereController : MonoBehaviour
    {
        [SerializeField] private StatesSettings statesSettings;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private Transform playerInputSpace = default;

        [SerializeField, Range(0, 90f)] private float maxGroundAngle = 25, maxStairsAngle = 50;

        [SerializeField, Range(0f, 100f)] private float maxSnapSpeed = 100f;

        [SerializeField, Range(15f, 100f)] private float terminalVelocity = 30f;

        [SerializeField, Min(0f)] private float probeDistance = 1f;

        [SerializeField] public LayerMask probeMask = -1, stairsMask = -1;

        
        public ControllerState CurrentState { get; private set; }

        /// <summary>Force based on near gravity sources. </summary>
        public Vector3 Gravity { get; private set; }
        
        public Vector3 ContactNormal { get; private set; }
        public Vector3 SteepNormal { get; private set; }
        
        public Vector3 LastContactNormal { get; private set; }
        public Vector3 LastSteepNormal { get; private set; }
        
        public float LastGroundedTime { get; private set; }
        public int StepsSinceLastGrounded { get; private set; }

        public Vector3 UpAxis { get; private set; }
        public Vector3 RightAxis { get; private set; }
        public Vector3 ForwardAxis { get; private set; }
        
        public Vector3 BodyPosition => body.position;
        
        public bool OnGround => groundContactCount > 0;
        public bool OnSteep => steepContactCount > 0;
        public bool IsFalling => !OnGround && Vector3.Dot(velocity, Gravity) > 0;
        
        /// <summary> Direction pointing downward the slope.</summary>
        public Vector3 SlopeDirection => ProjectDirectionOnPlane(Gravity, SteepNormal);

        public List<ControllerCollisionInfo> CollisionsInfo { get; } = new List<ControllerCollisionInfo>();

        /// <summary> Velocity to be computed by the end of fixed update. </summary>
        public Vector3 Velocity
        {
            get => velocity;
            set => velocity = value;
        }

        /// <summary> Movement relative to the connected rigidbody, like a moving platform. </summary>
        public Vector3 RelativeMovement => (body.velocity - lastConnectedBodyVelocity) * Time.deltaTime;
        public Vector3 RelativeVelocity => velocity - connectedBodyHandler.Velocity;

        public Vector3 AngularVelocity => connectedBodyHandler.AngularVelocity;
        
        
        private int groundContactCount;
        private int steepContactCount;

        private Rigidbody body;

        private float minGroundDotProduct;
        private float minStairsDotProduct;
        
        private Vector3 velocity;
        
        private Vector3 lastConnectedBodyVelocity;
        
        private ConnectedBodyHandler connectedBodyHandler;
        
        private bool canSnapToGround = true;

        private List<ControllerState> states;

        private List<Collision> Collisions { get; } = new List<Collision>();
        private List<Collider> Triggers { get; } = new List<Collider>();
        

        /// <summary>
        /// Change the controller FSM state.
        /// </summary>
        /// <typeparam name="TStateType"></typeparam>
        /// <returns>Success or failure</returns>
        public bool SetState<TStateType>() where TStateType : ControllerState
        {
            foreach (TStateType state in states.OfType<TStateType>())
            {
                SetState(state);
                return true;
            }

            return false;
        }

        public void SetState(ControllerState state)
        {
            if (!state.CheckStateEnter())
                return;
            
            ControllerState previousState = CurrentState;
            CurrentState = state;

            if (previousState)
                previousState.OnStateExit();

            CurrentState.OnStateEnter();
        }

        public void PreventSnapToGround()
        {
            canSnapToGround = false;
        }
        
        /// <param name="layer"></param>
        /// <returns>Collider. Null if none is present.</returns>
        public Collider GetColliderOfLayer(LayerMask layer)
        {
            foreach (Collider trigger in Triggers)
            {
                if ((layer & (1 << trigger.gameObject.layer)) != 0)
                    return trigger;
            }

            return null;
        }
        
        public void SetMovementAxis(Vector3 rightAxis, Vector3 forwardAxis)
        {
            RightAxis = rightAxis;
            ForwardAxis = forwardAxis;
        }
        
        /// <summary>
        /// Gets the movement vector based on the right/forward axis and the contact normal.
        /// </summary>
        public Vector3 GetMovement(float speed, float acceleration, Vector3 movementInput, bool hasYMovement = false)
        {
            Vector3 xAxis = ProjectDirectionOnPlane(RightAxis, ContactNormal);
            Vector3 zAxis = ProjectDirectionOnPlane(ForwardAxis, ContactNormal);

            Vector3 adjustment;
            adjustment.x = movementInput.x * speed - Vector3.Dot(RelativeVelocity, xAxis);
            adjustment.z = movementInput.z * speed - Vector3.Dot(RelativeVelocity, zAxis);
            adjustment.y = hasYMovement ? movementInput.y * speed - Vector3.Dot(RelativeVelocity, UpAxis) : 0f;

            float maxSpeedChange = acceleration * Time.fixedDeltaTime;

            adjustment = Vector3.ClampMagnitude(adjustment, maxSpeedChange);

            Vector3 yVelocity = Vector3.zero;
            if (hasYMovement)
                yVelocity = adjustment.y * UpAxis;
            
            return xAxis * adjustment.x + zAxis * adjustment.z + yVelocity;
        }

        /// <summary> Checks if the controller is touching multiple steep contacts. </summary>
        public bool CheckSteepContacts()
        {
            if (steepContactCount > 1)
            {
                SteepNormal.Normalize();

                float upDot = Vector3.Dot(UpAxis, SteepNormal);
                if (upDot >= minGroundDotProduct)
                {
                    steepContactCount = 0;
                    groundContactCount = 1;
                    ContactNormal = SteepNormal;
                    return true;
                }
            }

            return false;
        }

        public void SetContactNormal(Vector3 normal)
        {
            ContactNormal = normal;
            groundContactCount = 1;
            StepsSinceLastGrounded = 0;
        }


        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            connectedBodyHandler = new ConnectedBodyHandler(body);
            
            states = GetComponents<ControllerState>().ToList();
            states.ForEach((s) => s.Initialize(this, inputHandler, statesSettings));
            SetState(states.OfType<GroundedState>().First());

            OnValidate();
        }

        private void OnValidate()
        {
            minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
            minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);
        }

        private void Update()
        {
            if (playerInputSpace)
            {
                RightAxis = ProjectDirectionOnPlane(playerInputSpace.right, UpAxis);
                ForwardAxis = ProjectDirectionOnPlane(playerInputSpace.forward, UpAxis);
            }
            else
            {
                RightAxis = ProjectDirectionOnPlane(Vector3.right, UpAxis);
                ForwardAxis = ProjectDirectionOnPlane(Vector3.forward, UpAxis);
            }

            CurrentState.OnStateUpdate();
        }

        private void FixedUpdate()
        {
            Gravity = CustomGravity.GetGravity(body.position, out Vector3 upAxis);
            UpAxis = upAxis;

            UpdateInternalState();

            CurrentState.OnStateFixedUpdate();

            body.velocity = Vector3.ClampMagnitude(velocity, terminalVelocity);

            ClearInternalState();
        }

        private void UpdateInternalState()
        {
            StepsSinceLastGrounded += 1;
            velocity = body.velocity;

            if (OnGround || SnapToGround())
            {
                StepsSinceLastGrounded = 0;
                LastGroundedTime = Time.unscaledTime;
                
                if (groundContactCount > 1)
                    ContactNormal.Normalize();
            }

            canSnapToGround = true;
            connectedBodyHandler.UpdateState();
        }

        private bool SnapToGround()
        {
            if (!canSnapToGround || StepsSinceLastGrounded > 1)
                return false;

            float speed = velocity.magnitude;
            if (speed > maxSnapSpeed)
                return false;

            if (!Physics.Raycast(body.position, -UpAxis, out RaycastHit hit,
                probeDistance,
                probeMask,
                QueryTriggerInteraction.Ignore))
                return false;

            float upDot = Vector3.Dot(UpAxis, hit.normal);
            if (upDot < GetMinDot(hit.collider.gameObject.layer))
                return false;

            groundContactCount = 1;
            ContactNormal = hit.normal;

            float dot = Vector3.Dot(velocity, hit.normal);

            if (dot > 0f)
                velocity = (velocity - hit.normal * dot).normalized * speed;

            connectedBodyHandler.Connect(hit.rigidbody);

            return true;
        }

        private Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
        {
            return (direction - normal * Vector3.Dot(direction, normal)).normalized;
        }

        private void OnCollisionEnter(Collision collision)
        {
            Collisions.Add(collision);
            EvaluateCollision(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            Collisions.Add(collision);
            EvaluateCollision(collision);
        }

        private void OnTriggerEnter(Collider other)
        {
            Triggers.Add(other);
        }

        private void OnTriggerStay(Collider other)
        {
            Triggers.Add(other);
            
            if (other.attachedRigidbody)
                connectedBodyHandler.Connect(other.attachedRigidbody);
        }

        private void EvaluateCollision(Collision collision)
        {
            int layer = collision.gameObject.layer;
            float minDot = GetMinDot(layer);

            var collisionInfo = new ControllerCollisionInfo
            {
                Layer = layer,
                Normals = new Vector3[collision.contactCount], 
                Angles = new float[collision.contactCount]
            };

            for (var i = 0; i < collision.contactCount; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                collisionInfo.Normals[i] = normal;
                collisionInfo.Angles[i] = Vector3.Angle(UpAxis, normal);

                float dotUp = Vector3.Dot(UpAxis, normal);
                
                if (dotUp >= minDot)
                {
                    groundContactCount += 1;
                    ContactNormal += normal;

                    connectedBodyHandler.Connect(collision.rigidbody);
                }
                else
                {
                    steepContactCount += 1;
                    SteepNormal += normal;

                    if (groundContactCount == 0)
                        connectedBodyHandler.Connect(collision.rigidbody);
                }
            }
            
            CollisionsInfo.Add(collisionInfo);
        }

        private float GetMinDot(int layer)
        {
            return (stairsMask & (1 << layer)) != 0 ? minStairsDotProduct : minGroundDotProduct;
        }

        private void ClearInternalState()
        {
            lastConnectedBodyVelocity = connectedBodyHandler.Velocity;
            connectedBodyHandler.ClearState();
            
            if (ContactNormal != Vector3.zero)
                LastContactNormal = ContactNormal;
            
            if (LastSteepNormal != Vector3.zero)
                LastSteepNormal = SteepNormal;

            ContactNormal = SteepNormal = Vector3.zero;
            groundContactCount = steepContactCount = 0;
            
            Collisions.Clear();
            CollisionsInfo.Clear();
            Triggers.Clear();
        }
    }
}
