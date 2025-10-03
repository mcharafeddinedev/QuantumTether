# Player Systems Setup - Reference Guide

## What This Document Contains

This document explains how to build player movement systems including grappling hook mechanics and dash abilities. It covers physics integration, visual effects, audio feedback, and how to make the systems work with upgrades.

## The Player System Overview

### What Each Part Does
- **EnhancedPlayerSwing.cs** - Handles grappling hook physics and mechanics
- **EnhancedPlayerDash.cs** - Provides quick burst movement with cooldown
- **Physics integration** - Uses Unity's DistanceJoint2D and Rigidbody2D for realistic movement
- **Visual feedback** - Renders ropes and provides visual cues for player actions
- **Audio feedback** - Plays sounds for grappling, dashing, and other actions

### How It Works
1. **Grappling System** uses physics joints to create realistic swinging motion
2. **Dash System** applies impulse forces for quick horizontal movement
3. **Visual System** renders ropes and provides feedback for all actions
4. **Audio System** plays appropriate sounds for different actions
5. **Upgrade System** can modify player capabilities and add new features

## Setting Up Player Systems in Future Projects

### Step 1: Basic Player Setup
```csharp
// EnhancedPlayerSwing.cs - Core grappling mechanics
public class EnhancedPlayerSwing : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] private float maxRopeLength = 8f;
    [SerializeField] private LayerMask grappleLayer = 1;
    [SerializeField] private float minGrappleDistance = 1f;
    
    [Header("Physics Settings")]
    [SerializeField] private bool enableCollisionWhileGrappling = true;
    [SerializeField] private bool keepPlayerUpright = true;
    
    [Header("Visual")]
    [SerializeField] private LineRenderer rope;
    [SerializeField] private Color ropeColor = Color.white;
    [SerializeField] private float ropeWidth = 0.1f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] grappleSounds;
    [SerializeField] private AudioClip releaseSound;
    [SerializeField] private AudioSource audioSource;
    
    private DistanceJoint2D joint;
    private Rigidbody2D rb;
    private bool isGrappling = false;
    private Vector2 grapplePoint;
    
    public bool IsGrappling => isGrappling;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        
        // Freeze rotation to keep player upright
        if (rb != null && keepPlayerUpright)
        {
            rb.freezeRotation = true;
        }
        
        // Configure the joint
        joint.enabled = false;
        joint.autoConfigureDistance = false;
        joint.autoConfigureConnectedAnchor = false;
        
        // Setup rope visual
        if (rope != null)
        {
            rope.material = new Material(Shader.Find("Sprites/Default"));
            rope.startColor = ropeColor;
            rope.endColor = ropeColor;
            rope.startWidth = ropeWidth;
            rope.endWidth = ropeWidth;
        }
        
        EnsureAudioSource();
    }
}
```

### Step 2: Input Handling
```csharp
void Update()
{
    // Don't allow grappling if player is dead
    if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
    {
        return;
    }
    
    // Primary grapple input (left mouse button)
    if (Input.GetMouseButtonDown(0))
    {
        if (!isGrappling)
        {
            TryGrapple();
        }
    }
    
    // Release grapple when left mouse is released
    if (Input.GetMouseButtonUp(0) && isGrappling)
    {
        ReleaseGrapple();
    }
    
    // Manual contract when spacebar is held
    if (Input.GetKey(KeyCode.Space) && isGrappling)
    {
        ContractRope();
    }
    
    // Update rope visual
    UpdateRopeVisual();
}
```

### Step 3: Grapple Mechanics
```csharp
void TryGrapple()
{
    if (mainCamera == null) 
    {
        mainCamera = Camera.main;
    }
    
    Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
    Vector2 direction = mousePos - (Vector2)transform.position;
    float distance = direction.magnitude;
    
    bool isInRange = (distance >= minGrappleDistance && distance <= maxRopeLength);
    
    if (isInRange)
    {
        // Use raycast to find exact hit point
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, grappleLayer);
        
        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            joint.connectedAnchor = grapplePoint;
            joint.distance = Vector2.Distance(transform.position, grapplePoint);
            joint.enabled = true;
            isGrappling = true;
            
            // Play random grapple sound
            PlayRandomGrappleSound();
        }
    }
}

void ReleaseGrapple()
{
    if (!isGrappling) return;
    
    joint.enabled = false;
    isGrappling = false;
    
    // Play release sound
    if (audioSource && releaseSound)
    {
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        audioSource.PlayOneShot(releaseSound, sfxVolume);
    }
}
```

### Step 4: Dash System
```csharp
// EnhancedPlayerDash.cs - Dash mechanics
public class EnhancedPlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashCooldown = 1.5f;
    [SerializeField] private bool dashTowardMouse = true;
    
    [Header("References")]
    [SerializeField] private EnhancedPlayerSwing grapplingSystem;
    
    private Rigidbody2D rb;
    private float lastDashTime;
    private bool canDash = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Freeze rotation to keep player upright
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        
        // Auto-detect grappling system if not assigned
        if (grapplingSystem == null)
        {
            grapplingSystem = GetComponent<EnhancedPlayerSwing>();
        }
    }
    
    void Update()
    {
        // Check cooldown
        if (Time.time - lastDashTime >= dashCooldown)
        {
            canDash = true;
        }
        
        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            // Check if we can dash while grappling
            bool isGrappling = grapplingSystem != null && grapplingSystem.IsGrappling;
            if (!isGrappling || canDashWhileGrappling)
            {
                Dash();
            }
        }
    }
    
    void Dash()
    {
        // Get dash direction
        Vector2 dashDirection;
        if (dashTowardMouse)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dashDirection = (mousePos - (Vector2)transform.position).normalized;
        }
        else
        {
            dashDirection = Vector2.right; // Default direction
        }
        
        // Apply dash force
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        
        // Reset cooldown
        lastDashTime = Time.time;
        canDash = false;
    }
}
```

## Key Patterns I Used

### 1. Physics-Based Movement
```csharp
// Use Unity's physics system for realistic movement
private DistanceJoint2D joint;
private Rigidbody2D rb;

// Apply forces instead of direct position changes
rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
```

### 2. Input Validation Pattern
```csharp
// Always check game state before processing input
if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
{
    return;
}
```

### 3. Audio Integration Pattern
```csharp
void EnsureAudioSource()
{
    if (audioSource == null)
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }
    }
}
```

### 4. Visual Feedback Pattern
```csharp
void UpdateRopeVisual()
{
    if (isGrappling && rope != null)
    {
        if (rope.positionCount != 2)
        {
            rope.positionCount = 2;
        }
        
        rope.SetPosition(0, transform.position);
        rope.SetPosition(1, grapplePoint);
    }
    else if (rope != null)
    {
        rope.positionCount = 0;
    }
}
```

## What I Learned

### Good Practices
- **Use physics for movement** - more realistic and fun
- **Always check game state** before processing input
- **Provide visual feedback** for all player actions
- **Use audio feedback** to make actions feel responsive
- **Keep player upright** with freezeRotation for 2D games

### Common Pitfalls
- **Don't forget to configure joints** properly
- **Don't forget to check game state** before input
- **Don't forget to update visuals** every frame
- **Don't forget to handle edge cases** (no camera, no audio, etc.)

### Performance Tips
- **Use object pooling** for visual effects
- **Cache references** instead of finding them every frame
- **Use appropriate physics timesteps** for smooth movement
- **Limit simultaneous audio** to prevent audio spam

## Integration Patterns

### With Game Manager
```csharp
// Check game state before processing input
if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
{
    return;
}
```

### With Audio System
```csharp
// Play sounds with proper volume integration
float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
audioSource.PlayOneShot(grappleSound, sfxVolume);
```

### With Upgrade System
```csharp
// Allow upgrades to modify player capabilities
public void SetMaxRopeLength(float length)
{
    maxRopeLength = Mathf.Clamp(length, 1f, 20f);
}

public void SetDashForce(float force)
{
    dashForce = Mathf.Clamp(force, 0.1f, 50f);
}
```

## Advanced Features

### Secondary Grapple System
```csharp
// Support for multiple grapples
[SerializeField] private bool enableSecondaryGrapple = false;
[SerializeField] private float secondaryGrappleMaxDistance = 8f;

private DistanceJoint2D secondaryJoint;
private bool isSecondaryGrappling = false;

// Right mouse for secondary grapple
if (Input.GetMouseButtonDown(1) && enableSecondaryGrapple)
{
    TrySecondaryGrapple();
}
```

### Auto-Contraction System
```csharp
// Automatic rope shortening for "yoink" effect
[SerializeField] private bool autoContractOnConnect = true;
[SerializeField] private float autoContractAmount = 0.2f;

void AutoContractOnConnect()
{
    if (joint.enabled)
    {
        float targetDistance = originalDistance * (1f - autoContractAmount);
        joint.distance = Mathf.MoveTowards(joint.distance, targetDistance, contractSpeed * Time.deltaTime);
    }
}
```

### Pulse Effects
```csharp
// Visual feedback for grapple attempts
void CreatePulseEffect(Vector2 worldPosition, bool isInRange = false)
{
    if (pulseEffectPrefab != null)
    {
        GameObject pulse = Instantiate(pulseEffectPrefab, worldPosition, Quaternion.identity);
        StartCoroutine(AnimatePulse(pulse, isInRange));
    }
}
```

## Future Improvements

### What I Could Add Next Time
- **Wall running** mechanics
- **Double jump** system
- **Grapple swinging** physics (pendulum motion)
- **Combo systems** (grapple + dash combinations)
- **Player animations** for all actions

### Advanced Features
- **Grapple physics** with realistic rope behavior
- **Player state machine** for complex movement states
- **Input buffering** for precise timing
- **Movement analytics** (track player behavior)

## Quick Setup Checklist

For future projects, here's my quick player system setup:

1. Create player GameObject with Rigidbody2D
2. Add DistanceJoint2D for grappling
3. Create EnhancedPlayerSwing script
4. Create EnhancedPlayerDash script
5. Setup input handling (mouse + keyboard)
6. Add visual feedback (rope rendering)
7. Add audio feedback (grapple/dash sounds)
8. Test physics behavior
9. Add upgrade system integration
10. Test on different platforms

This player system worked really well for Quantum Thread and should work for physics-based movement in other 2D games I work on in the future.
