using UnityEngine;

public class GrappleController : MonoBehaviour
{
    [Header("Controller Settings")]
    /// <summary>
    /// Which controller is this? RTouch = Right, LTouch = Left
    /// </summary>
    [SerializeField] private OVRInput.Controller controllerHand = OVRInput.Controller.RTouch;

    [Header("Ray Settings")]
    /// <summary>
    /// The empty GameObject that determines where ray starts and points
    /// </summary>
    [SerializeField] private Transform rayOrigin;

    /// <summary>
    /// Maximum distance the grapple can reach
    /// </summary>
    [SerializeField] private float maxGrappleDistance = 20f;

    /// <summary>
    /// Which layers can be grappled? Set in Inspector to 'Grappleable' only
    /// </summary>
    [SerializeField] private LayerMask grappleableLayers;

    [Header("Rope Visual")]
    /// <summary>
    /// LineRenderer component for drawing the rope
    /// </summary>
    [SerializeField] private LineRenderer ropeRenderer;

    /// <summary>
    /// How many points in the rope? More = more curved/realistic
    /// </summary>
    [SerializeField] private int ropeSegments = 10;

    /// <summary>
    /// How much the rope sags in the middle (0 = straight, 1 = lots of sag)
    /// </summary>
    [SerializeField] private float ropeSag = 0.3f;

    [Header("Haptics")]
    /// <summary>
    /// Vibration strength when grapple connects (0-1)
    /// </summary>
    [SerializeField] private float hapticStrength = 0.5f;

    /// <summary>
    /// Vibration duration in seconds
    /// </summary>
    [SerializeField] private float hapticDuration = 0.1f;

    private bool isGrappling = false;
    private Vector3 grapplePoint;
    private SwingPhysicsController physicsController;
    private float hapticTimer = 0f;

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

            Debug.Log($"[GrappleController] Grapple connected at {grapplePoint}");
        }
        else
        {
            Debug.Log($"[GrappleController] No grappleable surface within {maxGrappleDistance}m");
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