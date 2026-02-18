using UnityEngine;

public class GrappleController : MonoBehaviour
{
    [Header("Controller Settings")]
    [SerializeField] private OVRInput.Controller controllerHand = OVRInput.Controller.RTouch;

    [Header("Ray Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float maxGrappleDistance = 20f;
    [SerializeField] private LayerMask grappleableLayers;

    [Header("Rope Visual")]
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private int ropeSegments = 10;
    [SerializeField] private float ropeSag = 0.3f;

    [Header("Haptics")]
    [SerializeField] private float hapticStrength = 0.5f;
    [SerializeField] private float hapticDuration = 0.1f;

    private bool isGrappling = false;
    private Vector3 grapplePoint;
    private SwingPhysicsController physicsController;
    private float hapticTimer = 0f;
    private bool isPulling = false;

    void Start()
    {
        physicsController = GetComponentInParent<SwingPhysicsController>();

        if (physicsController == null)
        {
            Debug.LogError($"[GrappleController] No SwingPhysicsController found on parent! Attach to PlayerBody.");
        }

        if (rayOrigin == null)
        {
            Debug.LogError($"[GrappleController] RayOrigin not assigned! Create empty child GameObject.");
        }

        if (ropeRenderer == null)
        {
            Debug.LogError($"[GrappleController] LineRenderer not assigned! Add LineRenderer component.");
        }

        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = false;
        }
    }

    void Update()
    {
        HandleTriggerInput();

        if (isGrappling)
        {
            UpdateRopeVisual();
        }

        UpdateHaptics();
    }

    void HandleTriggerInput()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controllerHand))
        {
            TryStartGrapple();
        }

        if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, controllerHand))
        {
            EndGrapple();
        }
        
        if (isGrappling)
        {
            bool gripPressed = OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, controllerHand);
            
            if (gripPressed != isPulling)
            {
                isPulling = gripPressed;
                
                if (physicsController != null)
                {
                    physicsController.SetPulling(controllerHand == OVRInput.Controller.RTouch, isPulling);
                }
            }
        }
    }

    void TryStartGrapple()
    {
        if (rayOrigin == null) return;

        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);

        RaycastHit hit;
        bool didHit = Physics.Raycast(
            ray,
            out hit,
            maxGrappleDistance,
            grappleableLayers
        );

        if (didHit)
        {
            grapplePoint = hit.point;
            isGrappling = true;
            
            Debug.Log($"[GrappleController] HIT! Object: {hit.collider.gameObject.name}, Layer: {LayerMask.LayerToName(hit.collider.gameObject.layer)}, Distance: {hit.distance:F2}m");


            if (ropeRenderer != null)
            {
                ropeRenderer.enabled = true;
            }

            if (physicsController != null)
            {
                physicsController.StartGrapple(
                    controllerHand == OVRInput.Controller.RTouch,
                    grapplePoint,
                    rayOrigin
                );
            }

            StartHapticFeedback();
        }
        else
        {
            Debug.LogWarning($"[GrappleController] MISSED! Check: Layer=Grappleable, Has Collider, Within {maxGrappleDistance}m");
        }
    }

    void EndGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;

        if (ropeRenderer != null)
        {
            ropeRenderer.enabled = false;
        }

        if (physicsController != null)
        {
            physicsController.EndGrapple(
                controllerHand == OVRInput.Controller.RTouch
            );
        }

        Debug.Log($"[GrappleController] Grapple released");
    }

    void UpdateRopeVisual()
    {
        if (ropeRenderer == null || rayOrigin == null) return;

        ropeRenderer.positionCount = ropeSegments;

        Vector3 startPos = rayOrigin.position;
        Vector3 endPos = grapplePoint;

        for (int i = 0; i < ropeSegments; i++)
        {
            float t = i / (float)(ropeSegments - 1);

            Vector3 position = Vector3.Lerp(startPos, endPos, t);

            float sagAmount = ropeSag * (1f - Mathf.Pow(2f * t - 1f, 2f));
            position.y -= sagAmount;

            ropeRenderer.SetPosition(i, position);
        }
    }

    void StartHapticFeedback()
    {
        hapticTimer = hapticDuration;
    }

    void UpdateHaptics()
    {
        if (hapticTimer > 0f)
        {
            OVRInput.SetControllerVibration(
                1f,
                hapticStrength,
                controllerHand
            );

            hapticTimer -= Time.deltaTime;
        }
        else
        {
            OVRInput.SetControllerVibration(0f, 0f, controllerHand);
        }
    }

    public bool IsGrappling => isGrappling;

    public Vector3 GrapplePoint => grapplePoint;

    void OnDrawGizmos()
    {
        if (rayOrigin == null) return;

        Gizmos.color = isGrappling ? Color.green : Color.red;
        Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * maxGrappleDistance);

        if (isGrappling)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(grapplePoint, 0.2f);
        }
    }
}