using Controller;
using Controller.States.SuperStates;
using Gravity;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private LayerMask obstructionMask = -1;

    [SerializeField] private Transform focus;
    [SerializeField, Range(1f, 20f)] private float distance = 5f;

    [Header("Focus Settings")]
    [SerializeField, Min(0f)] private float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] private float focusCentering = 0.5f;
    
    [SerializeField, Min(0f)]
    private float upAlignmentSpeed = 360f;
    
    [Header("Orbit Settings")]
    [SerializeField, Range(1f, 360f)]
    private float rotationSpeed = 90f;

    [SerializeField] private float sensibility = 2f;
    
    [SerializeField, Range(-89f, 89f)]
    private float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    
    [SerializeField, Min(0f)]
    private float alignDelay = 5f;
    
    [SerializeField, Range(0f, 90f)] private float alignSmoothRange = 45f;
    
    private float lastManualRotationTime = 0f;

    private Vector2 orbitAngles = new Vector2(45f, 0f);

    private Vector3 focusPoint, previousFocusPoint;

    private Camera regularCamera;
    
    private Quaternion gravityAlignment = Quaternion.identity;

    private Quaternion orbitRotation;

    private Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;

            halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;

            return halfExtends;
        }
    }

    private void Awake()
    {
        focusPoint = focus.position;
        previousFocusPoint = focusPoint;

        regularCamera = GetComponent<Camera>();
        
        transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);
    }

    private void OnValidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
            maxVerticalAngle = minVerticalAngle;
    }

    private void LateUpdate()
    {
        UpdateGravityAlignment();
        
        UpdateFocusPoint();

        if (ClimbRotation() || ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            orbitRotation = Quaternion.Euler(orbitAngles);
        }

        Quaternion lookRotation = gravityAlignment * orbitRotation;
        
        Vector3 lookDirection = lookRotation * Vector3.forward;

        Vector3 lookPosition = focusPoint - lookDirection * distance;

        lookPosition = GetLookPositionWithObstruction(lookDirection, lookPosition, lookRotation);

        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    private Vector3 GetLookPositionWithObstruction(Vector3 lookDirection, Vector3 lookPosition, Quaternion lookRotation)
    {
        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(castFrom, CameraHalfExtends, castDirection, out RaycastHit hit,
            lookRotation, castDistance, obstructionMask, QueryTriggerInteraction.Ignore))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }

        return lookPosition;
    }

    private void UpdateGravityAlignment()
    {
        Vector3 fromUp = gravityAlignment * Vector3.up;
        Vector3 toUp = CustomGravity.GetUpAxis(focusPoint);

        float dot = Mathf.Clamp(Vector3.Dot(fromUp, toUp), -1f, 1f);

        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        float maxAngle = upAlignmentSpeed * Time.deltaTime;

        Quaternion newAlignment = Quaternion.FromToRotation(fromUp, toUp) * gravityAlignment;

        if (angle <= maxAngle)
        {
            gravityAlignment = newAlignment;
        }
        else
        {
            gravityAlignment = Quaternion.SlerpUnclamped(gravityAlignment, newAlignment, maxAngle / angle);
        }
    }

    private void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;

        if (focusRadius > 0f)
        {
            float currentDistance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            
            if (currentDistance > 0.01f && focusCentering > 0f)
            {
                t = Mathf.Pow(1f - focusCentering, Time.unscaledDeltaTime);
            }

            if (currentDistance > focusRadius)
            {
                t = Mathf.Min(t, focusRadius / currentDistance);
            }

            focusPoint = Vector3.Slerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    private bool ClimbRotation()
    {
        if (!focus.TryGetComponent(out SphereController movingSphere)) return false;
        
        if (!(movingSphere.CurrentState is ClimbingState state))
            return false;

        Vector3 direction = Quaternion.Inverse(gravityAlignment) * -state.ClimbNormal;
        
        float headingAngle = GetHorizontalAngle(direction, true);
        float angleDeltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Time.unscaledDeltaTime;

        if (angleDeltaAbs < alignSmoothRange)
            rotationChange *= angleDeltaAbs / alignSmoothRange;
        else if (180f - angleDeltaAbs < alignSmoothRange)
            rotationChange *= (180f - angleDeltaAbs) / alignSmoothRange;
            
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        
        return true;
    }

    private bool ManualRotation()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return false;
        
        Vector2 input = new Vector2(
            -Input.GetAxis("Mouse Y"),
            Input.GetAxis("Mouse X")
        );

        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * sensibility * Time.unscaledDeltaTime * input;
            lastManualRotationTime = Time.unscaledTime;

            return true;
        }

        return false;
    }

    private bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
            return false;

        Vector3 alignedDelta = Quaternion.Inverse(gravityAlignment) * (focusPoint - previousFocusPoint);

        Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

        float movementDeltaSqr = movement.sqrMagnitude;
        if (movementDeltaSqr < 0.000001f)
            return false;

        float headingAngle = GetHorizontalAngle(movement / Mathf.Sqrt(movementDeltaSqr));
        float angleDeltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, movementDeltaSqr);

        if (angleDeltaAbs < alignSmoothRange)
            rotationChange *= angleDeltaAbs / alignSmoothRange;
        else if (180f - angleDeltaAbs < alignSmoothRange)
            rotationChange *= (180f - angleDeltaAbs) / alignSmoothRange;
            
        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);
        
        return true;
    }

    private void ConstrainAngles()
    {
        bool hasGravity = CustomGravity.GetGravity(focusPoint) != Vector3.zero;
        if (hasGravity) 
            orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
            orbitAngles.y += 360f;
        else if (orbitAngles.y > 360f)
            orbitAngles.y -= 360f;
    }

    private static float GetHorizontalAngle(Vector3 direction, bool wallAxis = false)
    {
        float angle = Mathf.Acos(wallAxis ? direction.z : direction.y) * Mathf.Rad2Deg;
        
        return direction.x < 0f ? 360f - angle : angle;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(focusPoint, 0.1f );
    }
}
