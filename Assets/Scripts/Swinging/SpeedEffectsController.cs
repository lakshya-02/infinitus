using UnityEngine;

public class SpeedEffectsController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SwingPhysicsController swingPhysics;
    
    [Header("Speed Thresholds")]
    [SerializeField] private float mediumSpeedThreshold = 10f;
    [SerializeField] private float highSpeedThreshold = 20f;
    [SerializeField] private float extremeSpeedThreshold = 30f;
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem speedTrailEffect;
    [SerializeField] private bool enableSpeedLines = true;
    
    [Header("Audio")]
    [SerializeField] private AudioSource windAudioSource;
    [SerializeField] private float maxWindVolume = 0.5f;
    
    [Header("Haptics")]
    [SerializeField] private bool enableSpeedHaptics = true;
    [SerializeField] private float hapticIntensityMultiplier = 0.3f;
    
    private float currentSpeed;
    private SpeedLevel currentSpeedLevel = SpeedLevel.Slow;
    
    private enum SpeedLevel
    {
        Slow,
        Medium,
        High,
        Extreme
    }
    
    void Start()
    {
        if (swingPhysics == null)
        {
            swingPhysics = GetComponent<SwingPhysicsController>();
        }
        
        if (speedTrailEffect != null)
        {
            speedTrailEffect.Stop();
        }
    }
    
    void Update()
    {
        if (swingPhysics == null) return;
        
        currentSpeed = swingPhysics.CurrentSpeed;
        UpdateSpeedLevel();
        ApplySpeedEffects();
    }
    
    void UpdateSpeedLevel()
    {
        SpeedLevel newLevel;
        
        if (currentSpeed >= extremeSpeedThreshold)
            newLevel = SpeedLevel.Extreme;
        else if (currentSpeed >= highSpeedThreshold)
            newLevel = SpeedLevel.High;
        else if (currentSpeed >= mediumSpeedThreshold)
            newLevel = SpeedLevel.Medium;
        else
            newLevel = SpeedLevel.Slow;
        
        if (newLevel != currentSpeedLevel)
        {
            currentSpeedLevel = newLevel;
            OnSpeedLevelChanged(newLevel);
        }
    }
    
    void ApplySpeedEffects()
    {
        UpdateVisualEffects();
        UpdateAudioEffects();
        UpdateHapticEffects();
    }
    
    void UpdateVisualEffects()
    {
        if (speedTrailEffect == null) return;
        
        if (currentSpeedLevel >= SpeedLevel.High && !speedTrailEffect.isPlaying)
        {
            speedTrailEffect.Play();
        }
        else if (currentSpeedLevel < SpeedLevel.High && speedTrailEffect.isPlaying)
        {
            speedTrailEffect.Stop();
        }
        
        if (speedTrailEffect.isPlaying)
        {
            var emission = speedTrailEffect.emission;
            float emissionRate = Mathf.Lerp(10f, 50f, currentSpeed / extremeSpeedThreshold);
            emission.rateOverTime = emissionRate;
        }
    }
    
    void UpdateAudioEffects()
    {
        if (windAudioSource == null) return;
        
        float speedRatio = Mathf.Clamp01(currentSpeed / extremeSpeedThreshold);
        windAudioSource.volume = speedRatio * maxWindVolume;
        windAudioSource.pitch = 0.8f + (speedRatio * 0.4f);
        
        if (currentSpeedLevel >= SpeedLevel.Medium && !windAudioSource.isPlaying)
        {
            windAudioSource.Play();
        }
        else if (currentSpeedLevel < SpeedLevel.Medium && windAudioSource.isPlaying)
        {
            windAudioSource.Stop();
        }
    }
    
    void UpdateHapticEffects()
    {
        if (!enableSpeedHaptics) return;
        
        if (currentSpeedLevel >= SpeedLevel.High)
        {
            float intensity = (currentSpeed / extremeSpeedThreshold) * hapticIntensityMultiplier;
            intensity = Mathf.Clamp01(intensity);
            
            OVRInput.SetControllerVibration(0.5f, intensity, OVRInput.Controller.Touch);
        }
    }
    
    void OnSpeedLevelChanged(SpeedLevel newLevel)
    {
        Debug.Log($"[SpeedEffects] Speed level changed to: {newLevel} (Speed: {currentSpeed:F1} m/s)");
    }
    
    public float GetCurrentSpeed() => currentSpeed;
    public string GetSpeedLevelString() => currentSpeedLevel.ToString();
}
