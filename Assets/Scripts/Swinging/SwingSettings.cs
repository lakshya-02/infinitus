using UnityEngine;

[CreateAssetMenu(fileName = "SwingSettings", menuName = "Swinging/Swing Settings", order = 1)]
public class SwingSettings : ScriptableObject
{
    [Header("Grapple Range")]
    /// <summary>
    /// Maximum distance player can grapple (meters)
    /// </summary>
    public float maxGrappleDistance = 20f;
    
    [Header("Rope Physics")]
    /// <summary>
    /// How stiff is the rope? Higher = rope stays taut, Lower = more stretch
    /// </summary>
    [Range(10f, 200f)]
    public float ropeSpringStiffness = 100f;
    
    /// <summary>
    /// Dampens rope bouncing. Higher = less bouncy, Lower = more bouncy
    /// </summary>
    [Range(1f, 50f)]
    public float ropeSpringDamping = 10f;
    
    [Header("Swing Forces")]
    /// <summary>
    /// How much force to apply for swinging motion. Higher = faster swings
    /// </summary>
    [Range(1f, 20f)]
    public float swingForceMultiplier = 5f;
    
    /// <summary>
    /// Gravity multiplier while swinging. Lower = more floaty/arcade feel
    /// </summary>
    [Range(0.1f, 1f)]
    public float gravityScaleWhileSwinging = 0.6f;
    
    /// <summary>
    /// How much controller movement boosts swing. Higher = more responsive
    /// </summary>
    [Range(0f, 10f)]
    public float controllerVelocityBoost = 3f;
    
    [Header("Rope Visual")]
    /// <summary>
    /// How much the rope sags visually (0 = straight line, 1 = lots of sag)
    /// </summary>
    [Range(0f, 2f)]
    public float ropeSagAmount = 0.5f;
    
    /// <summary>
    /// Number of segments in rope visual (more = smoother curve)
    /// </summary>
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
