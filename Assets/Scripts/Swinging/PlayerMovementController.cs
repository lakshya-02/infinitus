using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Ground Movement")]
    [SerializeField] private float groundMoveSpeed = 5f;
    [SerializeField] private float groundAcceleration = 10f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCheckDistance = 1.5f;

    [Header("Rotation Control")]
    [SerializeField] private bool lockYRotation = true;
    [SerializeField] private bool lockAllRotation = true;
    [SerializeField] private Transform cameraRig;

    private Rigidbody rb;
    private bool isGrounded;
    private Quaternion lockedRotation;
    private SwingPhysicsController swingPhysics;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        swingPhysics = GetComponent<SwingPhysicsController>();

        if (swingPhysics == null)
        {
            Debug.LogWarning("[PlayerMovement] SwingPhysicsController not found on this GameObject. Grapple state cannot be read.");
        }

        if (cameraRig == null)
        {
            cameraRig = GetComponentInChildren<OVRCameraRig>()?.transform;
        }

        // Capture locked rotation AFTER scene/XR init to get correct initial orientation.
        lockedRotation = transform.rotation;

        if (lockAllRotation)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
            Debug.Log("[PlayerMovement] All rotation LOCKED via constraints");
        }
        else if (lockYRotation)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        
        rb.maxAngularVelocity = 0f;
        // NOTE: Automatic Tensor in Inspector must be UNCHECKED for inertiaTensor to hold.
        rb.automaticInertiaTensor = false;
        rb.inertiaTensor = Vector3.one * 0.0001f;

        Debug.Log($"[PlayerMovement] Movement controller initialized. Ground layers: {groundLayers.value}");
    }
    
    void FixedUpdate()
    {
        CheckGrounded();
        
        bool isGrappling = swingPhysics != null && swingPhysics.IsGrappling;
        
        if (isGrounded && !isGrappling)
        {
            HandleGroundMovement();
        }
        else if (isGrappling)
        {
            // While grappling, kill any residual horizontal velocity that does not
            // come from the swing physics (e.g. leftover ground-movement momentum).
            // Vertical velocity is left intact so gravity and pull forces work.
            Vector3 vel = rb.linearVelocity;
            vel.x = Mathf.MoveTowards(vel.x, 0f, groundAcceleration * Time.fixedDeltaTime);
            vel.z = Mathf.MoveTowards(vel.z, 0f, groundAcceleration * Time.fixedDeltaTime);
            rb.linearVelocity = vel;
        }
        
        if (lockAllRotation)
        {
            rb.angularVelocity = Vector3.zero;
            transform.rotation = lockedRotation;
        }
        else if (lockYRotation)
        {
            rb.angularVelocity = Vector3.zero;
            Vector3 euler = transform.eulerAngles;
            euler.x = lockedRotation.eulerAngles.x;
            euler.z = lockedRotation.eulerAngles.z;
            transform.rotation = Quaternion.Euler(euler);
        }
    }
    
    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayers);
        
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[PlayerMovement] Grounded: {isGrounded}, Distance: {groundCheckDistance}m");
        }
    }
    
    void HandleGroundMovement()
    {
        Vector2 thumbstick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);
        
        if (Time.frameCount % 60 == 0 && thumbstick.magnitude > 0.05f)
        {
            Debug.Log($"[PlayerMovement] Thumbstick: {thumbstick}, CameraRig: {cameraRig != null}");
        }
        
        if (thumbstick.magnitude > 0.1f && cameraRig != null)
        {
            Vector3 forward = cameraRig.forward;
            Vector3 right = cameraRig.right;
            
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();
            
            Vector3 moveDirection = (forward * thumbstick.y + right * thumbstick.x).normalized;
            Vector3 targetVelocity = moveDirection * groundMoveSpeed;
            
            Vector3 currentVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 velocityChange = (targetVelocity - currentVelocity) * groundAcceleration * Time.fixedDeltaTime;
            
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
    
    public bool IsGrounded => isGrounded;
}
