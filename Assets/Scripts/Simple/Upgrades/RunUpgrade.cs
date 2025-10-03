using UnityEngine;

/// <summary>
/// Data class for individual upgrades that can be purchased during a run
/// </summary>
[System.Serializable]
public class RunUpgrade
{
    [Header("Basic Info")]
    public string id;
    public string displayName;
    [TextArea(2, 4)]
    public string description;
    public Sprite icon;
    public int cost;
    public int maxStacks;
    
    [Header("Category")]
    public UpgradeCategory category;
    
    [Header("Movement Effects")]
    [Tooltip("Additive change to dash cooldown (negative = faster)")]
    public float dashCooldownDelta = 0f;
    
    [Tooltip("Additive change to dash force strength")]
    public float dashForceDelta = 0f;
    
    [Tooltip("Enable dash while grappling")]
    public bool enableDashWhileGrappling = false;
    
    [Header("Grapple Effects")]
    [Tooltip("Additive change to primary grapple range")]
    public float primaryGrappleRangeDelta = 0f;
    
    [Tooltip("Additive change to secondary grapple range")]
    public float secondaryGrappleRangeDelta = 0f;
    
    [Tooltip("Enable secondary grapple ability")]
    public bool enableSecondaryGrapple = false;
    
    [Header("Score Effects")]
    [Tooltip("Multiplier for score rate (1.0 = no change)")]
    public float scoreRateMultiplier = 1f;
    
    [Tooltip("Additive change to collectible value")]
    public int collectibleValueDelta = 0;
    
    [Header("Camera Effects")]
    [Tooltip("Multiplier for camera speed (1.0 = no change)")]
    public float cameraSpeedMultiplier = 1f;
    
    [Tooltip("Multiplier for camera size/zoom (1.0 = no change, 1.5 = 50% larger view)")]
    public float cameraSizeMultiplier = 1f;
    
    [Header("World Effects")]
    [Tooltip("Multiplier for hazard spawn rate (1.0 = no change)")]
    public float hazardSpawnRateMultiplier = 1f;
    
    [Tooltip("Additive change to anchor cluster spacing")]
    public float anchorClusterSpacingDelta = 0f;
    
    [Tooltip("Multiplier for anchor density (1.0 = normal, 0.5 = half density, 2.0 = double density)")]
    public float anchorDensityMultiplier = 1f;
    
    [Tooltip("Additive change to collectible spawn distance")]
    public float collectibleSpawnDistanceDelta = 0f;
    
    [Header("Safety Effects")]
    [Tooltip("Additive change to grace period after restart")]
    public float gracePeriodDelta = 0f;
    
    [Tooltip("Additive change to death distance behind camera")]
    public float deathDistanceDelta = 0f;
}

/// <summary>
/// Categories for organizing upgrades
/// </summary>
public enum UpgradeCategory
{
    Movement,
    Grapple,
    Score,
    Camera,
    World,
    Safety
}
