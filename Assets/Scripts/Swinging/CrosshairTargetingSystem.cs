using UnityEngine;
using UnityEngine.UI;

public class CrosshairTargetingSystem : MonoBehaviour
{
    [Header("Dual Hand Setup")]
    [SerializeField] private bool useDualCrosshairs = true;
    
    [Header("Right Hand Crosshair")]
    [SerializeField] private Image rightCrosshairImage;
    [SerializeField] private Transform rightRayOrigin;
    [SerializeField] private Color rightNormalColor = new Color(1, 0.5f, 0, 0.5f);
    [SerializeField] private Color rightTargetColor = new Color(1, 0.8f, 0, 1f);
    
    [Header("Left Hand Crosshair")]
    [SerializeField] private Image leftCrosshairImage;
    [SerializeField] private Transform leftRayOrigin;
    [SerializeField] private Color leftNormalColor = new Color(0, 0.5f, 1, 0.5f);
    [SerializeField] private Color leftTargetColor = new Color(0, 0.8f, 1, 1f);
    
    [Header("Shared Settings")]
    [SerializeField] private float crosshairSize = 20f;
    [SerializeField] private float maxDistance = 50f;
    [SerializeField] private LayerMask grappleableLayers;
    
    [Header("Visual Feedback")]
    [SerializeField] private float targetPulseSpeed = 2f;
    [SerializeField] private float targetPulseAmount = 1.2f;
    
    private class HandCrosshair
    {
        public Image image;
        public RectTransform rectTransform;
        public Transform rayOrigin;
        public Color normalColor;
        public Color targetColor;
        public GrapplePoint currentTarget;
        public bool isTargeting;
        public float pulseTimer;
    }
    
    private HandCrosshair rightHand = new HandCrosshair();
    private HandCrosshair leftHand = new HandCrosshair();
    
    void Start()
    {
        InitializeHand(rightHand, rightCrosshairImage, rightRayOrigin, rightNormalColor, rightTargetColor);
        
        if (useDualCrosshairs)
        {
            InitializeHand(leftHand, leftCrosshairImage, leftRayOrigin, leftNormalColor, leftTargetColor);
        }
    }
    
    void InitializeHand(HandCrosshair hand, Image image, Transform rayOrigin, Color normalColor, Color targetColor)
    {
        hand.image = image;
        hand.rayOrigin = rayOrigin;
        hand.normalColor = normalColor;
        hand.targetColor = targetColor;
        
        if (hand.image != null)
        {
            hand.rectTransform = hand.image.GetComponent<RectTransform>();
            hand.rectTransform.sizeDelta = new Vector2(crosshairSize, crosshairSize);
            hand.image.color = normalColor;
        }
    }
    
    void Update()
    {
        UpdateHandTargeting(rightHand);
        UpdateHandVisuals(rightHand);
        
        if (useDualCrosshairs)
        {
            UpdateHandTargeting(leftHand);
            UpdateHandVisuals(leftHand);
        }
    }
    
    void UpdateHandTargeting(HandCrosshair hand)
    {
        if (hand.rayOrigin == null) return;
        
        Ray ray = new Ray(hand.rayOrigin.position, hand.rayOrigin.forward);
        RaycastHit hit;
        
        GrapplePoint previousTarget = hand.currentTarget;
        hand.currentTarget = null;
        hand.isTargeting = false;
        
        if (Physics.Raycast(ray, out hit, maxDistance, grappleableLayers))
        {
            GrapplePoint grapplePoint = hit.collider.GetComponent<GrapplePoint>();
            
            if (grapplePoint != null && grapplePoint.IsActive)
            {
                hand.currentTarget = grapplePoint;
                hand.isTargeting = true;
                
                if (previousTarget != hand.currentTarget)
                {
                    hand.currentTarget.OnTargeted();
                }
            }
        }
        
        if (previousTarget != null && previousTarget != hand.currentTarget)
        {
            previousTarget.OnUntargeted();
        }
    }
    
    void UpdateHandVisuals(HandCrosshair hand)
    {
        if (hand.image == null) return;
        
        if (hand.isTargeting)
        {
            hand.pulseTimer += Time.deltaTime * targetPulseSpeed;
            float pulse = 1f + Mathf.Sin(hand.pulseTimer) * (targetPulseAmount - 1f) * 0.5f;
            
            hand.rectTransform.sizeDelta = new Vector2(crosshairSize * pulse, crosshairSize * pulse);
            hand.image.color = Color.Lerp(hand.image.color, hand.targetColor, Time.deltaTime * 10f);
        }
        else
        {
            hand.rectTransform.sizeDelta = Vector2.Lerp(hand.rectTransform.sizeDelta, 
                new Vector2(crosshairSize, crosshairSize), Time.deltaTime * 10f);
            hand.image.color = Color.Lerp(hand.image.color, hand.normalColor, Time.deltaTime * 10f);
            hand.pulseTimer = 0f;
        }
    }
    
    public bool IsRightHandTargeting() => rightHand.isTargeting;
    public bool IsLeftHandTargeting() => leftHand.isTargeting;
    public GrapplePoint GetRightHandTarget() => rightHand.currentTarget;
    public GrapplePoint GetLeftHandTarget() => leftHand.currentTarget;
    
    void OnDrawGizmos()
    {
        if (rightHand.rayOrigin != null)
        {
            Gizmos.color = rightHand.isTargeting ? Color.green : Color.red;
            Gizmos.DrawRay(rightHand.rayOrigin.position, rightHand.rayOrigin.forward * maxDistance);
        }
        
        if (useDualCrosshairs && leftHand.rayOrigin != null)
        {
            Gizmos.color = leftHand.isTargeting ? Color.cyan : Color.yellow;
            Gizmos.DrawRay(leftHand.rayOrigin.position, leftHand.rayOrigin.forward * maxDistance);
        }
    }
}
