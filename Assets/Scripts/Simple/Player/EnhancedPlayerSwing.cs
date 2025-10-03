using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced player swing/grapple system with upgrade support
/// This is the improved version that actually works well
/// </summary>
public class EnhancedPlayerSwing : MonoBehaviour
{
    [Header("Grapple Settings")]
    [SerializeField] private float maxRopeLength = 8f;
    [SerializeField] private LayerMask grappleLayer = 1;
    [SerializeField] private float minGrappleDistance = 1f;
    [SerializeField] private bool autoContractOnConnect = true;
    [SerializeField] private float autoContractAmount = 0.2f;
    [SerializeField] private float contractSpeed = 2f;
    
    [Header("Physics Settings")]
    [SerializeField] private bool enableCollisionWhileGrappling = true;
    [SerializeField] private bool keepPlayerUpright = true;
    [SerializeField] private Vector2 grappleLineOffset = new Vector2(0.3f, 0f);
    [SerializeField] private float anchorBounceForce = 5f;
    [SerializeField] private float collisionCheckInterval = 0.1f;
    [SerializeField] private float minBounceSpeed = 2f;
    
    [Header("Secondary Grapple")]
    [SerializeField] private bool enableSecondaryGrapple = false;
    [SerializeField] private float secondaryGrappleMaxDistance = 8f;
    [SerializeField] private float secondaryAutoContractAmount = 0.3f;
    [SerializeField] private float primaryGrappleCooldown = 0.1f;
    [SerializeField] private float secondaryGrappleCooldown = 0.1f;
    
    [Header("Auto-Disconnect")]
    [SerializeField] private float autoDisconnectTime = 3f;
    [SerializeField] private float warningFlashDuration = 0.75f;
    [SerializeField] private Color warningColor = Color.red;
    
    [Header("Visual")]
    [SerializeField] private LineRenderer rope;
    [SerializeField] private GameObject secondaryRopeObject;
    [SerializeField] private Color ropeColor = Color.white;
    [SerializeField] private Color secondaryRopeColor = Color.cyan;
    [SerializeField] private float ropeWidth = 0.1f;
    [SerializeField] private float secondaryRopeWidth = 0.1f;
    [SerializeField] private bool showGrappleRay = true;
    
    [Header("Feedback")]
    [SerializeField] private GameObject tooFarPopupPrefab;
    [SerializeField] private float popupDuration = 0.5f;
    [SerializeField] private Vector3 popupOffset = Vector3.up * 1f;
    
    [Header("Pulse Effect")]
    [SerializeField] private GameObject pulseEffectPrefab;
    [SerializeField] private float pulseDuration = 0.3f;
    [SerializeField] private float pulseScale = 4f;
    [SerializeField] private Color inRangePulseColor = Color.blue;
    [SerializeField] private Color outOfRangePulseColor = Color.red;
    
    
    [System.NonSerialized] private GameObject currentPopup;
    [System.NonSerialized] private Camera mainCamera;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] grappleSounds;
    [SerializeField] private AudioClip releaseSound;
    [SerializeField] private AudioClip ropeContractionSound;
    [SerializeField] private float grappleVolume = 1f;
    [SerializeField] private float releaseVolume = 1f;
    [SerializeField] private float contractionVolume = 1f;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitVolume = 1f;
    
    [SerializeField] private float hitSoundCooldown = 0.2f;
    
    private DistanceJoint2D joint;
    private DistanceJoint2D secondaryJoint;
    private Rigidbody2D rb;
    private AudioSource audioSource;
    private bool isGrappling = false;
    private Vector2 grapplePoint;
    private float originalDistance;
    private float targetAutoContractDistance;
    private bool hasAutoContracted = false;
    private float grappleStartTime;
    private bool isWarning = false;
    private Color originalRopeColor;
    private Color originalSecondaryRopeColor;
    private LineRenderer secondaryRope; // Will be the LineRenderer on secondaryRopeObject
    private float lastCollisionCheckTime;
    
    // Secondary grapple state
    private bool isSecondaryGrappling = false;
    private Vector2 secondaryGrapplePoint;
    private float secondaryOriginalDistance;
    private float secondaryTargetAutoContractDistance;
    private bool hasSecondaryAutoContracted = false;
    private float secondaryGrappleStartTime;
    private bool isSecondaryWarning = false;
    
    // Cooldown timers
    private float lastPrimaryGrappleTime;
    private float lastSecondaryGrappleTime;
    
    // Rope contraction audio state
    private bool isPlayingContractionSound = false;
    private Coroutine contractionSoundCoroutine;
    private float lastHitSoundTime = 0f;
    
    // Public property for other systems to check grappling state
    public bool IsGrappling => isGrappling;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        joint = GetComponent<DistanceJoint2D>();
        mainCamera = Camera.main;
        
        // Freeze rotation to keep player upright
        if (rb != null && keepPlayerUpright)
        {
            rb.freezeRotation = true;
        }
        
        // Ensure AudioSource exists
        EnsureAudioSource();
        
        // Configure the primary joint for better collision behavior
        joint.enabled = false;
        joint.autoConfigureDistance = false;
        joint.autoConfigureConnectedAnchor = false;
        // Note: Unity 6 DistanceJoint2D API changed, so I'm using basic configuration
        
        // Add and configure secondary joint if secondary grapple is enabled
        if (enableSecondaryGrapple)
        {
            secondaryJoint = gameObject.AddComponent<DistanceJoint2D>();
            secondaryJoint.enabled = false;
            secondaryJoint.autoConfigureDistance = false;
            secondaryJoint.autoConfigureConnectedAnchor = false;
        }
        
        // Setup rope visual
        if (rope != null)
        {
            rope.material = new Material(Shader.Find("Sprites/Default"));
            rope.startColor = ropeColor;
            rope.endColor = ropeColor;
            rope.startWidth = ropeWidth;
            rope.endWidth = ropeWidth;
            originalRopeColor = ropeColor;
        }
        
        // Setup secondary rope visual
        if (enableSecondaryGrapple)
        {
            // Create secondary rope GameObject if it doesn't exist
            if (secondaryRopeObject == null)
            {
                secondaryRopeObject = new GameObject("SecondaryRope");
                secondaryRopeObject.transform.SetParent(transform);
                secondaryRopeObject.transform.localPosition = Vector3.zero;
            }
            
            // Get or add LineRenderer to secondary rope object
            secondaryRope = secondaryRopeObject.GetComponent<LineRenderer>();
            if (secondaryRope == null)
            {
                secondaryRope = secondaryRopeObject.AddComponent<LineRenderer>();
            }
            
            // Configure secondary rope
            secondaryRope.material = new Material(Shader.Find("Sprites/Default"));
            secondaryRope.startColor = secondaryRopeColor;
            secondaryRope.endColor = secondaryRopeColor;
            secondaryRope.startWidth = secondaryRopeWidth;
            secondaryRope.endWidth = secondaryRopeWidth;
            originalSecondaryRopeColor = secondaryRopeColor;
        }
    }
    
    void Update()
    {
        
        // Don't allow grappling if player is dead
        if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
        {
            return;
        }
        
        // Primary grapple input (left mouse button) - Hold to maintain, release to disconnect
        if (Input.GetMouseButtonDown(0) && Time.time - lastPrimaryGrappleTime >= primaryGrappleCooldown)
        {
            if (!isGrappling && !isSecondaryGrappling)
            {
                // Start primary grapple
                TryGrapple();
                lastPrimaryGrappleTime = Time.time;
            }
            else if (enableSecondaryGrapple && isSecondaryGrappling && !isGrappling)
            {
                // Add primary grapple while holding secondary (only if secondary grapple is enabled)
                TryGrapple();
                lastPrimaryGrappleTime = Time.time;
            }
        }
        
        // Secondary grapple input (right mouse button) - Only works if secondary grapple is enabled
        if (Input.GetMouseButtonDown(1) && enableSecondaryGrapple && Time.time - lastSecondaryGrappleTime >= secondaryGrappleCooldown)
        {
            if (!isGrappling && !isSecondaryGrappling)
            {
                // Start secondary grapple
                TrySecondaryGrapple();
                lastSecondaryGrappleTime = Time.time;
            }
            else if (isGrappling && !isSecondaryGrappling)
            {
                // Add secondary grapple while holding primary
                TrySecondaryGrapple();
                lastSecondaryGrappleTime = Time.time;
            }
        }
        
        // Release primary grapple when left mouse is released
        if (Input.GetMouseButtonUp(0) && isGrappling)
        {
            ReleaseGrapple();
        }
        
        // Release secondary grapple when right mouse is released
        if (Input.GetMouseButtonUp(1) && isSecondaryGrappling)
        {
            ReleaseSecondaryGrapple();
        }
        
        // Auto-contract a bit on first connection for yoink effect
        if (isGrappling && autoContractOnConnect && !hasAutoContracted)
        {
            AutoContractOnConnect();
        }
        
        // Auto-contract for secondary grapple
        if (isSecondaryGrappling && autoContractOnConnect && !hasSecondaryAutoContracted)
        {
            AutoContractSecondaryOnConnect();
        }
        
        // Manual contract when spacebar is held
        if (Input.GetKey(KeyCode.Space))
        {
            if (isGrappling)
            {
                ContractRope();
                StartRopeContractionSound();
            }
            if (isSecondaryGrappling)
            {
                ContractSecondaryRope();
                StartRopeContractionSound();
            }
        }
        else
        {
            // Stop contraction sound when spacebar is released
            StopRopeContractionSound();
        }
        
        // Check for auto-disconnect
        if (isGrappling)
        {
            CheckAutoDisconnect();
        }
        
        // Check for secondary grapple auto-disconnect
        if (isSecondaryGrappling)
        {
            CheckSecondaryAutoDisconnect();
        }
        
        // Update rope visual
        UpdateRopeVisual();
        
        // Keep player upright if enabled
        if (keepPlayerUpright && rb != null)
        {
            // Reset rotation if it's not upright
            if (Mathf.Abs(transform.eulerAngles.z) > 0.1f && Mathf.Abs(transform.eulerAngles.z - 360f) > 0.1f)
            {
                transform.rotation = Quaternion.identity;
                rb.angularVelocity = 0f;
            }
        }
        
        // Adjust joint settings for better collision behavior
        if (isGrappling && enableCollisionWhileGrappling)
        {
            AdjustJointForCollision();
            
            // Proactively check for collisions while grappling
            if (Time.time - lastCollisionCheckTime >= collisionCheckInterval)
            {
                CheckForCollisionsWhileGrappling();
                lastCollisionCheckTime = Time.time;
            }
        }
    }
    
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
        bool isClickingOnAnchor = IsClickingOnAnchorPoint(mousePos);
        
        Debug.Log($"[EnhancedPlayerSwing] Grapple attempt - Distance: {distance:F1}, InRange: {isInRange}, OnAnchor: {isClickingOnAnchor}");
        
        // Always show pulse effect with appropriate color
        CreatePulseEffect(mousePos, isInRange);
        
        // Only show text if clicking on an anchor point AND it's out of range
        if (!isInRange && isClickingOnAnchor)
        {
            if (distance < minGrappleDistance) 
            {
                ShowTooFarPopup(mousePos, "Too close!");
            }
            else if (distance > maxRopeLength) 
            {
                ShowTooFarPopup(mousePos, "TOO FAR AWAY!");
            }
            return;
        }
        
        // If in range, try to grapple normally
        if (isInRange)
        {
            // Continue with normal grapple logic
        }
        else
        {
            // Out of range but not clicking on anchor - no text, just return
            return;
        }
        
        // Use the original direction and distance for raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, grappleLayer);
        
        if (hit.collider != null)
        {
            grapplePoint = hit.point;
            joint.connectedAnchor = grapplePoint;
            joint.distance = Vector2.Distance(transform.position, grapplePoint);
            joint.enabled = true;
            isGrappling = true;
            originalDistance = joint.distance;
            targetAutoContractDistance = originalDistance * (1f - autoContractAmount); // Calculate target distance
            hasAutoContracted = false; // Reset auto-contract flag
            grappleStartTime = Time.time; // Start timer for auto-disconnect
            isWarning = false; // Reset warning state
            
            Debug.Log($"[EnhancedPlayerSwing] Grapple connected to {hit.collider.name} at distance {joint.distance:F1}");
            
            // Play random grapple sound
            PlayRandomGrappleSound();
            
        }
        else
        {
            // No valid target - pulse already shown above
            // Only show text if clicking on an anchor point AND it's out of range
            if (!isInRange && isClickingOnAnchor)
            {
                ShowTooFarPopup(mousePos, "No Target!");
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
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
            }
            else
            {
            }
        }
    }
    
    void PlayRandomGrappleSound()
    {
        // Ensure AudioSource exists before playing
        EnsureAudioSource();
        
        if (audioSource == null || grappleSounds == null || grappleSounds.Length == 0)
            return;
        
        AudioClip randomGrappleSound = grappleSounds[Random.Range(0, grappleSounds.Length)];
        if (randomGrappleSound == null)
            return;
        
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        float finalVolume = sfxVolume * grappleVolume;
        
        audioSource.PlayOneShot(randomGrappleSound, finalVolume);
    }
    
    void StartRopeContractionSound()
    {
        if (isPlayingContractionSound || ropeContractionSound == null) return;
        
        EnsureAudioSource();
        if (audioSource == null) return;
        
        isPlayingContractionSound = true;
        contractionSoundCoroutine = StartCoroutine(PlayContractionSoundLoop());
        
    }
    
    void StopRopeContractionSound()
    {
        if (!isPlayingContractionSound) return;
        
        isPlayingContractionSound = false;
        
        if (contractionSoundCoroutine != null)
        {
            StopCoroutine(contractionSoundCoroutine);
            contractionSoundCoroutine = null;
        }
        
    }
    
    System.Collections.IEnumerator PlayContractionSoundLoop()
    {
        while (isPlayingContractionSound && ropeContractionSound != null)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            float finalVolume = sfxVolume * contractionVolume;
            
            audioSource.PlayOneShot(ropeContractionSound, finalVolume);
            
            // Wait for the sound to finish before playing again
            yield return new WaitForSeconds(ropeContractionSound.length * 0.8f); // Slight overlap for continuous effect
        }
    }
    
    void ReleaseGrapple()
    {
        if (!isGrappling) return;
        
        Debug.Log("[EnhancedPlayerSwing] Grapple released");
        joint.enabled = false;
        isGrappling = false;
        
        // Play release sound
        EnsureAudioSource();
        
        if (audioSource && releaseSound)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            float finalVolume = sfxVolume * releaseVolume;
            audioSource.PlayOneShot(releaseSound, finalVolume);
        }
        
    }
    
    void TrySecondaryGrapple()
    {
        if (mainCamera == null || !enableSecondaryGrapple) return;
        
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float distance = Vector2.Distance(transform.position, mousePos);
        
        // Use the same distance validation as primary grapple
        bool isInRange = (distance >= minGrappleDistance && distance <= secondaryGrappleMaxDistance);
        bool isClickingOnAnchor = IsClickingOnAnchorPoint(mousePos);
        
        // Always show pulse effect with appropriate color
        CreatePulseEffect(mousePos, isInRange);
        
        // Only show text if clicking on an anchor point AND it's out of range
        if (!isInRange && isClickingOnAnchor)
        {
            if (distance < minGrappleDistance) 
            {
                ShowTooFarPopup(mousePos, "Too close!");
            }
            else if (distance > secondaryGrappleMaxDistance) 
            {
                ShowTooFarPopup(mousePos, "TOO FAR AWAY!");
            }
            return;
        }
        
        // If in range and clicking on anchor, create secondary grapple
        if (isInRange && isClickingOnAnchor)
        {
            // Use raycast to find exact hit point like primary grapple
            Vector2 direction = mousePos - (Vector2)transform.position;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distance, grappleLayer);
            
            if (hit.collider != null)
            {
                CreateSecondaryGrapple(hit.point);
            }
            // No valid target - just return without popup
        }
        else if (!isInRange)
        {
            // Out of range but not clicking on anchor - no text, just return
            return;
        }
    }
    
    void CreateSecondaryGrapple(Vector2 targetPoint)
    {
        if (secondaryJoint == null) return;
        
        isSecondaryGrappling = true;
        secondaryGrapplePoint = targetPoint;
        secondaryGrappleStartTime = Time.time;
        isSecondaryWarning = false; // Reset warning state
        
        // Show secondary rope object
        if (secondaryRopeObject != null)
        {
            secondaryRopeObject.SetActive(true);
        }
        
        // Configure secondary joint
        secondaryJoint.enabled = true;
        secondaryJoint.connectedAnchor = targetPoint;
        secondaryJoint.distance = Vector2.Distance(transform.position, targetPoint);
        secondaryOriginalDistance = secondaryJoint.distance;
        
        // Auto-contract setup
        secondaryTargetAutoContractDistance = secondaryOriginalDistance * (1f - secondaryAutoContractAmount);
        hasSecondaryAutoContracted = false;
        
        // Play random grapple sound
        PlayRandomGrappleSound();
        
    }
    
    void ContractSecondaryRope()
    {
        if (secondaryJoint != null && secondaryJoint.enabled)
        {
            secondaryJoint.distance = Mathf.MoveTowards(secondaryJoint.distance, 0f, contractSpeed * Time.deltaTime);
        }
    }
    
    void ReleaseSecondaryGrapple()
    {
        if (isSecondaryGrappling)
        {
            isSecondaryGrappling = false;
            if (secondaryJoint != null)
            {
                secondaryJoint.enabled = false;
            }
            hasSecondaryAutoContracted = false;
            
            // Reset secondary rope visual
            if (secondaryRope != null)
            {
                secondaryRope.positionCount = 0;
            }
            
            // Hide secondary rope object if not grappling
            if (secondaryRopeObject != null)
            {
                secondaryRopeObject.SetActive(false);
            }
            
            // Play release sound
            if (audioSource != null && releaseSound != null)
            {
                float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
                audioSource.PlayOneShot(releaseSound, sfxVolume);
            }
            
        }
    }
    
    /// <summary>
    /// Temporarily disable the joint to allow for better collision detection
    /// </summary>
    public void TemporarilyDisableJoint(float duration = 0.1f)
    {
        if (isGrappling)
        {
            StartCoroutine(TemporarilyDisableJointCoroutine(duration));
        }
    }
    
    IEnumerator TemporarilyDisableJointCoroutine(float duration)
    {
        bool wasEnabled = joint.enabled;
        joint.enabled = false;
        
        yield return new WaitForSeconds(duration);
        
        if (isGrappling && wasEnabled)
        {
            joint.enabled = true;
        }
    }
    
    /// <summary>
    /// Adjust joint settings to allow better collision detection while grappling
    /// </summary>
    void AdjustJointForCollision()
    {
        if (joint == null || !isGrappling) return;
        
        // Unity 6 DistanceJoint2D API has changed - using basic collision handling
        // The joint will use its default physics properties for collision detection
        float currentSpeed = rb.linearVelocity.magnitude;
        
    }
    
    /// <summary>
    /// Proactively check for collisions while grappling using OverlapCircle
    /// </summary>
    void CheckForCollisionsWhileGrappling()
    {
        if (!isGrappling || !enableCollisionWhileGrappling) return;
        
        // Use OverlapCircle to detect nearby objects
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);
        
        foreach (Collider2D col in colliders)
        {
            // Check for death objects by tag or name (hazard anchors, ground, etc.)
            if (col.CompareTag("Death") || col.name.ToLower().Contains("death"))
            {
                EnhancedGameManager.Instance?.Die();
                return;
            }
            
            // Check for anchor points (but be more selective during contraction)
            if (col.name.Contains("Anchor") || col.name.Contains("anchor"))
            {
                // Only apply bounce if we're moving fast enough (not just slowly contracting)
                float currentSpeed = rb.linearVelocity.magnitude;
                if (currentSpeed > minBounceSpeed) // Only bounce if moving with some speed
                {
                    // Temporarily disable joint to allow proper collision
                    TemporarilyDisableJoint(0.2f);
                    
                    // Add bounce effect
                    Vector2 bounceDirection = (transform.position - col.transform.position).normalized;
                    rb.linearVelocity = bounceDirection * anchorBounceForce;
                    
                }
            }
        }
    }
    
    void ContractRope()
    {
        if (joint.enabled)
        {
            joint.distance = Mathf.MoveTowards(joint.distance, 0f, contractSpeed * Time.deltaTime);
        }
    }
    
    void AutoContractOnConnect()
    {
        if (joint.enabled)
        {
            // Contract a bit quickly for yoink effect
            joint.distance = Mathf.MoveTowards(joint.distance, targetAutoContractDistance, contractSpeed * 4f * Time.deltaTime);
            
            // Mark as auto-contracted when we reach target
            if (joint.distance <= targetAutoContractDistance)
            {
                hasAutoContracted = true;
            }
        }
    }
    
    void AutoContractSecondaryOnConnect()
    {
        if (secondaryJoint != null && secondaryJoint.enabled)
        {
            // Contract a bit quickly for yoink effect
            secondaryJoint.distance = Mathf.MoveTowards(secondaryJoint.distance, secondaryTargetAutoContractDistance, contractSpeed * 4f * Time.deltaTime);
            
            // Mark as auto-contracted when we reach target
            if (secondaryJoint.distance <= secondaryTargetAutoContractDistance)
            {
                hasSecondaryAutoContracted = true;
            }
        }
    }
    
    void CheckAutoDisconnect()
    {
        float grappleTime = Time.time - grappleStartTime;
        float timeUntilDisconnect = autoDisconnectTime - grappleTime;
        
        // Start warning flash
        if (timeUntilDisconnect <= warningFlashDuration && !isWarning)
        {
            isWarning = true;
        }
        
        // Auto-disconnect
        if (grappleTime >= autoDisconnectTime)
        {
            ReleaseGrapple();
        }
    }
    
    void CheckSecondaryAutoDisconnect()
    {
        float grappleTime = Time.time - secondaryGrappleStartTime;
        float timeUntilDisconnect = autoDisconnectTime - grappleTime;
        
        // Start warning flash
        if (timeUntilDisconnect <= warningFlashDuration && !isSecondaryWarning)
        {
            isSecondaryWarning = true;
        }
        
        // Auto-disconnect
        if (grappleTime >= autoDisconnectTime)
        {
            ReleaseSecondaryGrapple();
        }
    }
    
    void UpdateRopeVisual()
    {
        // Update primary rope
        if (isGrappling && rope != null)
        {
            // Ensure the rope has 2 positions
            if (rope.positionCount != 2)
            {
                rope.positionCount = 2;
            }
            
            // Apply offset to rope start position
            Vector2 ropeStartPosition = (Vector2)transform.position + grappleLineOffset;
            rope.SetPosition(0, ropeStartPosition);
            rope.SetPosition(1, grapplePoint);
            
            // Handle warning flash
            if (isWarning)
            {
                float flashSpeed = 10f; // How fast to flash
                float flashValue = Mathf.PingPong(Time.time * flashSpeed, 1f);
                Color currentColor = Color.Lerp(originalRopeColor, warningColor, flashValue);
                rope.startColor = currentColor;
                rope.endColor = currentColor;
            }
            else
            {
                rope.startColor = originalRopeColor;
                rope.endColor = originalRopeColor;
            }
        }
        else if (rope != null)
        {
            rope.positionCount = 0;
        }
        
        // Update secondary rope
        if (isSecondaryGrappling && secondaryRope != null)
        {
            // Ensure the secondary rope has 2 positions
            if (secondaryRope.positionCount != 2)
            {
                secondaryRope.positionCount = 2;
            }
            
            // Apply offset to secondary rope start position
            Vector2 secondaryRopeStartPosition = (Vector2)transform.position + grappleLineOffset;
            secondaryRope.SetPosition(0, secondaryRopeStartPosition);
            secondaryRope.SetPosition(1, secondaryGrapplePoint);
            
            // Handle warning flash for secondary rope
            if (isSecondaryWarning)
            {
                float flashSpeed = 10f; // How fast to flash
                float flashValue = Mathf.PingPong(Time.time * flashSpeed, 1f);
                Color currentColor = Color.Lerp(originalSecondaryRopeColor, warningColor, flashValue);
                secondaryRope.startColor = currentColor;
                secondaryRope.endColor = currentColor;
            }
            else
            {
                secondaryRope.startColor = originalSecondaryRopeColor;
                secondaryRope.endColor = originalSecondaryRopeColor;
            }
        }
        else if (secondaryRope != null)
        {
            secondaryRope.positionCount = 0;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check for death objects by tag or name
        bool isDeathObject = collision.gameObject.CompareTag("Death") || 
                           collision.gameObject.name.ToLower().Contains("death") || 
                           collision.gameObject.name.ToLower().Contains("ground") || 
                           collision.gameObject.name.ToLower().Contains("floor") || 
                           collision.gameObject.name.ToLower().Contains("spike") || 
                           collision.gameObject.name.ToLower().Contains("hazard") || 
                           collision.gameObject.name.ToLower().Contains("wall");
        
        if (isDeathObject)
        {
            // Player hit death object (ground or hazard) - death
            EnhancedGameManager.Instance?.Die();
        }
        else
        {
            // Play hit sound for non-death collisions
            // Don't play for self-collision
            if (collision.gameObject.CompareTag("Player") == false)
            {
                PlayHitSound();
            }
        }
        
        // Handle collision with anchor points while grappling
        if (isGrappling && enableCollisionWhileGrappling)
        {
            // Check if we hit an anchor point
            if (collision.gameObject.name.Contains("Anchor") || collision.gameObject.name.Contains("anchor"))
            {
                // Disable the joint for longer to allow proper collision interaction
                TemporarilyDisableJoint(0.3f);
                
                // Add some bounce/knockback effect when hitting anchor points
                Vector2 bounceDirection = (transform.position - collision.transform.position).normalized;
                rb.linearVelocity = bounceDirection * anchorBounceForce;
                
            }
        }
    }
    
    bool IsClickingOnAnchorPoint(Vector2 worldPosition)
    {
        // Check if there's an anchor point near the click position
        Collider2D[] colliders = Physics2D.OverlapCircleAll(worldPosition, 1f, grappleLayer);
        
        foreach (Collider2D col in colliders)
        {
            // Check if it's an anchor point by name (since "Anchor" tag might not exist)
            if (col.name.Contains("Anchor") || col.name.Contains("anchor"))
            {
                return true;
            }
        }
        
        return false;
    }
    
    void ShowTooFarPopup(Vector2 clickPosition, string message)
    {
        if (tooFarPopupPrefab == null)
        {
            CreateDynamicPopup(clickPosition, message);
            return;
        }
        
        // Destroy previous popup if it exists
        if (currentPopup != null)
        {
            Destroy(currentPopup);
        }
        
        // Find the Canvas to parent the popup
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }
        
        // Create popup as child of Canvas
        currentPopup = Instantiate(tooFarPopupPrefab, canvas.transform);
        
        // Ensure the popup has proper UI setup
        RectTransform popupRect = currentPopup.GetComponent<RectTransform>();
        if (popupRect != null)
        {
            // Set anchor to center for proper positioning
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            
            // Set initial position to center
            popupRect.anchoredPosition = Vector2.zero;
        }
        
        // Set the text - try different text components
        UnityEngine.UI.Text popupText = currentPopup.GetComponent<UnityEngine.UI.Text>();
        TMPro.TextMeshProUGUI popupTextMeshPro = currentPopup.GetComponent<TMPro.TextMeshProUGUI>();
        
        if (popupText != null)
        {
            popupText.text = message;
        }
        else if (popupTextMeshPro != null)
        {
            popupTextMeshPro.text = message;
        }
        else
        {
            // Try to find any text component in children
            UnityEngine.UI.Text childText = currentPopup.GetComponentInChildren<UnityEngine.UI.Text>(true);
            TMPro.TextMeshProUGUI childTextMeshPro = currentPopup.GetComponentInChildren<TMPro.TextMeshProUGUI>(true);
            
            if (childText != null)
            {
                childText.text = message;
            }
            else if (childTextMeshPro != null)
            {
                childTextMeshPro.text = message;
            }
            else
            {
            }
        }
        
        // Position the popup immediately at the click position
        PositionPopupAtClick(clickPosition);
        
        // Start coroutine to hide popup after duration
        StartCoroutine(HidePopupAfterDelay());
    }
    
    void CreateDynamicPopup(Vector2 clickPosition, string message)
    {
        
        // Find Canvas
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }
        
        
        // Create a new GameObject for the popup
        currentPopup = new GameObject("DynamicPopup");
        currentPopup.transform.SetParent(canvas.transform, false);
        
        // Add RectTransform
        RectTransform rectTransform = currentPopup.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        // Add Text component
        UnityEngine.UI.Text textComponent = currentPopup.AddComponent<UnityEngine.UI.Text>();
        textComponent.text = message;
        // Use default font settings (Resources.GetBuiltinResource is deprecated in Unity 6)
        textComponent.fontSize = 24;
        textComponent.color = Color.red;
        textComponent.alignment = TextAnchor.MiddleCenter;
        
        
        // Position the popup
        PositionPopupAtClick(clickPosition);
        
        // Start coroutine to hide popup after duration
        StartCoroutine(HidePopupAfterDelay());
        
    }
    
    void PositionPopupAtClick(Vector2 worldClickPosition)
    {
        if (currentPopup == null) return;
        
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;
        
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;
        
        RectTransform popupRect = currentPopup.GetComponent<RectTransform>();
        if (popupRect == null) return;
        
        // Use mouse position directly for UI positioning - this is resolution independent
        Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
        
        // Get canvas rect
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        Vector2 canvasSize = canvasRect.rect.size;
        
        // Convert screen coordinates to canvas local coordinates
        // Screen coordinates (0,0) = bottom-left, Canvas local (0,0) = center
        Vector2 localPos;
        localPos.x = screenPos.x - Screen.width / 2f;
        localPos.y = screenPos.y - Screen.height / 2f;
        
        // Apply the offset from inspector
        Vector2 finalPos = localPos + new Vector2(popupOffset.x, popupOffset.y);
        
        // Clamp to canvas bounds to keep it on screen
        float margin = 50f;
        finalPos.x = Mathf.Clamp(finalPos.x, -canvasSize.x/2 + margin, canvasSize.x/2 - margin);
        finalPos.y = Mathf.Clamp(finalPos.y, -canvasSize.y/2 + margin, canvasSize.y/2 - margin);
        
        // Set the final position
        popupRect.anchoredPosition = finalPos;
    }
    
    void UpdatePopupPosition()
    {
        // No longer needed - popup stays at click position
        // This method is kept for compatibility but does nothing
    }
    
    void CreatePulseEffect(Vector2 worldPosition, bool isInRange = false)
    {
        if (pulseEffectPrefab != null)
        {
            // Create pulse effect in world space
            GameObject pulse = Instantiate(pulseEffectPrefab, worldPosition, Quaternion.identity);
            
            // Start pulse animation coroutine with color
            StartCoroutine(AnimatePulse(pulse, isInRange));
            
        }
        else
        {
            // Fallback: create a simple visual effect using a sprite
            CreateSimplePulseEffect(worldPosition, isInRange);
        }
    }
    
    void CreateSimplePulseEffect(Vector2 worldPosition, bool isInRange = false)
    {
        // Create a simple circle sprite for pulse effect
        GameObject pulse = new GameObject("PulseEffect");
        pulse.transform.position = worldPosition;
        
        // Add sprite renderer
        SpriteRenderer sr = pulse.AddComponent<SpriteRenderer>();
        sr.sprite = CreateCircleSprite();
        
        // Set color based on range
        Color pulseColor = isInRange ? inRangePulseColor : outOfRangePulseColor;
        sr.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 0.8f);
        
        // Start pulse animation
        StartCoroutine(AnimatePulse(pulse, isInRange));
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
    
    IEnumerator AnimatePulse(GameObject pulse, bool isInRange = false)
    {
        if (pulse == null) yield break;
        
        Vector3 originalScale = Vector3.one;
        float elapsed = 0f;
        
        // Get the base color
        Color baseColor = isInRange ? inRangePulseColor : outOfRangePulseColor;
        
        while (elapsed < pulseDuration && pulse != null)
        {
            float progress = elapsed / pulseDuration;
            
            // Scale up and fade out
            float scale = Mathf.Lerp(0.1f, pulseScale, progress);
            float alpha = Mathf.Lerp(1f, 0f, progress);
            
            pulse.transform.localScale = originalScale * scale;
            
            // Update alpha if it has a sprite renderer
            SpriteRenderer sr = pulse.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = baseColor;
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
    }
    
    IEnumerator HidePopupAfterDelay()
    {
        yield return new WaitForSeconds(popupDuration);
        
        if (currentPopup != null)
        {
            Destroy(currentPopup);
            currentPopup = null;
        }
    }
    
    
    void OnDrawGizmos()
    {
        // Show grapple line offset position
        if (showGrappleRay)
        {
            Gizmos.color = Color.yellow;
            Vector2 offsetPosition = (Vector2)transform.position + grappleLineOffset;
            Gizmos.DrawWireSphere(offsetPosition, 0.1f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (showGrappleRay)
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (mainCamera == null) return;
            Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = mousePos - (Vector2)transform.position;
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, (Vector2)transform.position + direction.normalized * maxRopeLength);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minGrappleDistance);
        }
    }
    
    // Upgrade system methods
    public void SetMaxRopeLength(float length)
    {
        maxRopeLength = Mathf.Clamp(length, 1f, 20f);
    }
    
    public void SetSecondaryGrappleMaxDistance(float distance)
    {
        secondaryGrappleMaxDistance = Mathf.Clamp(distance, 1f, 20f);
    }
    
    public void SetEnableSecondaryGrapple(bool enable)
    {
        enableSecondaryGrapple = enable;
        
        if (enable && secondaryJoint == null)
        {
            secondaryJoint = gameObject.AddComponent<DistanceJoint2D>();
            secondaryJoint.enabled = false;
            secondaryJoint.autoConfigureDistance = false;
            secondaryJoint.autoConfigureConnectedAnchor = false;
        }
        
        if (enable && secondaryRopeObject == null)
        {
            secondaryRopeObject = new GameObject("SecondaryRope");
            secondaryRopeObject.transform.SetParent(transform);
            secondaryRopeObject.transform.localPosition = Vector3.zero;
            
            secondaryRope = secondaryRopeObject.AddComponent<LineRenderer>();
            
            // Try to find a better material for LineRenderer
            Shader lineShader = Shader.Find("Sprites/Default");
            if (lineShader == null)
            {
                lineShader = Shader.Find("Legacy Shaders/Sprites/Default");
            }
            if (lineShader == null)
            {
                lineShader = Shader.Find("Unlit/Color");
            }
            
            secondaryRope.material = new Material(lineShader);
            secondaryRope.startColor = secondaryRopeColor;
            secondaryRope.endColor = secondaryRopeColor;
            secondaryRope.startWidth = secondaryRopeWidth; // Use secondaryRopeWidth instead of ropeWidth
            secondaryRope.endWidth = secondaryRopeWidth;   // Use secondaryRopeWidth instead of ropeWidth
            secondaryRope.positionCount = 0;
            secondaryRope.sortingOrder = 10; // Higher sorting order to ensure visibility
            secondaryRope.useWorldSpace = true; // Use world space for proper positioning
            secondaryRope.enabled = true; // Ensure it's enabled
            
        }
        
    }
    
    /// <summary>
    /// Set whether the player should stay upright (freeze rotation)
    /// </summary>
    public void SetKeepPlayerUpright(bool keepUpright)
    {
        keepPlayerUpright = keepUpright;
        
        if (rb != null)
        {
            rb.freezeRotation = keepUpright;
        }
        
    }
    
    /// <summary>
    /// Reset player rotation to upright (0 degrees)
    /// </summary>
    public void ResetPlayerRotation()
    {
        if (rb != null)
        {
            rb.angularVelocity = 0f;
            transform.rotation = Quaternion.identity;
        }
        
    }
    
    /// <summary>
    /// Play hit sound when player collides with objects
    /// </summary>
    public void PlayHitSound()
    {
        // Check cooldown to prevent sound spam
        if (Time.time - lastHitSoundTime < hitSoundCooldown)
        {
            return;
        }
        
        EnsureAudioSource();
        if (audioSource && hitSound)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            float finalVolume = sfxVolume * hitVolume;
            audioSource.PlayOneShot(hitSound, finalVolume);
            lastHitSoundTime = Time.time;
        }
    }
    
    /// <summary>
    /// Set the grapple line offset to avoid coming from the character's face
    /// </summary>
    public void SetGrappleLineOffset(Vector2 offset)
    {
        grappleLineOffset = offset;
    }
    
    public float GetMaxRopeLength()
    {
        return maxRopeLength;
    }
    
    public float GetSecondaryGrappleMaxDistance()
    {
        return secondaryGrappleMaxDistance;
    }
}
