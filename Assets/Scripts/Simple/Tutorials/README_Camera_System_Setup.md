# Camera System Setup - Reference Guide

## What This Document Contains

This document explains how to build a camera system that auto-scrolls with increasing speed, detects when the player falls behind, and includes a threshold shove system for speed boosts. It covers speed ramping, death detection, and integration with other game systems.

## The Camera System Overview

### What Each Part Does
- **EnhancedCamera.cs** - Controls camera movement and speed ramping
- **Death detection** - Kills the player when they fall too far behind
- **Threshold shove system** - Provides speed boost when player crosses screen center
- **Grace period** - Prevents immediate death after restart

### How It Works
1. **Camera Movement** moves horizontally at increasing speed over time
2. **Speed Ramping** uses AnimationCurve for smooth difficulty progression
3. **Death Detection** monitors player position relative to camera
4. **Threshold Shove** triggers speed boost when player crosses screen middle
5. **Grace Period** prevents death immediately after restart

## Setting Up Camera System in Future Projects

### Step 1: Basic Camera Setup
```csharp
// EnhancedCamera.cs - Core camera mechanics
public class EnhancedCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform player;
    
    [Header("Speed Settings")]
    [SerializeField] private float startSpeed = 3f;
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float rampDuration = 100f;
    [SerializeField] private AnimationCurve speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Death Settings")]
    [SerializeField] public float deathDistance = 3f;
    [SerializeField] public float gracePeriod = 2f;
    
    private float currentSpeed;
    private float gameTime;
    private float graceTimer;
    
    void Start()
    {
        currentSpeed = startSpeed;
        graceTimer = gracePeriod;
        
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
}
```

### Step 2: Speed Ramping System
```csharp
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
    currentSpeed = baseSpeed;
    
    // Move camera horizontally only
    transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    
    // Lock Y position to fixed value
    transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
}
```

### Step 3: Death Detection
```csharp
void Update()
{
    // ... speed ramping code ...
    
    // Check if player is too far behind (only after grace period)
    if (player != null && graceTimer <= 0)
    {
        float distanceBehind = transform.position.x - player.position.x;
        if (distanceBehind > deathDistance)
        {
            EnhancedGameManager.Instance?.Die();
        }
        
        // Check if player fell below ground level
        if (player.position.y < -5f) // 2 units below ground
        {
            EnhancedGameManager.Instance?.Die();
        }
    }
}
```

### Step 4: Threshold Shove System
```csharp
[Header("Threshold Shove")]
[SerializeField] private bool enableThresholdShove = true;
[SerializeField] private float thresholdPosition = 0.5f; // 0.5 = middle of screen
[SerializeField] private float baseShoveSpeedBoost = 15f;
[SerializeField] private float shoveDuration = 1f;
[SerializeField] private AnimationCurve shoveCurve = new AnimationCurve(
    new Keyframe(0f, 0f),
    new Keyframe(0.1f, 1f),
    new Keyframe(0.9f, 1f),
    new Keyframe(1f, 0f)
);

private bool hasCrossedThreshold = false;
private float lastShoveTime = -10f;
private float shoveTimer = 0f;
private bool isShoving = false;

void Update()
{
    // ... existing code ...
    
    // Handle threshold shove
    if (enableThresholdShove && player != null)
    {
        HandleThresholdShove();
    }
    
    // Calculate final speed (base speed + shove boost)
    float shoveBoost = 0f;
    if (isShoving)
    {
        float shoveProgress = 1f - (shoveTimer / shoveDuration);
        float curveValue = shoveCurve.Evaluate(shoveProgress);
        shoveBoost = baseShoveSpeedBoost * curveValue;
    }
    float finalSpeed = currentSpeed + shoveBoost;
    
    // Move camera with final speed
    transform.Translate(Vector3.right * finalSpeed * Time.deltaTime);
}
```

### Step 5: Threshold Detection Logic
```csharp
void HandleThresholdShove()
{
    // Convert player world position to screen position
    Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(player.position);
    float screenX = playerScreenPos.x / Screen.width;
    
    // Check if player is currently on the right side of the threshold
    bool isOnRightSide = screenX > thresholdPosition;
    
    if (isOnRightSide && !hasCrossedThreshold)
    {
        // Player just crossed the threshold from left to right
        hasCrossedThreshold = true;
        TriggerCameraShove();
    }
    else if (!isOnRightSide && hasCrossedThreshold)
    {
        // Player moved back to left side, reset for next crossing
        hasCrossedThreshold = false;
    }
}

void TriggerCameraShove()
{
    // Check cooldown
    if (Time.time - lastShoveTime < 8f) // 8 second cooldown
    {
        return;
    }
    
    // Start smooth shove
    isShoving = true;
    shoveTimer = shoveDuration;
    lastShoveTime = Time.time;
}
```

## Key Patterns I Used

### 1. Speed Ramping Pattern
```csharp
// Use AnimationCurve for smooth speed progression
float speedProgress = Mathf.Clamp01(gameTime / rampDuration);
float baseSpeed = Mathf.Lerp(startSpeed, maxSpeed, speedCurve.Evaluate(speedProgress));
```

### 2. Death Detection Pattern
```csharp
// Check distance behind camera
float distanceBehind = transform.position.x - player.position.x;
if (distanceBehind > deathDistance)
{
    EnhancedGameManager.Instance?.Die();
}
```

### 3. Grace Period Pattern
```csharp
// Prevent immediate death after restart
if (graceTimer > 0)
{
    graceTimer -= Time.deltaTime;
}
// Only check death after grace period
if (player != null && graceTimer <= 0)
{
    // Check for death
}
```

### 4. Threshold Detection Pattern
```csharp
// Convert world position to screen position
Vector3 playerScreenPos = Camera.main.WorldToScreenPoint(player.position);
float screenX = playerScreenPos.x / Screen.width;

// Check if player crossed threshold
bool isOnRightSide = screenX > thresholdPosition;
```

## What I Learned

### Good Practices
- **Use AnimationCurve** for smooth speed progression
- **Always check for null** before accessing player
- **Use grace periods** to prevent frustrating deaths
- **Convert world to screen coordinates** for UI-based detection
- **Use cooldowns** to prevent spam

### Common Pitfalls
- **Don't forget to reset state** on game restart
- **Don't forget to unsubscribe** from events
- **Don't forget to check grace period** before death detection
- **Don't forget to handle edge cases** (no player, no camera, etc.)

### Performance Tips
- **Cache camera reference** instead of finding it every frame
- **Use efficient distance calculations** (avoid sqrt when possible)
- **Limit threshold checks** to reasonable frequency
- **Use appropriate update frequencies** for different systems

## Integration Patterns

### With Game Manager
```csharp
// Subscribe to restart events
EnhancedGameManager.OnGameRestart += OnGameRestart;

void OnGameRestart()
{
    graceTimer = gracePeriod;
    gameTime = 0f;
    currentSpeed = startSpeed;
    // Reset threshold state
    hasCrossedThreshold = false;
    isShoving = false;
}
```

### With Scoring System
```csharp
// Provide speed data for scoring
public float GetCurrentSpeed()
{
    return currentSpeed;
}

public float GetStartSpeed()
{
    return startSpeed;
}
```

### With Spawning System
```csharp
// Provide camera bounds for spawning
public Bounds GetCameraBounds()
{
    Camera cam = GetComponent<Camera>();
    float height = cam.orthographicSize * 2f;
    float width = height * cam.aspect;
    
    return new Bounds(transform.position, new Vector3(width, height, 0f));
}
```

## Advanced Features

### Dynamic Speed Multipliers
```csharp
// Allow upgrades to modify camera speed
private float speedMultiplier = 1f;

public void SetSpeedMultiplier(float multiplier)
{
    speedMultiplier = Mathf.Clamp(multiplier, 0.3f, 2f);
}

// Apply multiplier to final speed
float finalSpeed = (currentSpeed + shoveBoost) * speedMultiplier;
```

### Camera Size Control
```csharp
// Allow upgrades to modify camera size
private float sizeMultiplier = 1f;
private float baseOrthographicSize;

public void SetSizeMultiplier(float multiplier)
{
    sizeMultiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
    UpdateCameraSize();
}

private void UpdateCameraSize()
{
    Camera cam = GetComponent<Camera>();
    if (cam != null)
    {
        cam.orthographicSize = baseOrthographicSize * sizeMultiplier;
    }
}
```

### Smooth Threshold Transitions
```csharp
// Use smooth curves for threshold effects
[SerializeField] private AnimationCurve shoveCurve = new AnimationCurve(
    new Keyframe(0f, 0f),      // Start at 0
    new Keyframe(0.1f, 1f),     // Quick ramp up
    new Keyframe(0.9f, 1f),     // Hold at max
    new Keyframe(1f, 0f)        // Quick ramp down
);
```

## Future Improvements

### What I Could Add Next Time
- **Camera shake** effects for impacts
- **Dynamic camera following** (follow player vertically)
- **Camera zones** (different behavior in different areas)
- **Camera transitions** (smooth movement between states)
- **Camera effects** (screen distortion, color changes)

### Advanced Features
- **Camera state machine** for complex behaviors
- **Camera analytics** (track player position patterns)
- **Camera accessibility** (motion sickness options)
- **Camera recording** (for replay systems)

## Quick Setup Checklist

For future projects, here's my quick camera system setup:

1. Create camera GameObject with Camera component
2. Create EnhancedCamera script
3. Setup speed ramping with AnimationCurve
4. Implement death detection
5. Add grace period system
6. Implement threshold shove system
7. Add game event subscriptions
8. Test speed progression
9. Test death detection
10. Test threshold shove mechanics

This camera system worked really well for Quantum Thread and should work for auto-scrolling cameras in other 2D games I work on in the future.
