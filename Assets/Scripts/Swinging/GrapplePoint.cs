using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    [Header("Grapple Point Settings")]
    [SerializeField] private bool isActive = true;
    [SerializeField] private float highlightRadius = 0.5f;
    [SerializeField] private Color highlightColor = Color.cyan;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject visualIndicator;
    [SerializeField] private bool alwaysShowIndicator = true;
    
    private bool isBeingTargeted = false;
    private MeshRenderer indicatorRenderer;
    
    void Start()
    {
        if (visualIndicator != null)
        {
            indicatorRenderer = visualIndicator.GetComponent<MeshRenderer>();
            visualIndicator.SetActive(alwaysShowIndicator);
        }
        
        gameObject.layer = LayerMask.NameToLayer("Grappleable");
    }
    
    public void OnTargeted()
    {
        if (!isActive) return;
        
        isBeingTargeted = true;
        
        if (visualIndicator != null && !alwaysShowIndicator)
        {
            visualIndicator.SetActive(true);
        }
        
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material.SetColor("_Color", highlightColor);
        }
    }
    
    public void OnUntargeted()
    {
        isBeingTargeted = false;
        
        if (visualIndicator != null && !alwaysShowIndicator)
        {
            visualIndicator.SetActive(false);
        }
    }
    
    public void OnGrappled()
    {
        Debug.Log($"[GrapplePoint] Player grappled to {gameObject.name}");
    }
    
    public void OnReleased()
    {
        isBeingTargeted = false;
    }
    
    public bool IsActive => isActive;
    public Vector3 GetGrapplePosition() => transform.position;
    
    void OnDrawGizmos()
    {
        Gizmos.color = isBeingTargeted ? highlightColor : Color.gray;
        Gizmos.DrawWireSphere(transform.position, highlightRadius);
    }
}
