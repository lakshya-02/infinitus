using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwingPhysicsController : MonoBehaviour
{
    [Header("Swing Physics Settings")]
    [SerializeField] private SwingSettings settings;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugGizmos = true;
    
    [Header("Speed Tracking")]
    [SerializeField] private bool trackSpeed = true;
    
    [Header("Velocity Damping")]
    [SerializeField] private float velocityDampOnRelease = 0.7f;
    [SerializeField] private bool dampVelocityOnRelease = true;
    [SerializeField] private bool stopGroundVelocityOnGrapple = true;
    
    [Header("Arm Throwing/Climbing Mechanics")]
    [SerializeField] private bool enableArmThrowing = true;
    [SerializeField] private float armThrowMultiplier = 15f;
    [SerializeField] private float minHandVelocityThreshold = 1f;
    
    public float CurrentSpeed { get; private set; }
    public float MaxSpeedReached { get; private set; }
    public bool IsGrappling => rightGrapple.isActive || leftGrapple.isActive;
    
    private class GrappleState
    {
        public bool isActive;
        public Vector3 grapplePoint;
        public Transform handTransform;
        public float initialDistance;
        public bool isPulling;
    }
    
    private GrappleState rightGrapple = new GrappleState();
    private GrappleState leftGrapple = new GrappleState();
    
    private Rigidbody rb;
    private float normalGravityScale;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("[SwingPhysicsController] No Rigidbody found! Add Rigidbody to PlayerBody.");
        }
        
        if (settings == null)
        {
            Debug.LogError("[SwingPhysicsController] Swing Settings not assigned! Create and assign in Inspector.");
        }
        
        normalGravityScale = 1f;
    }
    
    void FixedUpdate()
    {
        if (rightGrapple.isActive || leftGrapple.isActive)
        {
            ApplySwingPhysics();
        }
        else
        {
            Physics.gravity = new Vector3(0, -9.81f, 0);
        }
        
        if (trackSpeed)
        {
            UpdateSpeedTracking();
        }
    }
    
    public void StartGrapple(bool isRightHand, Vector3 grapplePoint, Transform handTransform)
    {
        GrappleState grapple = isRightHand ? rightGrapple : leftGrapple;
        
        float distance = Vector3.Distance(rb.position, grapplePoint);
        
        grapple.isActive = true;
        grapple.grapplePoint = grapplePoint;
        grapple.handTransform = handTransform;
        grapple.initialDistance = distance;
        
        if (stopGroundVelocityOnGrapple && rb != null)
        {
            Vector3 vel = rb.linearVelocity;
            vel.x = 0f;
            vel.z = 0f;
            rb.linearVelocity = vel;
            Debug.Log($"[SwingPhysics] Ground velocity fully zeroed on grapple start");
        }
        
        Debug.Log($"[SwingPhysics] {(isRightHand ? "Right" : "Left")} grapple started. Distance: {distance:F2}m");
    }
    
    public void EndGrapple(bool isRightHand)
    {
        GrappleState grapple = isRightHand ? rightGrapple : leftGrapple;
        
        grapple.isActive = false;
        grapple.isPulling = false;
        
        if (!rightGrapple.isActive && !leftGrapple.isActive)
        {
            Physics.gravity = new Vector3(0, -9.81f, 0);
            
            if (dampVelocityOnRelease && rb != null)
            {
                rb.linearVelocity *= velocityDampOnRelease;
                Debug.Log($"[SwingPhysics] Velocity damped to: {rb.linearVelocity.magnitude:F1} m/s");
            }
        }
        
        Debug.Log($"[SwingPhysics] {(isRightHand ? "Right" : "Left")} grapple released");
    }
    
    public void SetPulling(bool isRightHand, bool isPulling)
    {
        GrappleState grapple = isRightHand ? rightGrapple : leftGrapple;
        
        if (grapple.isActive)
        {
            grapple.isPulling = isPulling;
            Debug.Log($"[SwingPhysics] {(isRightHand ? "Right" : "Left")} pull: {isPulling}");
        }
    }
    
    void ApplySwingPhysics()
    {
        if (settings == null || rb == null) return;
        
        Physics.gravity = new Vector3(0, -9.81f * settings.gravityScaleWhileSwinging, 0);
        
        Vector3 totalForce = Vector3.zero;
        
        if (rightGrapple.isActive)
        {
            totalForce += CalculateSwingForce(rightGrapple);
            
            if (rightGrapple.isPulling)
            {
                totalForce += CalculatePullForce(rightGrapple);
            }
        }
        
        if (leftGrapple.isActive)
        {
            totalForce += CalculateSwingForce(leftGrapple);
            
            if (leftGrapple.isPulling)
            {
                totalForce += CalculatePullForce(leftGrapple);
            }
        }
        
        if (rightGrapple.isActive && leftGrapple.isActive)
        {
            totalForce *= 0.5f;
        }
        
        rb.AddForce(totalForce, ForceMode.Acceleration);
    }
    
    Vector3 CalculateSwingForce(GrappleState grapple)
    {
        Vector3 toGrapple = grapple.grapplePoint - rb.position;
        float currentDistance = toGrapple.magnitude;
        Vector3 toGrappleDir = toGrapple.normalized;
        
        Vector3 springForce = Vector3.zero;
        float distanceError = currentDistance - grapple.initialDistance;
        
        if (distanceError > 0.01f)
        {
            springForce = toGrappleDir * (distanceError * settings.ropeSpringStiffness);
            
            float velocityAlongRope = Vector3.Dot(rb.linearVelocity, toGrappleDir);
            springForce -= toGrappleDir * (velocityAlongRope * settings.ropeSpringDamping);
        }
        
        Vector3 pendulumForce = Vector3.zero;
        
        Vector3 swingDirection = Vector3.Cross(toGrappleDir, Vector3.up).normalized;
        
        if (swingDirection.magnitude > 0.01f)
        {
            pendulumForce = swingDirection * settings.swingForceMultiplier;
        }
        
        Vector3 armThrowForce = Vector3.zero;
        
        if (enableArmThrowing && grapple.handTransform != null)
        {
            Vector3 controllerVelocity = OVRInput.GetLocalControllerVelocity(
                grapple == rightGrapple ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch
            );
            
            if (controllerVelocity.magnitude > minHandVelocityThreshold)
            {
                Vector3 throwDirection = controllerVelocity.normalized;
                float throwSpeed = controllerVelocity.magnitude;
                
                armThrowForce = throwDirection * throwSpeed * armThrowMultiplier;
                
                if (Time.frameCount % 20 == 0)
                {
                    Debug.Log($"[SwingPhysics] Arm throw: {throwSpeed:F1} m/s, Force: {armThrowForce.magnitude:F1}");
                }
            }
        }
        
        Vector3 totalForce = springForce + pendulumForce + armThrowForce;
        
        return totalForce;
    }
    
    Vector3 CalculatePullForce(GrappleState grapple)
    {
        Vector3 toGrapple = grapple.grapplePoint - rb.position;
        Vector3 pullDirection = toGrapple.normalized;
        
        float pullMagnitude = settings.pullForce * settings.pullSpeedMultiplier;
        
        return pullDirection * pullMagnitude;
    }
    
    void UpdateSpeedTracking()
    {
        CurrentSpeed = rb.linearVelocity.magnitude;
        
        if (CurrentSpeed > MaxSpeedReached)
        {
            MaxSpeedReached = CurrentSpeed;
        }
    }
    
    void OnDrawGizmos()
    {
        if (!showDebugGizmos || rb == null) return;
        
        if (rightGrapple.isActive)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(rb.position, rightGrapple.grapplePoint);
            Gizmos.DrawWireSphere(rightGrapple.grapplePoint, 0.3f);
        }
        
        if (leftGrapple.isActive)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rb.position, leftGrapple.grapplePoint);
            Gizmos.DrawWireSphere(leftGrapple.grapplePoint, 0.3f);
        }
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rb.position, rb.linearVelocity);
    }
}
