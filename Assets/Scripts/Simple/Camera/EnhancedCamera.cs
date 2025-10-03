using UnityEngine;

/// <summary>
/// Enhanced camera with smooth following and speed ramping
/// This is the improved version that actually works well
/// </summary>
public class EnhancedCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField, Tooltip("Player to follow")]
    private Transform player;
    
    [Header("Speed Settings")]
    [SerializeField, Tooltip("Initial camera speed")]
    private float startSpeed = 3f;
    
    [SerializeField, Tooltip("Maximum camera speed")]
    private float maxSpeed = 12f;
    
    [SerializeField, Tooltip("Time to reach max speed (seconds)")]
    private float rampDuration = 100f;
    
    [SerializeField, Tooltip("Speed increase curve")]
    private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Position")]
    [SerializeField, Tooltip("Fixed Y position for camera")]
    private float fixedY = 0f;
    
    [SerializeField, Tooltip("Ground Y position for death detection")]
    private float groundY = -5f;
    
    [Header("Death Settings")]
    [SerializeField, Tooltip("Distance behind camera before death")]
    public float deathDistance = 3f;
    
    [SerializeField, Tooltip("Grace period after restart")]
    public float gracePeriod = 2f;
    
    [Header("Threshold Shove")]
    [SerializeField, Tooltip("Enable camera shove when player crosses middle threshold")]
    private bool enableThresholdShove = true;
    
    [SerializeField, Tooltip("Screen position threshold (0.5 = middle, 0.3 = left side, 0.7 = right side)")]
    [Range(0.1f, 0.9f)]
    private float thresholdPosition = 0.5f;
    
    [SerializeField, Tooltip("Base shove speed boost (added to base speed)")]
    private float baseShoveSpeedBoost = 15f;
    
    [SerializeField, Tooltip("How much the shove boost reduces over time (0 = no reduction, 1 = full reduction)")]
    [Range(0f, 1f)]
    private float shoveReductionRate = 0.5f;
    
    [SerializeField, Tooltip("Shove duration (how long the shove lasts)")]
    private float shoveDuration = 1f;
    
    [SerializeField, Tooltip("Shove acceleration curve (how the shove builds up and fades)")]
    private AnimationCurve shoveCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.1f, 1f),
        new Keyframe(0.9f, 1f),
        new Keyframe(1f, 0f)
    );
    
    [SerializeField, Tooltip("Maximum shove speed multiplier")]
    private float maxShoveMultiplier = 5f;
    
    [SerializeField, Tooltip("Cooldown between shoves (seconds)")]
    private float shoveCooldown = 8f;
    
    
    private float currentSpeed;
    private float gameTime;
    private float graceTimer;
    private Vector3 targetPosition;
    
    // Threshold shove variables
    private bool hasCrossedThreshold = false;
    private float lastShoveTime = -10f; // Initialize to allow immediate first shove
    private float shoveTimer = 0f;
    private bool isShoving = false;
    private float shoveSpeedMultiplier = 1f;
    private float lastPlayerScreenX = 0f;
    private bool isPlayerMovingForward = false;
    private float shoveStartTime = 0f;
    
    void Start()
    {
        currentSpeed = startSpeed;
        graceTimer = gracePeriod;
        
        // Initialize camera size
        camera = GetComponent<Camera>();
        if (camera != null)
        {
            baseOrthographicSize = camera.orthographicSize;
        }
        
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        // Subscribe to game events
        EnhancedGameManager.OnGameRestart += OnGameRestart;
    }
    
    void OnDestroy()
    {
        EnhancedGameManager.OnGameRestart -= OnGameRestart;
    }
    
    void Update()
    {
        // Update grace timer
        if (graceTimer > 0)
        {
            graceTimer -= Time.deltaTime;
        }
        
        // Increase speed over time
        gameTime += Time.deltaTime;
        float speedProgress = Mathf.Clamp01(gameTime / rampDuration);
        float baseSpeed = Mathf.Lerp(startSpeed, maxSpeed, speedCurve.Evaluate(speedProgress));
        currentSpeed = baseSpeed * speedMultiplier;
        
        // Handle threshold shove
        if (enableThresholdShove && player != null)
        {
            HandleThresholdShove();
        }
        
        
        
        // Calculate final speed (base speed + scaled shove boost)
        float shoveBoost = 0f;
        if (isShoving)
        {
            float shoveProgress = 1f - (shoveTimer / shoveDuration);
            float curveValue = shoveCurve.Evaluate(shoveProgress);
            
            // Calculate scaled shove boost that reduces over time
            float gameSpeedProgress = Mathf.Clamp01(gameTime / rampDuration);
            float shoveScale = 1f - (gameSpeedProgress * shoveReductionRate);
            float scaledShoveBoost = baseShoveSpeedBoost * shoveScale * curveValue;
            
            shoveBoost = scaledShoveBoost;
        }
        float finalSpeed = currentSpeed + shoveBoost;
        
        
        // Move camera horizontally only
        transform.Translate(Vector3.right * finalSpeed * Time.deltaTime);
        
        // Lock Y position to fixed value
        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
        
        // Check if player is too far behind (only after grace period)
        if (player != null && graceTimer <= 0)
        {
            float distanceBehind = transform.position.x - player.position.x;
            if (distanceBehind > deathDistance)
            {
                EnhancedGameManager.Instance?.Die();
            }
            
            // Check if player fell below ground level
            if (player.position.y < groundY - 2f) // 2 units below ground
            {
                EnhancedGameManager.Instance?.Die();
            }
        }
        
        // Update shove timer and apply smooth curve
        if (isShoving)
        {
            shoveTimer -= Time.deltaTime;
            if (shoveTimer <= 0f)
            {
                isShoving = false;
                shoveSpeedMultiplier = 1f;
            }
            else
            {
            }
        }
        
    }
    
    /// <summary>
    /// Handle threshold crossing detection and camera shove
    /// </summary>
    void HandleThresholdShove()
    {
        // Convert player world position to screen position using this camera
        Vector3 playerScreenPos = camera.WorldToScreenPoint(player.position);
        float screenX = playerScreenPos.x / Screen.width;
        
        // Store previous position before updating
        float previousScreenX = lastPlayerScreenX;
        
        // Determine if player is moving forward (left to right on screen)
        if (lastPlayerScreenX != 0f)
        {
            isPlayerMovingForward = screenX > lastPlayerScreenX;
        }
        else
        {
            // First frame - assume moving forward
            isPlayerMovingForward = true;
        }
        lastPlayerScreenX = screenX;
        
        // Check if player is currently on the right side of the threshold
        bool isOnRightSide = screenX > thresholdPosition;
        
        
        if (isOnRightSide && !hasCrossedThreshold)
        {
            // Player just crossed the threshold from left to right
            hasCrossedThreshold = true;
            
            // Always trigger on first crossing, or if player is moving forward
            bool shouldTrigger = (previousScreenX == 0f) || isPlayerMovingForward;
            
            if (shouldTrigger)
            {
                TriggerCameraShove();
            }
            else
            {
            }
        }
        else if (!isOnRightSide && hasCrossedThreshold)
        {
            // Player moved back to left side, reset for next crossing
            hasCrossedThreshold = false;
        }
    }
    
    /// <summary>
    /// Trigger a camera shove forward
    /// </summary>
    void TriggerCameraShove()
    {
        // Check cooldown
        if (Time.time - lastShoveTime < shoveCooldown)
        {
            return;
        }
        
        // Start smooth shove
        isShoving = true;
        shoveTimer = shoveDuration;
        shoveStartTime = Time.time;
        shoveSpeedMultiplier = 1f; // Start at normal speed
        lastShoveTime = Time.time;
        
        
        // Calculate current shove scale
        float speedProgress = Mathf.Clamp01(gameTime / rampDuration);
        float shoveScale = 1f - (speedProgress * shoveReductionRate);
        float currentShoveBoost = baseShoveSpeedBoost * shoveScale;
        
    }
    
    void OnGameRestart()
    {
        graceTimer = gracePeriod;
        gameTime = 0f;
        currentSpeed = startSpeed;
        speedMultiplier = 1f; // Reset speed multiplier on restart
        sizeMultiplier = 1f; // Reset size multiplier on restart
        UpdateCameraSize(); // Apply the reset
        
        // Reset threshold shove state
        hasCrossedThreshold = false;
        isShoving = false;
        shoveSpeedMultiplier = 1f;
        shoveTimer = 0f;
        lastPlayerScreenX = 0f;
        isPlayerMovingForward = false;
        shoveStartTime = 0f;
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw death distance
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + Vector3.left * deathDistance, transform.position + Vector3.left * deathDistance + Vector3.up * 5f);
    }
    
    /// <summary>
    /// Get the current camera speed for other systems
    /// </summary>
    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }
    
    /// <summary>
    /// Get the starting camera speed for other systems
    /// </summary>
    public float GetStartSpeed()
    {
        return startSpeed;
    }
    
    // Upgrade system methods
    private float speedMultiplier = 1f;
    private float sizeMultiplier = 1f;
    private float baseOrthographicSize;
    private new Camera camera;
    
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = Mathf.Clamp(multiplier, 0.3f, 2f);
    }
    
    public float GetSpeedMultiplier()
    {
        return speedMultiplier;
    }
    
    public void SetSizeMultiplier(float multiplier)
    {
        sizeMultiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
        UpdateCameraSize();
    }
    
    public float GetSizeMultiplier()
    {
        return sizeMultiplier;
    }
    
    private void UpdateCameraSize()
    {
        if (camera != null)
        {
            camera.orthographicSize = baseOrthographicSize * sizeMultiplier;
        }
    }
    
    public void SetGracePeriod(float gracePeriod)
    {
        this.gracePeriod = Mathf.Clamp(gracePeriod, 0f, 10f);
    }
    
    public void SetDeathDistance(float distance)
    {
        this.deathDistance = Mathf.Clamp(distance, 1f, 20f);
    }
    
    /// <summary>
    /// Enable or disable threshold shove
    /// </summary>
    public void SetThresholdShoveEnabled(bool enabled)
    {
        enableThresholdShove = enabled;
    }
    
    /// <summary>
    /// Set the threshold position (0.5 = middle, 0.3 = left side, 0.7 = right side)
    /// </summary>
    public void SetThresholdPosition(float position)
    {
        thresholdPosition = Mathf.Clamp(position, 0.1f, 0.9f);
    }
    
    /// <summary>
    /// Set the shove force
    /// </summary>
    public void SetShoveForce(float force)
    {
        baseShoveSpeedBoost = Mathf.Max(0f, force);
    }
    
    /// <summary>
    /// Set the shove duration
    /// </summary>
    public void SetShoveDuration(float duration)
    {
        shoveDuration = Mathf.Max(0.1f, duration);
    }
    
    /// <summary>
    /// Set the maximum shove multiplier
    /// </summary>
    public void SetMaxShoveMultiplier(float multiplier)
    {
        maxShoveMultiplier = Mathf.Max(0.1f, multiplier);
    }
    
    /// <summary>
    /// Force trigger a camera shove (for testing)
    /// </summary>
    [ContextMenu("Force Camera Shove")]
    public void ForceCameraShove()
    {
        float speedProgress = Mathf.Clamp01(gameTime / rampDuration);
        float shoveScale = 1f - (speedProgress * shoveReductionRate);
        float currentShoveBoost = baseShoveSpeedBoost * shoveScale;
        // Reset threshold state to allow immediate shove
        hasCrossedThreshold = false;
        TriggerCameraShove();
    }
    
    /// <summary>
    /// Get current shove settings for debugging
    /// </summary>
    public void LogShoveSettings()
    {
        float speedProgress = Mathf.Clamp01(gameTime / rampDuration);
        float shoveScale = 1f - (speedProgress * shoveReductionRate);
    }
    
    /// <summary>
    /// Set the shove curve for smooth transitions
    /// </summary>
    public void SetShoveCurve(AnimationCurve curve)
    {
        shoveCurve = curve;
    }
    
    /// <summary>
    /// Set the shove cooldown
    /// </summary>
    public void SetShoveCooldown(float cooldown)
    {
        shoveCooldown = Mathf.Max(0.1f, cooldown);
    }
    
    /// <summary>
    /// Get the current shove speed multiplier
    /// </summary>
    public float GetShoveMultiplier()
    {
        return shoveSpeedMultiplier;
    }
    
    /// <summary>
    /// Check if camera is currently shoving
    /// </summary>
    public bool IsShoving()
    {
        return isShoving;
    }
    
    /// <summary>
    /// Manually trigger a camera shove (for testing)
    /// </summary>
    [ContextMenu("Trigger Camera Shove")]
    public void ManualTriggerShove()
    {
        TriggerCameraShove();
    }
}
