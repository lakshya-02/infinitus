using UnityEngine;

[CreateAssetMenu(fileName = "SwingSettings", menuName = "Swinging/Swing Settings", order = 1)]
public class SwingSettings : ScriptableObject
{
    [Header("Grapple Range")]
    public float maxGrappleDistance = 20f;
    
    [Header("Rope Physics")]
    [Range(10f, 200f)]
    public float ropeSpringStiffness = 100f;
    
    [Range(1f, 50f)]
    public float ropeSpringDamping = 10f;
    
    [Header("Swing Forces")]
    [Range(1f, 20f)]
    public float swingForceMultiplier = 5f;
    
    [Range(0.1f, 1f)]
    public float gravityScaleWhileSwinging = 0.6f;
    
    [Range(0f, 10f)]
    public float controllerVelocityBoost = 3f;
    
    [Header("Pull Mechanics")]
    [Range(5f, 50f)]
    public float pullForce = 20f;
    
    [Range(0.1f, 5f)]
    public float pullSpeedMultiplier = 1.5f;
    
    [Header("Rope Visual")]
    [Range(0f, 2f)]
    public float ropeSagAmount = 0.5f;
    [Range(2, 20)]
    public int ropeSegments = 10;
    
    [Header("Haptic Feedback")]
    /// <summary>
    /// Vibration strength when grapple connects (0-1)
    /// </summary>
    [Range(0f, 1f)]
    public float hapticStrength = 0.5f;
    
    /// <summary>
    /// How long vibration lasts (seconds)
    /// </summary>
    [Range(0.05f, 0.5f)]
    public float hapticDuration = 0.1f;
}
