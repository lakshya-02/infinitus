using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwingPhysicsController : MonoBehaviour
{
    [Header("Swing Physics Settings")]
    [SerializeField] private SwingSettings settings;
    
    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugGizmos = true;
    
    private class GrappleState
    {
        public bool isActive;
        public Vector3 grapplePoint;
        public Transform handTransform;
        public float initialDistance;
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
    }
    
    public void StartGrapple(bool isRightHand, Vector3 grapplePoint, Transform handTransform)
    {
        GrappleState grapple = isRightHand ? rightGrapple : leftGrapple;
        
        float distance = Vector3.Distance(rb.position, grapplePoint);
        
        grapple.isActive = true;
        grapple.grapplePoint = grapplePoint;
        grapple.handTransform = handTransform;
        grapple.initialDistance = distance;
        
        Debug.Log($"[SwingPhysics] {(isRightHand ? "Right" : "Left")} grapple started. Distance: {distance:F2}m");
    }
    
    public void EndGrapple(bool isRightHand)
    {
        GrappleState grapple = isRightHand ? rightGrapple : leftGrapple;
        
        grapple.isActive = false;
        
        if (!rightGrapple.isActive && !leftGrapple.isActive)
        {
            Physics.gravity = new Vector3(0, -9.81f, 0);
        }
        
        Debug.Log($"[SwingPhysics] {(isRightHand ? "Right" : "Left")} grapple released");
    }
    
    void ApplySwingPhysics()
    {
        if (settings == null || rb == null) return;
        
        Physics.gravity = new Vector3(0, -9.81f * settings.gravityScaleWhileSwinging, 0);
        
        Vector3 totalForce = Vector3.zero;
        
        if (rightGrapple.isActive)
        {
            totalForce += CalculateSwingForce(rightGrapple);
        }
        
        if (leftGrapple.isActive)
        {
            totalForce += CalculateSwingForce(leftGrapple);
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
            float swingSpeed = Vector3.Dot(rb.linearVelocity, swingDirection);
            pendulumForce = swingDirection * settings.swingForceMultiplier;
        }
        
        Vector3 inputBoost = Vector3.zero;
        
        if (grapple.handTransform != null)
        {
            Vector3 controllerVelocity = OVRInput.GetLocalControllerVelocity(
                grapple == rightGrapple ? OVRInput.Controller.RTouch : OVRInput.Controller.LTouch
            );
            
            if (controllerVelocity.magnitude > 0.1f)
            {
                inputBoost = controllerVelocity * settings.controllerVelocityBoost;
            }
        }
        
        Vector3 totalForce = springForce + pendulumForce + inputBoost;
        
        return totalForce;
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
