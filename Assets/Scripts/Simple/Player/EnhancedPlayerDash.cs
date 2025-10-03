using UnityEngine;

/// <summary>
/// Enhanced dash mechanics with visual effects - the improved version
/// </summary>
public class EnhancedPlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField, Tooltip("Dash force strength (Left Shift to dash)")]
    private float dashForce = 15f;
    
    [SerializeField, Tooltip("Dash cooldown in seconds")]
    private float dashCooldown = 1.5f;
    
    [SerializeField, Tooltip("Dash direction (toward mouse or fixed)")]
    private bool dashTowardMouse = true;
    
    [SerializeField, Tooltip("Fixed dash direction (if not toward mouse)")]
    private Vector2 fixedDashDirection = Vector2.right;
    
    [SerializeField, Tooltip("Can dash while grappling? (for upgrades)")]
    private bool canDashWhileGrappling = false;
    
    [Header("References")]
    [SerializeField, Tooltip("Reference to grappling system (auto-detected if not assigned)")]
    private EnhancedPlayerSwing grapplingSystem;
    
    [Header("Visual Effects")]
    [SerializeField, Tooltip("Dash trail effect")]
    private GameObject dashTrailPrefab;
    
    [SerializeField, Tooltip("Dash duration")]
    private float dashDuration = 0.2f;
    
    [Header("Cooldown Pulse")]
    [SerializeField, Tooltip("Pulse effect prefab (optional)")]
    private GameObject pulseEffectPrefab;
    
    [SerializeField, Tooltip("Pulse duration")]
    private float pulseDuration = 0.3f;
    
    [SerializeField, Tooltip("Pulse scale")]
    private float pulseScale = 3f;
    
    [SerializeField, Tooltip("Cooldown ready pulse color (green)")]
    private Color cooldownReadyColor = Color.green;
    
    [Header("Audio")]
    [SerializeField, Tooltip("Dash sound")]
    private AudioClip dashSound;
    
    [SerializeField, Tooltip("Audio source")]
    private AudioSource audioSource;
    
    [SerializeField, Tooltip("Dash sound volume multiplier")]
    private float dashVolume = 1f;
    
    
    private Rigidbody2D rb;
    private float lastDashTime;
    private bool canDash = true;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private bool hasShownCooldownPulse = false;
    private GameObject currentPulse;
    private Camera mainCamera;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Freeze rotation to keep player upright
        if (rb != null)
        {
            rb.freezeRotation = true;
        }
        
        // Ensure AudioSource exists
        EnsureAudioSource();
        
        // Auto-detect grappling system if not assigned
        if (grapplingSystem == null)
        {
            grapplingSystem = GetComponent<EnhancedPlayerSwing>();
        }
        
        // Get camera reference
        mainCamera = Camera.main;
    }
    
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
            else
            {
            }
        }
    }
    
    void Update()
    {
        // Check cooldown
        if (Time.time - lastDashTime >= dashCooldown)
        {
            if (!canDash)
            {
                canDash = true;
                Debug.Log("[EnhancedPlayerDash] Dash cooldown ready");
                // Show cooldown ready pulse
                if (!hasShownCooldownPulse)
                {
                    CreateCooldownReadyPulse();
                    hasShownCooldownPulse = true;
                }
            }
        }
        else
        {
            // Reset pulse flag when cooldown starts
            hasShownCooldownPulse = false;
        }
        
        // Dash input
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && !isDashing)
        {
        // Check if we can dash while grappling
        bool isGrappling = grapplingSystem != null && grapplingSystem.IsGrappling;
        if (!isGrappling || canDashWhileGrappling)
        {
            Dash();
        }
        else
        {
            Debug.Log("[EnhancedPlayerDash] Dash blocked - currently grappling and canDashWhileGrappling is false");
        }
        }
        
        // Handle dash duration
        if (isDashing)
        {
            dashTimer += Time.deltaTime;
            if (dashTimer >= dashDuration)
            {
                EndDash();
            }
        }
    }
    
    void Dash()
    {
        // Get dash direction
        Vector2 dashDirection;
        if (dashTowardMouse)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            dashDirection = (mousePos - (Vector2)transform.position).normalized;
        }
        else
        {
            dashDirection = fixedDashDirection.normalized;
        }
        
        // Apply dash force
        rb.AddForce(dashDirection * dashForce, ForceMode2D.Impulse);
        Debug.Log($"[EnhancedPlayerDash] Dash executed - Force: {dashForce}, Direction: {dashDirection}");
        
        // Start dash state
        isDashing = true;
        dashTimer = 0f;
        
        // Reset cooldown
        lastDashTime = Time.time;
        canDash = false;
        
        // Play dash sound
        EnsureAudioSource();
        if (audioSource && dashSound)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            float finalVolume = sfxVolume * dashVolume;
            audioSource.PlayOneShot(dashSound, finalVolume);
        }
        
        // Create dash trail effect
        if (dashTrailPrefab != null)
        {
            GameObject trail = Instantiate(dashTrailPrefab, transform.position, Quaternion.identity);
            Destroy(trail, dashDuration);
        }
        
    }
    
    void EndDash()
    {
        isDashing = false;
        dashTimer = 0f;
    }
    
    void CreateCooldownReadyPulse()
    {
        // Destroy previous pulse if it exists
        if (currentPulse != null)
        {
            Destroy(currentPulse);
        }
        
        if (pulseEffectPrefab != null)
        {
            // Create pulse effect at player position
            currentPulse = Instantiate(pulseEffectPrefab, transform.position, Quaternion.identity);
            
            // Start pulse animation coroutine
            StartCoroutine(AnimateCooldownPulse(currentPulse));
            
        }
        else
        {
            // Fallback: create a simple visual effect using a sprite
            CreateSimpleCooldownPulse();
        }
    }
    
    void CreateSimpleCooldownPulse()
    {
        // Create a simple circle sprite for pulse effect
        currentPulse = new GameObject("CooldownReadyPulse");
        currentPulse.transform.position = transform.position;
        
        // Add sprite renderer
        SpriteRenderer sr = currentPulse.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        sr.color = new Color(cooldownReadyColor.r, cooldownReadyColor.g, cooldownReadyColor.b, 0.8f);
        
        // Start pulse animation
        StartCoroutine(AnimateCooldownPulse(currentPulse));
    }
    
    Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = 1f - (distance / radius);
                alpha = Mathf.Clamp01(alpha);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
    
    System.Collections.IEnumerator AnimateCooldownPulse(GameObject pulse)
    {
        if (pulse == null) yield break;
        
        Vector3 originalScale = Vector3.one;
        float elapsed = 0f;
        
        while (elapsed < pulseDuration && pulse != null)
        {
            float progress = elapsed / pulseDuration;
            
            // Track player position
            pulse.transform.position = transform.position;
            
            // Scale up and fade out
            float scale = Mathf.Lerp(0.1f, pulseScale, progress);
            float alpha = Mathf.Lerp(1f, 0f, progress);
            
            pulse.transform.localScale = originalScale * scale;
            
            // Update alpha if it has a sprite renderer
            SpriteRenderer sr = pulse.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = cooldownReadyColor;
                color.a = alpha;
                sr.color = color;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (pulse != null)
        {
            Destroy(pulse);
        }
        
        // Clear the reference
        if (currentPulse == pulse)
        {
            currentPulse = null;
        }
    }
    
    public bool CanDash => canDash && !isDashing;
    
    public float DashCooldownRemaining => Mathf.Max(0f, dashCooldown - (Time.time - lastDashTime));
    
    // Public method for upgrades to enable/disable grappling dash
    public void SetCanDashWhileGrappling(bool canDash)
    {
        canDashWhileGrappling = canDash;
    }
    
    // Public method to check current grappling dash state
    public bool GetCanDashWhileGrappling()
    {
        return canDashWhileGrappling;
    }
    
    // Public method for upgrades to modify dash cooldown
    public void SetDashCooldown(float cooldown)
    {
        dashCooldown = Mathf.Clamp(cooldown, 0.1f, 5f);
    }
    
    // Public method to get current dash cooldown
    public float GetDashCooldown()
    {
        return dashCooldown;
    }
    
    // Public method for upgrades to modify dash force
    public void SetDashForce(float force)
    {
        dashForce = Mathf.Clamp(force, 0.1f, 50f);
    }
    
    // Public method to get current dash force
    public float GetDashForce()
    {
        return dashForce;
    }
    
    void OnDestroy()
    {
        // Clean up pulse effect if script is destroyed
        if (currentPulse != null)
        {
            Destroy(currentPulse);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw dash direction
        Gizmos.color = Color.blue;
        Vector2 direction = dashTowardMouse ? Vector2.right : fixedDashDirection.normalized;
        Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction * 2f);
    }
}
