using UnityEngine;

/// <summary>
/// Service that applies upgrade effects to existing game systems - basically the upgrade effect handler
/// </summary>
public class UpgradeApplier : MonoBehaviour
{
    [Header("System References")]
    [SerializeField, Tooltip("Player dash system")]
    private EnhancedPlayerDash playerDash;
    
    [SerializeField, Tooltip("Player swing/grapple system")]
    private EnhancedPlayerSwing playerSwing;
    
    [SerializeField, Tooltip("Camera system")]
    private EnhancedCamera cameraSystem;
    
    [SerializeField, Tooltip("Score system")]
    private EnhancedScore scoreSystem;
    
    [SerializeField, Tooltip("Spawner system")]
    private EnhancedSpawner spawnerSystem;
    
    [Header("Base Values (for clamping)")]
    [SerializeField, Tooltip("Minimum dash cooldown")]
    private float minDashCooldown = 0.1f;
    
    [SerializeField, Tooltip("Maximum dash cooldown")]
    private float maxDashCooldown = 5f;
    
    [SerializeField, Tooltip("Minimum grapple range")]
    private float minGrappleRange = 1f;
    
    [SerializeField, Tooltip("Maximum grapple range")]
    private float maxGrappleRange = 20f;
    
    [SerializeField, Tooltip("Minimum camera speed multiplier")]
    private float minCameraSpeedMultiplier = 0.3f;
    
    [SerializeField, Tooltip("Maximum camera speed multiplier")]
    private float maxCameraSpeedMultiplier = 2f;
    
    [SerializeField, Tooltip("Minimum camera size multiplier")]
    private float minCameraSizeMultiplier = 0.5f;
    
    [SerializeField, Tooltip("Maximum camera size multiplier")]
    private float maxCameraSizeMultiplier = 2f;
    
    [SerializeField, Tooltip("Minimum score rate multiplier")]
    private float minScoreRateMultiplier = 0.5f;
    
    [SerializeField, Tooltip("Maximum score rate multiplier")]
    private float maxScoreRateMultiplier = 3f;
    
    // Track current multipliers for stacking
    private float currentCameraSpeedMultiplier = 1f;
    private float currentCameraSizeMultiplier = 1f;
    private float currentScoreRateMultiplier = 1f;
    private float currentHazardSpawnRateMultiplier = 1f;
    
    void Start()
    {
        // Auto-find systems if not assigned
        if (playerDash == null)
            playerDash = FindFirstObjectByType<EnhancedPlayerDash>();
        if (playerSwing == null)
            playerSwing = FindFirstObjectByType<EnhancedPlayerSwing>();
        if (cameraSystem == null)
            cameraSystem = FindFirstObjectByType<EnhancedCamera>();
        if (scoreSystem == null)
            scoreSystem = FindFirstObjectByType<EnhancedScore>();
        if (spawnerSystem == null)
            spawnerSystem = FindFirstObjectByType<EnhancedSpawner>();
        
        // Debug: Log which systems were found
        Debug.Log($"[UpgradeApplier] Systems found - Dash: {playerDash != null}, Swing: {playerSwing != null}, Camera: {cameraSystem != null}, Score: {scoreSystem != null}, Spawner: {spawnerSystem != null}");
        
        // Additional debug: Log the actual GameObject names if found
        if (playerDash != null) Debug.Log($"[UpgradeApplier] PlayerDash found: {playerDash.gameObject.name}");
        if (playerSwing != null) Debug.Log($"[UpgradeApplier] PlayerSwing found: {playerSwing.gameObject.name}");
        if (cameraSystem != null) Debug.Log($"[UpgradeApplier] CameraSystem found: {cameraSystem.gameObject.name}");
        if (scoreSystem != null) Debug.Log($"[UpgradeApplier] ScoreSystem found: {scoreSystem.gameObject.name}");
        if (spawnerSystem != null) Debug.Log($"[UpgradeApplier] SpawnerSystem found: {spawnerSystem.gameObject.name}");
            
        // Subscribe to game restart events so we can reset multipliers
        if (FindFirstObjectByType<EnhancedGameManager>() != null)
        {
            EnhancedGameManager.OnGameRestart += OnGameRestart;
        }
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (FindFirstObjectByType<EnhancedGameManager>() != null)
        {
            EnhancedGameManager.OnGameRestart -= OnGameRestart;
        }
    }
    
    void OnGameRestart()
    {
        // Reset all multipliers to default values
        currentCameraSpeedMultiplier = 1f;
        currentCameraSizeMultiplier = 1f;
        currentScoreRateMultiplier = 1f;
        currentHazardSpawnRateMultiplier = 1f;
        
        // Reset camera speed and size
        if (cameraSystem != null)
        {
            cameraSystem.SetSpeedMultiplier(1f);
            cameraSystem.SetSizeMultiplier(1f);
        }
        
        // Reset score rate
        if (scoreSystem != null)
        {
            scoreSystem.SetScoreRateMultiplier(1f);
        }
        
        // Reset spawner multipliers
        if (spawnerSystem != null)
        {
            spawnerSystem.SetHazardSpawnRateMultiplier(1f);
            spawnerSystem.SetCollectibleValueMultiplier(1f);
        }
    }
    
    /// <summary>
    /// Detect only the systems needed for the specific upgrade being applied
    /// </summary>
    private void DetectRequiredSystems(RunUpgrade upgrade)
    {
        // Check if we need PlayerDash system
        bool needsPlayerDash = upgrade.dashCooldownDelta != 0f || 
                              upgrade.dashForceDelta != 0f || 
                              upgrade.enableDashWhileGrappling;
        
        if (needsPlayerDash && playerDash == null)
        {
            Debug.Log("[UpgradeApplier] Detecting PlayerDash system for upgrade...");
            playerDash = FindFirstObjectByType<EnhancedPlayerDash>();
            if (playerDash != null)
            {
                Debug.Log($"[UpgradeApplier] PlayerDash found: {playerDash.gameObject.name}");
            }
            else
            {
                Debug.LogError("[UpgradeApplier] PlayerDash not found! Some upgrade effects may not work.");
            }
        }
        
        // Check if we need PlayerSwing system
        bool needsPlayerSwing = upgrade.primaryGrappleRangeDelta != 0f || 
                               upgrade.secondaryGrappleRangeDelta != 0f || 
                               upgrade.enableSecondaryGrapple;
        
        if (needsPlayerSwing && playerSwing == null)
        {
            Debug.Log("[UpgradeApplier] Detecting PlayerSwing system for upgrade...");
            playerSwing = FindFirstObjectByType<EnhancedPlayerSwing>();
            if (playerSwing != null)
            {
                Debug.Log($"[UpgradeApplier] PlayerSwing found: {playerSwing.gameObject.name}");
            }
            else
            {
                Debug.LogError("[UpgradeApplier] PlayerSwing not found! Some upgrade effects may not work.");
            }
        }
        
        // Check if we need ScoreSystem
        bool needsScoreSystem = upgrade.scoreRateMultiplier != 1f;
        
        if (needsScoreSystem && scoreSystem == null)
        {
            Debug.Log("[UpgradeApplier] Detecting ScoreSystem for upgrade...");
            scoreSystem = FindFirstObjectByType<EnhancedScore>();
            if (scoreSystem != null)
            {
                Debug.Log($"[UpgradeApplier] ScoreSystem found: {scoreSystem.gameObject.name}");
            }
            else
            {
                Debug.LogError("[UpgradeApplier] ScoreSystem not found! Some upgrade effects may not work.");
            }
        }
        
        // Check if we need CameraSystem
        bool needsCameraSystem = upgrade.cameraSpeedMultiplier != 1f || 
                                upgrade.cameraSizeMultiplier != 1f ||
                                upgrade.gracePeriodDelta != 0f || 
                                upgrade.deathDistanceDelta != 0f;
        
        if (needsCameraSystem && cameraSystem == null)
        {
            Debug.Log("[UpgradeApplier] Detecting CameraSystem for upgrade...");
            cameraSystem = FindFirstObjectByType<EnhancedCamera>();
            if (cameraSystem != null)
            {
                Debug.Log($"[UpgradeApplier] CameraSystem found: {cameraSystem.gameObject.name}");
            }
            else
            {
                Debug.LogError("[UpgradeApplier] CameraSystem not found! Some upgrade effects may not work.");
            }
        }
        
        // Check if we need SpawnerSystem
        bool needsSpawnerSystem = upgrade.hazardSpawnRateMultiplier != 1f || 
                                 upgrade.collectibleValueDelta != 0 || 
                                 upgrade.anchorClusterSpacingDelta != 0f || 
                                 upgrade.collectibleSpawnDistanceDelta != 0f || 
                                 upgrade.anchorDensityMultiplier != 1f;
        
        if (needsSpawnerSystem && spawnerSystem == null)
        {
            Debug.Log("[UpgradeApplier] Detecting SpawnerSystem for upgrade...");
            spawnerSystem = FindFirstObjectByType<EnhancedSpawner>();
            if (spawnerSystem != null)
            {
                Debug.Log($"[UpgradeApplier] SpawnerSystem found: {spawnerSystem.gameObject.name}");
            }
            else
            {
                Debug.LogError("[UpgradeApplier] SpawnerSystem not found! Some upgrade effects may not work.");
            }
        }
    }
    
    /// <summary>
    /// Apply an upgrade to the game systems
    /// </summary>
    public void ApplyUpgrade(RunUpgrade upgrade)
    {
        if (upgrade == null) 
        {
            Debug.LogError("[UpgradeApplier] Upgrade is null! Cannot apply upgrade.");
            return;
        }
        
        Debug.Log($"[UpgradeApplier] ===== APPLYING UPGRADE: {upgrade.displayName} (ID: {upgrade.id}) =====");
        Debug.Log($"[UpgradeApplier] Upgrade details - Cost: {upgrade.cost}, Max Stacks: {upgrade.maxStacks}");
        
        // Detect only the systems needed for this specific upgrade
        DetectRequiredSystems(upgrade);
        
        int effectsApplied = 0;
        
        // Apply movement effects
        if (upgrade.dashCooldownDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] MOVEMENT: Dash cooldown delta = {upgrade.dashCooldownDelta}");
            ApplyDashCooldownChange(upgrade.dashCooldownDelta);
            effectsApplied++;
        }
        
        if (upgrade.dashForceDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] MOVEMENT: Dash force delta = {upgrade.dashForceDelta}");
            ApplyDashForceChange(upgrade.dashForceDelta);
            effectsApplied++;
        }
        
        if (upgrade.enableDashWhileGrappling)
        {
            Debug.Log("[UpgradeApplier] MOVEMENT: Enabling dash while grappling");
            EnableDashWhileGrappling();
            effectsApplied++;
        }
        
        // Apply grapple effects
        if (upgrade.primaryGrappleRangeDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] GRAPPLE: Primary grapple range delta = {upgrade.primaryGrappleRangeDelta}");
            ApplyPrimaryGrappleRangeChange(upgrade.primaryGrappleRangeDelta);
            effectsApplied++;
        }
        
        if (upgrade.secondaryGrappleRangeDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] GRAPPLE: Secondary grapple range delta = {upgrade.secondaryGrappleRangeDelta}");
            ApplySecondaryGrappleRangeChange(upgrade.secondaryGrappleRangeDelta);
            effectsApplied++;
        }
        
        if (upgrade.enableSecondaryGrapple)
        {
            Debug.Log("[UpgradeApplier] GRAPPLE: Enabling secondary grapple");
            EnableSecondaryGrapple();
            effectsApplied++;
        }
        
        // Apply score effects
        if (upgrade.scoreRateMultiplier != 1f)
        {
            Debug.Log($"[UpgradeApplier] SCORE: Score rate multiplier = {upgrade.scoreRateMultiplier}");
            ApplyScoreRateMultiplier(upgrade.scoreRateMultiplier);
            effectsApplied++;
        }
        
        if (upgrade.collectibleValueDelta != 0)
        {
            Debug.Log($"[UpgradeApplier] SCORE: Collectible value delta = {upgrade.collectibleValueDelta}");
            ApplyCollectibleValueChange(upgrade.collectibleValueDelta);
            effectsApplied++;
        }
        
        // Apply camera effects
        if (upgrade.cameraSpeedMultiplier != 1f)
        {
            Debug.Log($"[UpgradeApplier] CAMERA: Camera speed multiplier = {upgrade.cameraSpeedMultiplier}");
            ApplyCameraSpeedMultiplier(upgrade.cameraSpeedMultiplier);
            effectsApplied++;
        }
        
        if (upgrade.cameraSizeMultiplier != 1f)
        {
            Debug.Log($"[UpgradeApplier] CAMERA: Camera size multiplier = {upgrade.cameraSizeMultiplier}");
            ApplyCameraSizeMultiplier(upgrade.cameraSizeMultiplier);
            effectsApplied++;
        }
        
        // Apply world effects
        if (upgrade.hazardSpawnRateMultiplier != 1f)
        {
            Debug.Log($"[UpgradeApplier] WORLD: Hazard spawn rate multiplier = {upgrade.hazardSpawnRateMultiplier}");
            ApplyHazardSpawnRateMultiplier(upgrade.hazardSpawnRateMultiplier);
            effectsApplied++;
        }
        
        if (upgrade.anchorClusterSpacingDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] WORLD: Anchor cluster spacing delta = {upgrade.anchorClusterSpacingDelta}");
            ApplyAnchorClusterSpacingChange(upgrade.anchorClusterSpacingDelta);
            effectsApplied++;
        }
        
        if (upgrade.anchorDensityMultiplier != 1f)
        {
            Debug.Log($"[UpgradeApplier] WORLD: Anchor density multiplier = {upgrade.anchorDensityMultiplier}");
            ApplyAnchorDensityMultiplier(upgrade.anchorDensityMultiplier);
            effectsApplied++;
        }
        
        if (upgrade.collectibleSpawnDistanceDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] WORLD: Collectible spawn distance delta = {upgrade.collectibleSpawnDistanceDelta}");
            ApplyCollectibleSpawnDistanceChange(upgrade.collectibleSpawnDistanceDelta);
            effectsApplied++;
        }
        
        // Apply safety effects
        if (upgrade.gracePeriodDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] SAFETY: Grace period delta = {upgrade.gracePeriodDelta}");
            ApplyGracePeriodChange(upgrade.gracePeriodDelta);
            effectsApplied++;
        }
        
        if (upgrade.deathDistanceDelta != 0f)
        {
            Debug.Log($"[UpgradeApplier] SAFETY: Death distance delta = {upgrade.deathDistanceDelta}");
            ApplyDeathDistanceChange(upgrade.deathDistanceDelta);
            effectsApplied++;
        }
        
        Debug.Log($"[UpgradeApplier] ===== UPGRADE COMPLETE: {upgrade.displayName} - {effectsApplied} effects applied =====");
    }
    
    // Movement effects
    private void ApplyDashCooldownChange(float delta)
    {
        if (playerDash == null) 
        {
            Debug.LogError("[UpgradeApplier] PlayerDash is null! Cannot apply dash cooldown change.");
            return;
        }
        
        // Get current cooldown and apply additive change
        float currentCooldown = GetDashCooldown();
        float newCooldown = Mathf.Clamp(currentCooldown + delta, minDashCooldown, maxDashCooldown);
        SetDashCooldown(newCooldown);
        
        Debug.Log($"[UpgradeApplier] Dash cooldown changed: {currentCooldown:F2} -> {newCooldown:F2} (delta: {delta:F2})");
    }
    
    private void ApplyDashForceChange(float delta)
    {
        if (playerDash == null) 
        {
            Debug.LogError("[UpgradeApplier] PlayerDash is null! Cannot apply dash force change.");
            return;
        }
        
        // Get current dash force and apply additive change
        float currentForce = GetDashForce();
        float newForce = currentForce + delta;
        
        // Clamp to reasonable range (0.1 to 50)
        newForce = Mathf.Clamp(newForce, 0.1f, 50f);
        
        SetDashForce(newForce);
        
        Debug.Log($"[UpgradeApplier] Dash force changed: {currentForce:F2} -> {newForce:F2} (delta: {delta:F2})");
    }
    
    private void EnableDashWhileGrappling()
    {
        if (playerDash == null) 
        {
            Debug.LogError("[UpgradeApplier] PlayerDash is null! Cannot enable dash while grappling.");
            return;
        }
        
        playerDash.SetCanDashWhileGrappling(true);
        Debug.Log("[UpgradeApplier] Dash while grappling enabled successfully!");
    }
    
    // Grapple effects
    private void ApplyPrimaryGrappleRangeChange(float delta)
    {
        if (playerSwing == null) 
        {
            Debug.LogError("[UpgradeApplier] PlayerSwing is null! Cannot apply primary grapple range change.");
            return;
        }
        
        float currentRange = GetPrimaryGrappleRange();
        float newRange = Mathf.Clamp(currentRange + delta, minGrappleRange, maxGrappleRange);
        SetPrimaryGrappleRange(newRange);
        
        Debug.Log($"[UpgradeApplier] Primary grapple range changed: {currentRange:F2} -> {newRange:F2} (delta: {delta:F2})");
    }
    
    private void ApplySecondaryGrappleRangeChange(float delta)
    {
        if (playerSwing == null) 
        {
            Debug.LogError("[UpgradeApplier] PlayerSwing is null! Cannot apply secondary grapple range change.");
            return;
        }
        
        float currentRange = GetSecondaryGrappleRange();
        float newRange = Mathf.Clamp(currentRange + delta, minGrappleRange, maxGrappleRange);
        SetSecondaryGrappleRange(newRange);
        
        Debug.Log($"[UpgradeApplier] Secondary grapple range changed: {currentRange:F2} -> {newRange:F2} (delta: {delta:F2})");
    }
    
    private void EnableSecondaryGrapple()
    {
        if (playerSwing == null) 
        {
            Debug.LogError("[UpgradeApplier] PlayerSwing is null! Cannot enable secondary grapple.");
            return;
        }
        
        playerSwing.SetEnableSecondaryGrapple(true);
        Debug.Log("[UpgradeApplier] Secondary grapple enabled successfully!");
    }
    
    // Score effects
    private void ApplyScoreRateMultiplier(float multiplier)
    {
        if (scoreSystem == null) 
        {
            Debug.LogError("[UpgradeApplier] ScoreSystem is null! Cannot apply score rate multiplier.");
            return;
        }
        
        float oldMultiplier = currentScoreRateMultiplier;
        currentScoreRateMultiplier *= multiplier;
        currentScoreRateMultiplier = Mathf.Clamp(currentScoreRateMultiplier, minScoreRateMultiplier, maxScoreRateMultiplier);
        
        Debug.Log($"[UpgradeApplier] Applying score rate multiplier: {oldMultiplier:F2} * {multiplier:F2} = {currentScoreRateMultiplier:F2}");
        
        scoreSystem.SetScoreRateMultiplier(currentScoreRateMultiplier);
        
        // Verify the change was applied
        Debug.Log($"[UpgradeApplier] Score system SetScoreRateMultiplier called with: {currentScoreRateMultiplier:F2}");
    }
    
    private void ApplyCollectibleValueChange(int delta)
    {
        if (spawnerSystem == null) 
        {
            Debug.LogError("[UpgradeApplier] SpawnerSystem is null! Cannot apply collectible value change.");
            return;
        }
        
        // Apply multiplier to collectible value
        float multiplier = 1f + (delta / 500f); // Convert delta to multiplier
        spawnerSystem.SetCollectibleValueMultiplier(multiplier);
        
        Debug.Log($"[UpgradeApplier] Collectible value multiplier changed to: {multiplier:F2} (delta: {delta})");
    }
    
    // Camera effects
    private void ApplyCameraSpeedMultiplier(float multiplier)
    {
        if (cameraSystem == null) 
        {
            Debug.LogError("[UpgradeApplier] CameraSystem is null! Cannot apply camera speed multiplier.");
            return;
        }
        
        // Apply as cumulative multiplier (each upgrade multiplies the current speed)
        currentCameraSpeedMultiplier *= multiplier;
        currentCameraSpeedMultiplier = Mathf.Clamp(currentCameraSpeedMultiplier, minCameraSpeedMultiplier, maxCameraSpeedMultiplier);
        
        cameraSystem.SetSpeedMultiplier(currentCameraSpeedMultiplier);
        
        // Verify the change was applied
        float actualMultiplier = cameraSystem.GetSpeedMultiplier();
        Debug.Log($"[UpgradeApplier] Camera speed multiplier changed to: {currentCameraSpeedMultiplier:F2} (applied: {multiplier:F2})");
        Debug.Log($"[UpgradeApplier] Camera system actual multiplier: {actualMultiplier:F2}");
    }
    
    private void ApplyCameraSizeMultiplier(float multiplier)
    {
        if (cameraSystem == null) 
        {
            Debug.LogError("[UpgradeApplier] CameraSystem is null! Cannot apply camera size multiplier.");
            return;
        }
        
        // Apply as cumulative multiplier (each upgrade multiplies the current size)
        currentCameraSizeMultiplier *= multiplier;
        currentCameraSizeMultiplier = Mathf.Clamp(currentCameraSizeMultiplier, minCameraSizeMultiplier, maxCameraSizeMultiplier);
        
        cameraSystem.SetSizeMultiplier(currentCameraSizeMultiplier);
        
        // Verify the change was applied
        float actualMultiplier = cameraSystem.GetSizeMultiplier();
        Debug.Log($"[UpgradeApplier] Camera size multiplier changed to: {currentCameraSizeMultiplier:F2} (applied: {multiplier:F2})");
        Debug.Log($"[UpgradeApplier] Camera system actual size multiplier: {actualMultiplier:F2}");
    }
    
    // World effects
    private void ApplyHazardSpawnRateMultiplier(float multiplier)
    {
        if (spawnerSystem == null) 
        {
            Debug.LogError("[UpgradeApplier] SpawnerSystem is null! Cannot apply hazard spawn rate multiplier.");
            return;
        }
        
        currentHazardSpawnRateMultiplier *= multiplier;
        
        spawnerSystem.SetHazardSpawnRateMultiplier(currentHazardSpawnRateMultiplier);
        
        Debug.Log($"[UpgradeApplier] Hazard spawn rate multiplier changed to: {currentHazardSpawnRateMultiplier:F2} (applied: {multiplier:F2})");
    }
    
    private void ApplyAnchorClusterSpacingChange(float delta)
    {
        if (spawnerSystem == null) 
        {
            Debug.LogError("[UpgradeApplier] SpawnerSystem is null! Cannot apply anchor cluster spacing change.");
            return;
        }
        
        float currentSpacing = spawnerSystem.minAnchorDistance;
        float newSpacing = currentSpacing + delta;
        spawnerSystem.SetAnchorClusterSpacing(newSpacing);
        
        Debug.Log($"[UpgradeApplier] Anchor cluster spacing changed: {currentSpacing:F2} -> {newSpacing:F2} (delta: {delta:F2})");
    }
    
    private void ApplyCollectibleSpawnDistanceChange(float delta)
    {
        if (spawnerSystem == null) 
        {
            Debug.LogError("[UpgradeApplier] SpawnerSystem is null! Cannot apply collectible spawn distance change.");
            return;
        }
        
        float currentDistance = spawnerSystem.minCollectibleDistance;
        float newDistance = currentDistance + delta;
        spawnerSystem.SetCollectibleSpawnDistance(newDistance);
        
        Debug.Log($"[UpgradeApplier] Collectible spawn distance changed: {currentDistance:F2} -> {newDistance:F2} (delta: {delta:F2})");
    }
    
    private void ApplyAnchorDensityMultiplier(float multiplier)
    {
        if (spawnerSystem == null) 
        {
            Debug.LogWarning("[UpgradeApplier] SpawnerSystem is null, attempting to find it...");
            spawnerSystem = FindFirstObjectByType<EnhancedSpawner>();
            if (spawnerSystem == null)
            {
                Debug.LogError("[UpgradeApplier] SpawnerSystem not found! Cannot apply anchor density multiplier.");
                return;
            }
            Debug.Log("[UpgradeApplier] SpawnerSystem found successfully!");
        }
        
        spawnerSystem.SetAnchorDensityMultiplier(multiplier);
        
        Debug.Log($"[UpgradeApplier] Anchor density multiplier changed to: {multiplier:F2}");
    }
    
    // Safety effects
    private void ApplyGracePeriodChange(float delta)
    {
        if (cameraSystem == null) 
        {
            Debug.LogWarning("[UpgradeApplier] CameraSystem is null, attempting to find it...");
            cameraSystem = FindFirstObjectByType<EnhancedCamera>();
            if (cameraSystem == null)
            {
                Debug.LogError("[UpgradeApplier] CameraSystem not found! Cannot apply grace period change.");
                return;
            }
            Debug.Log("[UpgradeApplier] CameraSystem found successfully!");
        }
        
        float currentGracePeriod = cameraSystem.gracePeriod;
        float newGracePeriod = currentGracePeriod + delta;
        cameraSystem.SetGracePeriod(newGracePeriod);
        
        Debug.Log($"[UpgradeApplier] Grace period changed: {currentGracePeriod:F2} -> {newGracePeriod:F2} (delta: {delta:F2})");
    }
    
    private void ApplyDeathDistanceChange(float delta)
    {
        if (cameraSystem == null) 
        {
            Debug.LogWarning("[UpgradeApplier] CameraSystem is null, attempting to find it...");
            cameraSystem = FindFirstObjectByType<EnhancedCamera>();
            if (cameraSystem == null)
            {
                Debug.LogError("[UpgradeApplier] CameraSystem not found! Cannot apply death distance change.");
                return;
            }
            Debug.Log("[UpgradeApplier] CameraSystem found successfully!");
        }
        
        float currentDeathDistance = cameraSystem.deathDistance;
        float newDeathDistance = currentDeathDistance + delta;
        cameraSystem.SetDeathDistance(newDeathDistance);
        
        Debug.Log($"[UpgradeApplier] Death distance changed: {currentDeathDistance:F2} -> {newDeathDistance:F2} (delta: {delta:F2})");
    }
    
    // Helper methods to get/set values
    private float GetDashCooldown()
    {
        if (playerDash != null)
            return playerDash.GetDashCooldown();
        return 1.5f; // Default value
    }
    
    private void SetDashCooldown(float value)
    {
        if (playerDash != null)
            playerDash.SetDashCooldown(value);
    }
    
    private float GetDashForce()
    {
        if (playerDash != null)
            return playerDash.GetDashForce();
        return 15f; // Default value
    }
    
    private void SetDashForce(float value)
    {
        if (playerDash != null)
            playerDash.SetDashForce(value);
    }
    
    private float GetPrimaryGrappleRange()
    {
        if (playerSwing != null)
            return playerSwing.GetMaxRopeLength();
        return 8f; // Default value
    }
    
    private void SetPrimaryGrappleRange(float value)
    {
        if (playerSwing != null)
            playerSwing.SetMaxRopeLength(value);
    }
    
    private float GetSecondaryGrappleRange()
    {
        if (playerSwing != null)
            return playerSwing.GetSecondaryGrappleMaxDistance();
        return 8f; // Default value
    }
    
    private void SetSecondaryGrappleRange(float value)
    {
        if (playerSwing != null)
            playerSwing.SetSecondaryGrappleMaxDistance(value);
    }
}
