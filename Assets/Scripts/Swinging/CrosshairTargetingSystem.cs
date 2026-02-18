using UnityEngine;
using UnityEngine.UI;

public class CrosshairTargetingSystem : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private Image crosshairImage;
    [SerializeField] private Color normalColor = new Color(1, 1, 1, 0.5f);
    [SerializeField] private Color targetColor = new Color(0, 1, 1, 1f);
    [SerializeField] private float crosshairSize = 20f;
    
    [Header("Targeting")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask grappleableLayers;
    [SerializeField] private bool showTargetInfo = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private float targetPulseSpeed = 2f;
    [SerializeField] private float targetPulseAmount = 1.2f;
    
    private GrapplePoint currentTarget;
    private bool isTargeting = false;
    private float pulseTimer = 0f;
    private RectTransform crosshairRect;
    
    void Start()
    {
        if (crosshairImage != null)
        {
            crosshairRect = crosshairImage.GetComponent<RectTransform>();
            crosshairRect.sizeDelta = new Vector2(crosshairSize, crosshairSize);
            crosshairImage.color = normalColor;
        }
    }
    
    void Update()
    {
        UpdateTargeting();
        UpdateCrosshairVisuals();
    }
    
    void UpdateTargeting()
    {
        if (rayOrigin == null) return;
        
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;
        
        GrapplePoint previousTarget = currentTarget;
        currentTarget = null;
        isTargeting = false;
        
        if (Physics.Raycast(ray, out hit, maxDistance, grappleableLayers))
        {
            GrapplePoint grapplePoint = hit.collider.GetComponent<GrapplePoint>();
            
            if (grapplePoint != null && grapplePoint.IsActive)
            {
                currentTarget = grapplePoint;
                isTargeting = true;
                
                if (previousTarget != currentTarget)
                {
                    currentTarget.OnTargeted();
                }
            }
        }
        
        if (previousTarget != null && previousTarget != currentTarget)
        {
            previousTarget.OnUntargeted();
        }
    }
    
    void UpdateCrosshairVisuals()
    {
        if (crosshairImage == null) return;
        
        if (isTargeting)
        {
            pulseTimer += Time.deltaTime * targetPulseSpeed;
            float pulse = 1f + Mathf.Sin(pulseTimer) * (targetPulseAmount - 1f) * 0.5f;
            
            crosshairRect.sizeDelta = new Vector2(crosshairSize * pulse, crosshairSize * pulse);
            crosshairImage.color = Color.Lerp(crosshairImage.color, targetColor, Time.deltaTime * 10f);
        }
        else
        {
            crosshairRect.sizeDelta = Vector2.Lerp(crosshairRect.sizeDelta, 
                new Vector2(crosshairSize, crosshairSize), Time.deltaTime * 10f);
            crosshairImage.color = Color.Lerp(crosshairImage.color, normalColor, Time.deltaTime * 10f);
            pulseTimer = 0f;
        }
    }
    
    public bool IsTargetingGrapplePoint() => isTargeting;
    public GrapplePoint GetCurrentTarget() => currentTarget;
    public Vector3 GetTargetPosition() => currentTarget != null ? currentTarget.GetGrapplePosition() : Vector3.zero;
    
    void OnDrawGizmos()
    {
        if (rayOrigin == null) return;
        
        Gizmos.color = isTargeting ? Color.green : Color.red;
        Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * maxDistance);
    }
}
