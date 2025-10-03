# Spawning System Setup - Reference Guide

## What This Document Contains

This document explains how to build a system that automatically creates obstacles and collectibles as the player moves through the game. Instead of manually placing everything, the system generates content on-the-fly to create endless gameplay.

## The Spawning System Overview

### What Each Part Does
- **EnhancedSpawner.cs** - Controls when and where to spawn objects
- **EnhancedCollectible.cs** - Handles individual collectible behavior
- **Pattern-based generation** - Creates obstacles in organized shapes instead of random placement
- **Difficulty scaling** - Gradually increases challenge over time
- **Object culling system** - Removes objects behind the camera to prevent performance issues

### How It Works
1. **Spawner** detects when the player is approaching and creates obstacles ahead
2. **Pattern System** arranges obstacles in interesting shapes (lines, spirals, waves, etc.)
3. **Hazard System** randomly makes some obstacles dangerous
4. **Collectible System** places coins in accessible locations
5. **Ground System** creates seamless endless ground tiles
6. **Cleanup System** removes old objects to keep performance smooth

## How to Build Your Own Spawning System (Step by Step)

### Step 1: Create the Basic Spawner
This script manages when and where to create objects in your game.

```csharp
public class EnhancedSpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    [SerializeField] private float startSpacingRange = 2f; // Initial distance between objects
    [SerializeField] private float maxSpacingRange = 8f; // Maximum distance as game gets harder
    [SerializeField] private float rampDuration = 150f; // Time to reach maximum difficulty
    [SerializeField] private float hazardChance = 0.1f; // 10% chance objects are dangerous
    
    [Header("Patterns")]
    [SerializeField] private float basePatternChance = 0.5f; // 50% chance to use complex patterns
    [SerializeField] private AnimationCurve difficultyCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // How difficulty increases over time
    
    [Header("References")]
    [SerializeField] private GameObject anchorPrefab; // Template for obstacles
    [SerializeField] private GameObject collectiblePrefab; // Template for coins
    [SerializeField] private GameObject groundPrefab; // Template for ground tiles
    
    private float gameTime; // Current game time
    private float lastSpawnX; // Last spawn position
    private List<GameObject> activeObjects = new List<GameObject>(); // Currently active objects
    
    void Start()
    {
        lastSpawnX = transform.position.x;
    }
    
    void Update()
    {
        gameTime += Time.deltaTime;
        SpawnAheadOfCamera();
        CullBehindCamera();
    }
}
```

### Step 2: Create Pattern System
Instead of placing obstacles randomly, organize them into interesting shapes that create better gameplay.

```csharp
// Different obstacle arrangements you can use
public enum PatternType
{
    HorizontalLine,    // Straight line of obstacles
    StairPattern,      // Stairs going upward
    CompactBlock,      // Square formation
    VShape,           // V-shaped arrangement
    TShape,           // T-shaped formation
    LShape,           // L-shaped formation
    SineWave,         // Wave pattern
    CosineWave,       // Offset wave pattern
    LogarithmicSpiral, // Spiral formation
    FibonacciSpiral,   // Golden ratio spiral
    WavePattern        // Complex wave combination
}

void SpawnClusterAt(float x)
{
    // Choose a pattern based on current difficulty
    PatternType pattern = ChoosePattern();
    
    // Generate positions for this pattern
    Vector2[] positions = GeneratePattern(pattern, x);
    
    // Create obstacles at each position
    foreach (Vector2 pos in positions)
    {
        bool isHazard = Random.value < hazardChance; // Random chance for danger
        SpawnAnchor(pos, isHazard);
    }
}

PatternType ChoosePattern()
{
    // Increase complex pattern chance as game gets harder
    float difficulty = Mathf.Clamp01(gameTime / rampDuration);
    float patternChance = basePatternChance + (difficulty * 0.3f);
    
    if (Random.value < patternChance)
    {
        // Choose complex pattern
        PatternType[] complexPatterns = { PatternType.StairPattern, PatternType.VShape, PatternType.TShape };
        return complexPatterns[Random.Range(0, complexPatterns.Length)];
    }
    else
    {
        // Choose simple pattern
        PatternType[] simplePatterns = { PatternType.HorizontalLine, PatternType.CompactBlock };
        return simplePatterns[Random.Range(0, simplePatterns.Length)];
    }
}

Vector2[] GeneratePattern(PatternType pattern, float x)
{
    List<Vector2> positions = new List<Vector2>();
    
    switch (pattern)
    {
        case PatternType.HorizontalLine:
            positions.Add(new Vector2(x, Random.Range(-1f, 4f)));
            positions.Add(new Vector2(x + 2f, Random.Range(-1f, 4f)));
            positions.Add(new Vector2(x + 4f, Random.Range(-1f, 4f)));
            break;
            
        case PatternType.StairPattern:
            for (int i = 0; i < 4; i++)
            {
                positions.Add(new Vector2(x + i * 1.5f, -1f + i * 1.5f));
            }
            break;
            
        case PatternType.CompactBlock:
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    positions.Add(new Vector2(x + i * 1f, -1f + j * 1f));
                }
            }
            break;
            
        case PatternType.VShape:
            positions.Add(new Vector2(x, 2f));
            positions.Add(new Vector2(x + 1f, 1f));
            positions.Add(new Vector2(x + 2f, 0f));
            positions.Add(new Vector2(x + 3f, 1f));
            positions.Add(new Vector2(x + 4f, 2f));
            break;
    }
    
    return positions.ToArray();
}
```

### Step 3: Collectible System
```csharp
// EnhancedCollectible.cs - Individual collectible behavior
public class EnhancedCollectible : MonoBehaviour
{
    [Header("Collectible Settings")]
    [SerializeField] private int pointsValue = 500;
    [SerializeField] private float collectionRadius = 1f;
    [SerializeField] private LayerMask playerLayerMask = 1;
    
    [Header("Visual Effects")]
    [SerializeField] private float rotationSpeed = 90f;
    [SerializeField] private float bobAmplitude = 0.5f;
    [SerializeField] private float bobFrequency = 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioSource audioSource;
    
    private bool isCollected = false;
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
        EnsureAudioSource();
    }
    
    void Update()
    {
        if (isCollected) return;
        
        // Visual effects
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Bobbing animation
        float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        transform.position = startPosition + Vector3.up * bobOffset;
        
        // Check for collection
        CheckForCollection();
    }
    
    void CheckForCollection()
    {
        Collider2D player = Physics2D.OverlapCircle(transform.position, collectionRadius, playerLayerMask);
        if (player != null)
        {
            Collect();
        }
    }
    
    void Collect()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Add points to score
        EnhancedScore.Instance?.AddPoints(pointsValue);
        
        // Play collection sound
        if (audioSource && collectSound)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            audioSource.PlayOneShot(collectSound, sfxVolume);
        }
        
        // Visual feedback
        StartCoroutine(CollectAnimation());
    }
    
    System.Collections.IEnumerator CollectAnimation()
    {
        // Scale up and fade out
        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, startScale * 1.5f, progress);
            
            // Fade out
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color color = sr.color;
                color.a = Mathf.Lerp(1f, 0f, progress);
                sr.color = color;
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // Destroy the collectible
        Destroy(gameObject);
    }
}
```

### Step 4: Object Culling System
```csharp
// CullObjectsBehindCamera - Actual implementation from EnhancedSpawner.cs
void CullObjectsBehindCamera(float cameraX)
{
    // Cull distance - objects behind camera by this amount get destroyed
    float cullX = cameraX - cullDistance;
    
    // Find all spawned objects and cull them if they're behind the camera
    GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
    foreach (GameObject obj in allObjects)
    {
        // Only cull objects that are likely spawned by this spawner
        if (obj.name.Contains("AnchorPoint") || 
            obj.name.Contains("Collectible") || 
            obj.name.Contains("Ground") || 
            obj.name.Contains("BG_Black") ||
            obj.name.Contains("(Clone)"))
        {
            if (obj.transform.position.x < cullX)
            {
                Destroy(obj);
            }
        }
    }
    
    // Also cull collectibles specifically by component
    EnhancedCollectible[] collectibles = FindObjectsByType<EnhancedCollectible>(FindObjectsSortMode.None);
    foreach (EnhancedCollectible collectible in collectibles)
    {
        if (collectible.transform.position.x < cullX)
        {
            Destroy(collectible.gameObject);
        }
    }
}
```

### Step 5: Ground Tiling System
```csharp
// GroundTiler.cs - Endless ground generation
public class GroundTiler : MonoBehaviour
{
    [Header("Ground Settings")]
    [SerializeField] private float groundY = -4f;
    [SerializeField] private float aheadScreens = 3f;
    [SerializeField] private float behindScreens = 1f;
    [SerializeField] private GameObject groundPrefab;
    
    private LinkedList<GameObject> activeTiles = new LinkedList<GameObject>();
    private float tileWidth;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        MeasureTileWidth();
        SpawnInitialTiles();
    }
    
    void Update()
    {
        SpawnForwardUntilCovered();
        CullBehind();
    }
    
    void MeasureTileWidth()
    {
        if (groundPrefab != null)
        {
            SpriteRenderer sr = groundPrefab.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                tileWidth = sr.bounds.size.x;
            }
            else
            {
                tileWidth = 10f; // Default fallback
            }
        }
    }
    
    void SpawnInitialTiles()
    {
        float cameraLeft = mainCamera.transform.position.x - mainCamera.orthographicSize * mainCamera.aspect;
        float startX = cameraLeft - (behindScreens * mainCamera.orthographicSize * mainCamera.aspect);
        float endX = cameraLeft + (aheadScreens * mainCamera.orthographicSize * mainCamera.aspect);
        
        for (float x = startX; x < endX; x += tileWidth)
        {
            SpawnTileAt(x);
        }
    }
    
    void SpawnForwardUntilCovered()
    {
        float cameraRight = mainCamera.transform.position.x + mainCamera.orthographicSize * mainCamera.aspect;
        float targetX = cameraRight + (aheadScreens * mainCamera.orthographicSize * mainCamera.aspect);
        
        if (activeTiles.Count > 0)
        {
            float lastTileX = activeTiles.Last.Value.transform.position.x + tileWidth / 2f;
            while (lastTileX < targetX)
            {
                SpawnTileAt(lastTileX + tileWidth / 2f);
                lastTileX += tileWidth;
            }
        }
    }
    
    void SpawnTileAt(float x)
    {
        GameObject tile = Instantiate(groundPrefab, new Vector3(x, groundY, 0), Quaternion.identity);
        activeTiles.AddLast(tile);
    }
    
    void CullBehind()
    {
        float cameraLeft = mainCamera.transform.position.x - mainCamera.orthographicSize * mainCamera.aspect;
        float cullX = cameraLeft - (behindScreens * mainCamera.orthographicSize * mainCamera.aspect);
        
        while (activeTiles.Count > 0)
        {
            GameObject firstTile = activeTiles.First.Value;
            if (firstTile.transform.position.x + tileWidth / 2f < cullX)
            {
                activeTiles.RemoveFirst();
                Destroy(firstTile);
            }
            else
            {
                break;
            }
        }
    }
}
```

## Key Patterns I Used

### 1. Mathematical Pattern Generation
These patterns use mathematical functions to create interesting obstacle arrangements. You don't need to understand the math - just copy the code and adjust the parameters.

```csharp
// Creates obstacles in a sine wave pattern
void SpawnSineWaveAt(Vector3 basePosition)
{
    float amplitude = Random.Range(2f, 4f); // Wave height
    float frequency = Random.Range(0.5f, 1.5f); // Wave frequency
    float spacing = Random.Range(0.8f, 1.2f); // Distance between obstacles
    int pointCount = Random.Range(8, 15); // Number of obstacles
    
    for (int i = 0; i < pointCount; i++)
    {
        float x = i * spacing;
        float y = Mathf.Sin(x * frequency) * amplitude;
        Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
        SpawnSingleAnchorAt(pos);
    }
}

// Creates obstacles in a logarithmic spiral
void SpawnLogarithmicSpiralAt(Vector3 basePosition)
{
    float a = Random.Range(0.1f, 0.3f); // Growth rate
    float b = Random.Range(0.5f, 1.5f); // Spiral tightness
    int pointCount = Random.Range(12, 20);
    
    for (int i = 0; i < pointCount; i++)
    {
        float angle = i * 0.5f;
        float radius = a * Mathf.Exp(b * angle);
        
        // Convert polar coordinates to cartesian
        float x = radius * Mathf.Cos(angle);
        float y = radius * Mathf.Sin(angle);
        
        Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
        SpawnSingleAnchorAt(pos);
    }
}

// Creates obstacles in a complex wave pattern
void SpawnWavePatternAt(Vector3 basePosition)
{
    float amplitude1 = Random.Range(1.5f, 3f);
    float frequency1 = Random.Range(0.3f, 0.8f);
    float amplitude2 = Random.Range(0.5f, 1.5f);
    float frequency2 = Random.Range(1.0f, 2.0f);
    float spacing = Random.Range(0.6f, 1.0f);
    int pointCount = Random.Range(10, 18);
    
    for (int i = 0; i < pointCount; i++)
    {
        float x = i * spacing;
        // Combine two sine waves
        float y = (Mathf.Sin(x * frequency1) * amplitude1) + (Mathf.Sin(x * frequency2) * amplitude2);
        Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
        SpawnSingleAnchorAt(pos);
    }
}
```

### 2. Pattern-Based Generation
```csharp
// Use enums and switch statements for clean pattern definition
public enum PatternType { HorizontalLine, StairPattern, CompactBlock, VShape, TShape, LShape }

Vector2[] GeneratePattern(PatternType pattern, float x)
{
    switch (pattern)
    {
        case PatternType.HorizontalLine:
            // Generate horizontal line pattern
            break;
        case PatternType.StairPattern:
            // Generate stair pattern
            break;
        // etc...
    }
}
```

### 2. Difficulty Scaling Pattern
```csharp
// Scale difficulty over time using curves
float difficulty = Mathf.Clamp01(gameTime / rampDuration);
float currentSpacing = Mathf.Lerp(startSpacingRange, maxSpacingRange, difficultyCurve.Evaluate(difficulty));
```

### 3. Camera-Relative Spawning
```csharp
// Spawn objects relative to camera position
float cameraRight = mainCamera.transform.position.x + mainCamera.orthographicSize * mainCamera.aspect;
float spawnX = cameraRight + spawnDistance;
```

### 4. Object Culling Pattern
```csharp
// Remove objects behind camera to prevent memory leaks
void CullBehind()
{
    float cameraLeft = mainCamera.transform.position.x - mainCamera.orthographicSize * mainCamera.aspect;
    float cullX = cameraLeft - behindDistance;
    
    // Remove objects that are too far behind
}
```

## What I Learned

### Good Practices
- **Use pattern-based generation** for varied, interesting layouts
- **Scale difficulty over time** to maintain engagement
- **Always cull behind camera** to prevent memory leaks
- **Use efficient culling** to destroy objects behind camera
- **Provide visual feedback** for all interactions

### Common Pitfalls
- **Don't forget to cull objects** - memory leaks will kill performance
- **Don't make patterns too complex** - players need to understand them
- **Don't spawn too close to camera** - objects will pop in visibly
- **Don't forget to handle edge cases** - what if camera moves backward?

### Performance Tips
- **Use LinkedList for active objects** - efficient insertion/deletion
- **Cache camera references** - don't find them every frame
- **Use efficient culling** - destroy objects behind camera to prevent memory leaks
- **Limit active objects** - don't spawn hundreds at once

## Integration Patterns

### With Camera System
```csharp
// Get camera bounds for spawning
Camera cam = Camera.main;
float cameraRight = cam.transform.position.x + cam.orthographicSize * cam.aspect;
float spawnX = cameraRight + spawnDistance;
```

### With Scoring System
```csharp
// Add points when collectible is collected
EnhancedScore.Instance?.AddPoints(pointsValue);
```

### With Culling System
```csharp
// Cull objects behind camera
void CullObjectsBehindCamera(float cameraX)
{
    float cullX = cameraX - cullDistance;
    
    GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
    foreach (GameObject obj in allObjects)
    {
        if (obj.name.Contains("AnchorPoint") || obj.name.Contains("Collectible"))
        {
            if (obj.transform.position.x < cullX)
            {
                Destroy(obj);
            }
        }
    }
}
```

## Advanced Features

### Dynamic Pattern Selection
```csharp
// Choose patterns based on player performance
PatternType ChoosePatternBasedOnPerformance()
{
    float playerSpeed = EnhancedCamera.Instance?.GetCurrentSpeed() ?? 1f;
    float baseSpeed = EnhancedCamera.Instance?.GetStartSpeed() ?? 1f;
    float speedRatio = playerSpeed / baseSpeed;
    
    if (speedRatio > 2f)
    {
        // Player is going fast - use easier patterns
        return PatternType.HorizontalLine;
    }
    else if (speedRatio < 1.5f)
    {
        // Player is going slow - use harder patterns
        return PatternType.VShape;
    }
    else
    {
        // Normal speed - use random patterns
        return ChooseRandomPattern();
    }
}
```

### Hazard System
```csharp
// Make some anchors deadly
void SpawnAnchor(Vector2 position, bool isHazard)
{
    GameObject anchor = Instantiate(anchorPrefab, position, Quaternion.identity);
    
    if (isHazard)
    {
        // Tag as hazard for death detection
        anchor.tag = "Death";
        
        // Change visual appearance
        SpriteRenderer sr = anchor.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.red;
        }
    }
}
```

### Collectible Value Scaling
```csharp
// Increase collectible value over time
int CalculateCollectibleValue()
{
    float difficulty = Mathf.Clamp01(gameTime / rampDuration);
    int baseValue = 500;
    int bonusValue = Mathf.RoundToInt(difficulty * 500);
    return baseValue + bonusValue;
}
```

## Future Improvements

### What I Could Add Next Time
- **Biome system** - different environments with different patterns
- **Power-up spawning** - special collectibles with unique effects
- **Dynamic difficulty** - adjust based on player performance
- **Pattern combinations** - mix multiple patterns together
- **Visual variety** - different sprites for different pattern types

### Advanced Features
- **Procedural pattern generation** - create patterns algorithmically
- **Pattern learning** - AI that learns what patterns players like
- **Seasonal events** - special patterns for holidays/events
- **Player-created patterns** - let players design their own patterns

## Quick Setup Checklist

For future projects, here's my quick spawning system setup:

1. Create EnhancedSpawner with pattern generation
2. Define pattern types and generation logic
3. Implement difficulty scaling over time
4. Create collectible system with collection mechanics
5. Implement ground tiling for endless world
6. Add object culling to prevent memory leaks
7. Implement efficient object destruction system
8. Add visual feedback for all interactions
9. Test pattern variety and difficulty progression
10. Optimize performance with proper culling

This spawning system worked really well for Quantum Thread and should work for procedural generation in other 2D games I work on in the future.
