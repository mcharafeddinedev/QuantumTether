using UnityEngine;

/// <summary>
/// Enhanced collectible coin with effects and scoring
/// This is the improved version that actually works well
/// </summary>
public class EnhancedCollectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField, Tooltip("Points awarded when collected")]
    private int points = 500;
    
    [SerializeField, Tooltip("Collection radius")]
    private float collectionRadius = 1f;
    
    [SerializeField, Tooltip("Rotation speed (degrees per second)")]
    private float rotationSpeed = 90f;
    
    [SerializeField, Tooltip("Bobbing amplitude")]
    private float bobAmplitude = 0.5f;
    
    [SerializeField, Tooltip("Bobbing frequency")]
    private float bobFrequency = 2f;
    
    [Header("Visual Effects")]
    [SerializeField, Tooltip("Scale when collected (relative to original size)")]
    private float collectScale = 1.02f;
    
    private Vector3 originalScale;
    
    [SerializeField, Tooltip("Collection animation duration")]
    private float collectDuration = 0.3f;
    
    [SerializeField, Tooltip("Fade out duration")]
    private float fadeDuration = 0.7f; // Total time = 1 second
    
    [Header("Audio")]
    [SerializeField, Tooltip("Collection sound")]
    private AudioClip collectSound;
    
    [SerializeField, Tooltip("Audio source")]
    private AudioSource audioSource;
    
    [SerializeField, Tooltip("Collect sound volume multiplier")]
    private float collectVolume = 1f;
    
    // Static reference to the collect sound for all collectibles
    private static AudioClip globalCollectSound;
    
    [Header("Debug")]
    [SerializeField, Tooltip("Show collection radius")]
    private bool showGizmos = true;
    
    private Vector3 startPosition;
    private bool isCollected = false;
    private float collectTimer = 0f;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    
    void Start()
    {
        // Store original scale for collection animation
        originalScale = transform.localScale;
        
        startPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Ensure AudioSource exists
        EnsureAudioSource();
        
        // Set up the collect sound if not already set
        SetupCollectSound();
    }
    
    /// <summary>
    /// Set the global collect sound for all collectibles
    /// </summary>
    public static void SetGlobalCollectSound(AudioClip sound)
    {
        globalCollectSound = sound;
        Debug.Log($"[EnhancedCollectible] Global collect sound set to: {(sound != null ? sound.name : "null")}");
    }
    
    /// <summary>
    /// Set up the collect sound for this collectible
    /// </summary>
    private void SetupCollectSound()
    {
        // Use the instance sound if available, otherwise use the global sound
        if (collectSound == null && globalCollectSound != null)
        {
            collectSound = globalCollectSound;
            Debug.Log($"[EnhancedCollectible] Using global collect sound: {collectSound.name}");
        }
        else if (collectSound != null)
        {
            // Set the global sound if this instance has one and global doesn't
            if (globalCollectSound == null)
            {
                globalCollectSound = collectSound;
                Debug.Log($"[EnhancedCollectible] Set global collect sound to: {collectSound.name}");
            }
        }
    }
    
    void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.Log("[EnhancedCollectible] No AudioSource found on " + gameObject.name + "! Adding one automatically...");
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                Debug.Log("[EnhancedCollectible] AudioSource added successfully!");
            }
            else
            {
                Debug.Log("[EnhancedCollectible] AudioSource found and ready.");
            }
        }
    }
    
    void Update()
    {
        if (isCollected)
        {
            HandleCollectionAnimation();
            return;
        }
        
        // Rotate the coin
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Bob up and down
        float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = startPosition + Vector3.up * bobOffset;
        
        // Check for player collection
        if (!isCollected) // Only check if not already collected
        {
            Collider2D player = Physics2D.OverlapCircle(transform.position, collectionRadius);
            if (player != null && player.CompareTag("Player"))
            {
                Collect();
            }
        }
    }
    
    void Collect()
    {
        if (isCollected) 
        {
            Debug.LogWarning("[EnhancedCollectible] Attempted to collect already collected item!");
            return;
        }
        
        Debug.Log("[EnhancedCollectible] Collecting item...");
        isCollected = true;
        collectTimer = 0f;
        
        // Play collection sound
        EnsureAudioSource();
        if (audioSource && collectSound)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            float finalVolume = sfxVolume * collectVolume;
            Debug.Log($"[EnhancedCollectible] Playing collect sound: {collectSound.name}, Volume: {finalVolume:F2} (SFX: {sfxVolume:F2} * Collect: {collectVolume:F2})");
            audioSource.PlayOneShot(collectSound, finalVolume);
        }
        else
        {
            if (audioSource == null)
                Debug.LogWarning("[EnhancedCollectible] AudioSource is still null after ensuring! Cannot play collect sound.");
            if (collectSound == null)
                Debug.Log("[EnhancedCollectible] Collect sound is null - this is optional, continuing without sound.");
        }
        
        // Calculate final points with multiplier from spawner
        int finalPoints = GetFinalPoints();
        
        // Notify score system
        if (EnhancedScore.Instance != null)
        {
            EnhancedScore.Instance.AddPoints(finalPoints);
        }
        else
        {
            // Fallback: find EnhancedScore directly
            EnhancedScore scoreSystem = FindFirstObjectByType<EnhancedScore>();
            if (scoreSystem != null)
            {
                scoreSystem.AddPoints(finalPoints);
            }
        }
        
    }
    
    int GetFinalPoints()
    {
        // Get multiplier from spawner system
        EnhancedSpawner spawner = FindFirstObjectByType<EnhancedSpawner>();
        if (spawner != null)
        {
            float multiplier = spawner.GetCollectibleValueMultiplier();
            return Mathf.RoundToInt(points * multiplier);
        }
        
        // Fallback to base points
        return points;
    }
    
    void HandleCollectionAnimation()
    {
        if (this == null || gameObject == null) return; // Safety check
        
        collectTimer += Time.deltaTime;
        
        if (collectTimer < collectDuration)
        {
            // Scale up animation (relative to original scale)
            float scale = Mathf.Lerp(1f, collectScale, collectTimer / collectDuration);
            transform.localScale = originalScale * scale;
        }
        else if (collectTimer < collectDuration + fadeDuration)
        {
            // Fade out animation
            float fadeProgress = (collectTimer - collectDuration) / fadeDuration;
            if (spriteRenderer != null)
            {
                Color color = originalColor;
                color.a = Mathf.Lerp(1f, 0f, fadeProgress);
                spriteRenderer.color = color;
            }
        }
        else
        {
            // Animation complete, destroy the object
            Debug.Log("[EnhancedCollectible] Collection animation complete, destroying object");
            Destroy(gameObject);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (showGizmos)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, collectionRadius);
        }
    }
    
    void OnDrawGizmos()
    {
        if (showGizmos && !isCollected)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, collectionRadius);
        }
    }
    
    /// <summary>
    /// Reset the collectible state (used before destruction)
    /// </summary>
    public void ResetCollectible()
    {
        isCollected = false;
        collectTimer = 0f;
        
        // Reset visual properties
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
        transform.localScale = originalScale;
        
        // Ensure AudioSource and collect sound are set up
        EnsureAudioSource();
        SetupCollectSound();
        
        Debug.Log("[EnhancedCollectible] Collectible reset for reuse");
    }
}
