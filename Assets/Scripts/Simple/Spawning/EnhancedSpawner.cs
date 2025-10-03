using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Enhanced spawner with patterns and difficulty scaling
/// This is the improved version that actually works well
/// </summary>
public class EnhancedSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField, Tooltip("Anchor point prefab")]
    private GameObject anchorPrefab;
    
    [SerializeField, Tooltip("Collectible coin prefab")]
    private GameObject collectiblePrefab;
    
    [SerializeField, Tooltip("Ground tile prefab")]
    private GameObject groundPrefab;
    
    [SerializeField, Tooltip("Background tile prefab")]
    private GameObject backgroundPrefab;
    
    [Header("Audio")]
    [SerializeField, Tooltip("Collectible collection sound")]
    private AudioClip collectibleSound;
    
    [Header("Multi-Image Sprites")]
    [SerializeField, Tooltip("Multiple anchor point sprites for variety")]
    private Sprite[] anchorSprites;
    
    
    [SerializeField, Tooltip("Planet and space object sprites for background decoration")]
    private Sprite[] planetSprites;
    
    [SerializeField, Tooltip("Various decoration sprites for random background variety")]
    private Sprite[] decorationSprites;
    
    [SerializeField, Tooltip("Background starfield sprites for variety (corona, normal, diagonal)")]
    private Sprite[] backgroundSprites;
    
    [Header("Planet & Space Object Settings")]
    [SerializeField, Tooltip("Enable random planets and space objects in background")]
    private bool enablePlanets = true;
    
    [SerializeField, Tooltip("Chance for planet/space object to spawn per frame (0-1)")]
    private float planetSpawnChance = 0.001f;
    
    [SerializeField, Tooltip("Planet spawn distance ahead of camera")]
    private float planetAheadDistance = 60f;
    
    [SerializeField, Tooltip("Planet spawn height range")]
    private Vector2 planetHeightRange = new Vector2(-3f, 10f);
    
    [SerializeField, Tooltip("Planet spawn distance range (how far left/right)")]
    private Vector2 planetHorizontalRange = new Vector2(-15f, 15f);
    
    [SerializeField, Tooltip("Planet scale range for size variety")]
    private Vector2 planetScaleRange = new Vector2(0.5f, 2f);
    
    [Header("Decoration Settings")]
    [SerializeField, Tooltip("Enable random decorations in background")]
    private bool enableDecorations = true;
    
    [SerializeField, Tooltip("Chance for decoration to spawn per frame (0-1)")]
    private float decorationSpawnChance = 0.002f;
    
    [SerializeField, Tooltip("Decoration spawn distance ahead of camera")]
    private float decorationAheadDistance = 50f;
    
    [SerializeField, Tooltip("Decoration spawn height range")]
    private Vector2 decorationHeightRange = new Vector2(-2f, 8f);
    
    [SerializeField, Tooltip("Decoration spawn distance range (how far left/right)")]
    private Vector2 decorationHorizontalRange = new Vector2(-20f, 20f);
    
    [SerializeField, Tooltip("Decoration scale range for size variety")]
    private Vector2 decorationScaleRange = new Vector2(0.3f, 1.5f);
    
    [SerializeField, Tooltip("Decoration sorting order (higher = in front)")]
    private int decorationSortingOrder = 3;
    
    [Header("Spawn Distances")]
    [SerializeField, Tooltip("Min distance between anchors")]
    public float minAnchorDistance = 8f;
    
    [SerializeField, Tooltip("Max distance between anchors")]
    private float maxAnchorDistance = 15f;
    
    [SerializeField, Tooltip("Anchor density multiplier (1.0 = normal, 0.5 = half density, 2.0 = double density)")]
    private float anchorDensityMultiplier = 1.0f;
    
    [SerializeField, Tooltip("Min distance between collectibles")]
    public float minCollectibleDistance = 15f;
    
    [SerializeField, Tooltip("Max distance between collectibles")]
    private float maxCollectibleDistance = 30f;
    
    [SerializeField, Tooltip("Ground tile width (should match prefab width)")]
    private float groundTileWidth = 25f;
    
    [SerializeField, Tooltip("Background tile width (should match prefab width)")]
    private float backgroundTileWidth = 45f;
    
    [SerializeField, Tooltip("How far ahead to spawn ground tiles")]
    private float groundAheadDistance = 60f;
    
    [SerializeField, Tooltip("How far ahead to spawn background tiles")]
    private float backgroundAheadDistance = 80f;
    
    [SerializeField, Tooltip("How far ahead to spawn anchors")]
    private float anchorAheadDistance = 15f;
    
    [SerializeField, Tooltip("How far behind camera to cull objects")]
    private float cullDistance = 50f;
    
    [SerializeField, Tooltip("Minimum distance collectibles must be from anchor points")]
    private float collectibleMinDistanceFromAnchors = 3f;
    
    [SerializeField, Tooltip("Maximum distance collectibles can be from a grappleable anchor point")]
    private float collectibleMaxDistanceFromAnchors = 8f;
    
    [SerializeField, Tooltip("Ensure collectibles have a clear grappling path (not blocked by hazards)")]
    private bool ensureClearPath = true;
    
    [SerializeField, Tooltip("Ground Y position")]
    private float groundY = -5f;
    
    [Header("Spawn Heights")]
    [SerializeField, Tooltip("Min spawn height")]
    private float minHeight = 2f;
    
    [SerializeField, Tooltip("Max spawn height")]
    private float maxHeight = 8f;
    
    [Header("Patterns")]
    [SerializeField, Tooltip("Enable pattern spawning")]
    private bool usePatterns = true;
    
    [SerializeField, Tooltip("Pattern types")]
    private PatternType[] patterns = { PatternType.Cluster, PatternType.Line, PatternType.Stairs, PatternType.Pyramid, PatternType.Wall, PatternType.Spiral, PatternType.Cross, PatternType.Diamond, PatternType.Square, PatternType.SineWave, PatternType.CosineWave, PatternType.LogarithmicSpiral, PatternType.FibonacciSpiral, PatternType.WavePattern };
    
    [SerializeField, Tooltip("Hazard chance (0-1)")]
    private float hazardChance = 0.4f;
    
    [Header("Difficulty")]
    [SerializeField, Tooltip("Increase difficulty over time")]
    private bool scaleDifficulty = true;
    
    [SerializeField, Tooltip("Speed multiplier for difficulty")]
    private float difficultyMultiplier = 1.1f;
    
    [SerializeField, Tooltip("Max difficulty multiplier")]
    private float maxDifficulty = 3f;
    
    
    private float lastAnchorX;
    private float lastCollectibleX;
    private float lastGroundX;
    private float lastBackgroundX;
    private float gameTime;
    private float currentDifficulty = 1f;
    private Camera mainCamera;
    private float lastCameraX;
    private float cameraSpeed;
    
    // Next spawn distances (randomized for anchors/collectibles, fixed for ground/background)
    private float nextAnchorDistance;
    private float nextCollectibleDistance;
    
    public enum PatternType
    {
        Single,
        Cluster,
        Line,
        Stairs,
        Pyramid,
        Wall,
        Spiral,
        Cross,
        Diamond,
        Square,
        SineWave,
        CosineWave,
        LogarithmicSpiral,
        FibonacciSpiral,
        WavePattern
    }
    
    void Start()
    {
        // Check for missing prefabs
        if (anchorPrefab == null || collectiblePrefab == null || groundPrefab == null || backgroundPrefab == null)
        {
            return;
        }
        
        // Find the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        if (mainCamera == null)
        {
            return;
        }
        
        // Initialize spawn positions to start spawning immediately
        float cameraX = mainCamera.transform.position.x;
        lastCameraX = cameraX;
        cameraSpeed = 0f;
        
        lastAnchorX = cameraX - nextAnchorDistance; // Start spawning immediately
        lastCollectibleX = cameraX - nextCollectibleDistance; // Start spawning immediately
        
        // For ground and background, start them at camera position so they spawn immediately
        lastGroundX = cameraX;
        lastBackgroundX = cameraX;
        
        // Initialize random spawn distances (only for anchors and collectibles)
        nextAnchorDistance = Random.Range(minAnchorDistance, maxAnchorDistance);
        nextCollectibleDistance = Random.Range(minCollectibleDistance, maxCollectibleDistance);
        
        // Fill the screen immediately with ground and background
        FillScreenWithContent();
        
        // Set up the global collect sound for all collectibles
        if (collectibleSound != null)
        {
            EnhancedCollectible.SetGlobalCollectSound(collectibleSound);
            Debug.Log($"[EnhancedSpawner] Global collectible sound set to: {collectibleSound.name}");
        }
        else
        {
            // No global sound assigned - collectibles will use their individual sounds
            Debug.Log("[EnhancedSpawner] No global collectible sound assigned - collectibles will use their individual prefab sounds");
        }
        
    }
    
    void Update()
    {
        if (mainCamera == null) return;
        
        gameTime += Time.deltaTime;
        
        // Calculate camera speed for dynamic spawning
        float cameraX = mainCamera.transform.position.x;
        cameraSpeed = Mathf.Abs(cameraX - lastCameraX) / Time.deltaTime;
        lastCameraX = cameraX;
        
        // Update difficulty
        if (scaleDifficulty)
        {
            currentDifficulty = Mathf.Min(1f + (gameTime / 100f) * difficultyMultiplier, maxDifficulty);
        }
        
        // Cull objects behind camera
        CullObjectsBehindCamera(cameraX);
        
        // Spawn anchors
        float effectiveAnchorDistance = (nextAnchorDistance / currentDifficulty) / anchorDensityMultiplier;
        if (cameraX - lastAnchorX > effectiveAnchorDistance)
        {
            SpawnAnchor();
            lastAnchorX = cameraX;
            nextAnchorDistance = Random.Range(minAnchorDistance, maxAnchorDistance);
        }
        
        // Spawn collectibles
        if (cameraX - lastCollectibleX > nextCollectibleDistance / currentDifficulty)
        {
            SpawnCollectible();
            lastCollectibleX = cameraX;
            nextCollectibleDistance = Random.Range(minCollectibleDistance, maxCollectibleDistance);
        }
        
        // Spawn ground and background with seamless tiling
        SpawnSeamlessContent();
        
        // Spawn random planets and space objects
        SpawnRandomPlanet();
        
        // Spawn random decorations
        SpawnRandomDecoration();
    }
    
    void FillScreenWithContent()
    {
        if (mainCamera == null) return;
        
        float cameraX = mainCamera.transform.position.x;
        float cameraWidth = mainCamera.orthographicSize * 2f * mainCamera.aspect;
        
        // Fill ground tiles from camera position to ahead distance
        float groundStartX = cameraX;
        float groundEndX = cameraX + groundAheadDistance;
        
        for (float x = groundStartX; x <= groundEndX; x += groundTileWidth)
        {
            Vector3 pos = new Vector3(x, groundY, 0);
            Instantiate(groundPrefab, pos, Quaternion.identity);
        }
        
        // Fill background tiles from camera position to ahead distance
        if (backgroundPrefab != null)
        {
            float backgroundStartX = cameraX;
            float backgroundEndX = cameraX + backgroundAheadDistance;
            
            for (float x = backgroundStartX; x <= backgroundEndX; x += backgroundTileWidth)
            {
                Vector3 pos = new Vector3(x, 0, 10);
                Instantiate(backgroundPrefab, pos, Quaternion.identity);
            }
        }
        else
        {
            return;
        }
        
        // Update last positions to the end of the last tile (for seamless continuation)
        // Calculate the end position of the last tile placed
        float lastGroundTileEnd = groundStartX + Mathf.Floor((groundEndX - groundStartX) / groundTileWidth) * groundTileWidth + groundTileWidth;
        
        lastGroundX = lastGroundTileEnd;
        
        if (backgroundPrefab != null)
        {
            float backgroundStartX = cameraX;
            float backgroundEndX = cameraX + backgroundAheadDistance;
            float lastBackgroundTileEnd = backgroundStartX + Mathf.Floor((backgroundEndX - backgroundStartX) / backgroundTileWidth) * backgroundTileWidth + backgroundTileWidth;
            lastBackgroundX = lastBackgroundTileEnd;
        }
        else
        {
            lastBackgroundX = cameraX; // Set to camera position if no background prefab
        }
        
    }
    
    void SpawnSeamlessContent()
    {
        if (mainCamera == null) return;
        
        float cameraX = mainCamera.transform.position.x;
        
        // Spawn ground tiles ahead of camera
        while (cameraX + groundAheadDistance > lastGroundX)
        {
            Vector3 pos = new Vector3(lastGroundX, groundY, 0);
            Instantiate(groundPrefab, pos, Quaternion.identity);
            lastGroundX += groundTileWidth;
        }
        
        // Spawn background tiles ahead of camera
        while (cameraX + backgroundAheadDistance > lastBackgroundX)
        {
            Vector3 pos = new Vector3(lastBackgroundX, 0, 10);
            SpawnRandomBackgroundTile(pos);
            lastBackgroundX += backgroundTileWidth;
        }
    }
    
    void SpawnAnchor()
    {
        if (anchorPrefab == null) return;
        
        if (usePatterns && patterns.Length > 0)
        {
            SpawnAnchorPattern();
        }
        else
        {
            SpawnSingleAnchor();
        }
    }
    
    void SpawnAnchorPattern()
    {
        PatternType pattern = patterns[Random.Range(0, patterns.Length)];
        Vector3 basePos = new Vector3(mainCamera.transform.position.x + anchorAheadDistance, Random.Range(minHeight, maxHeight), 0);
        
        switch (pattern)
        {
            case PatternType.Single:
                SpawnSingleAnchorAt(basePos);
                break;
                
            case PatternType.Cluster:
                SpawnClusterAt(basePos);
                break;
                
            case PatternType.Line:
                SpawnLineAt(basePos);
                break;
                
            case PatternType.Stairs:
                SpawnStairsAt(basePos);
                break;
                
            case PatternType.Pyramid:
                SpawnPyramidAt(basePos);
                break;
                
            case PatternType.Wall:
                SpawnWallAt(basePos);
                break;
                
            case PatternType.Spiral:
                SpawnSpiralAt(basePos);
                break;
                
            case PatternType.Cross:
                SpawnCrossAt(basePos);
                break;
                
            case PatternType.Diamond:
                SpawnDiamondAt(basePos);
                break;
                
            case PatternType.Square:
                SpawnSquareAt(basePos);
                break;
                
            case PatternType.SineWave:
                SpawnSineWaveAt(basePos);
                break;
                
            case PatternType.CosineWave:
                SpawnCosineWaveAt(basePos);
                break;
                
            case PatternType.LogarithmicSpiral:
                SpawnLogarithmicSpiralAt(basePos);
                break;
                
            case PatternType.FibonacciSpiral:
                SpawnFibonacciSpiralAt(basePos);
                break;
                
            case PatternType.WavePattern:
                SpawnWavePatternAt(basePos);
                break;
        }
        
        // Ensure there are safe anchor points in upper areas for collectible access
        EnsureUpperAreaAccess(basePos);
    }
    
    void EnsureUpperAreaAccess(Vector3 basePos)
    {
        // Check if we need to add safe anchor points in the upper area
        float upperAreaY = (minHeight + maxHeight) * 0.7f; // Upper 30% of spawn area
        Vector3 upperCheckPos = new Vector3(basePos.x, upperAreaY, 0);
        
        // Look for safe anchors in the upper area
        Collider2D[] upperAnchors = Physics2D.OverlapCircleAll(upperCheckPos, 10f);
        int safeUpperAnchors = 0;
        
        foreach (Collider2D col in upperAnchors)
        {
            bool isSafeAnchor = col.name.ToLower().Contains("anchor") && !col.name.ToLower().Contains("death");
            if (isSafeAnchor && col.transform.position.y >= upperAreaY)
            {
                safeUpperAnchors++;
            }
        }
        
        // If there are fewer than 2 safe anchors in the upper area, add some
        if (safeUpperAnchors < 2)
        {
            // Add a few safe anchor points in the upper area
            for (int i = 0; i < 2 - safeUpperAnchors; i++)
            {
                float randomX = basePos.x + Random.Range(-5f, 5f);
                float randomY = upperAreaY + Random.Range(0f, 3f);
                Vector3 safeAnchorPos = new Vector3(randomX, randomY, 0);
                
                // Spawn a safe anchor (not a hazard)
                GameObject safeAnchor = Instantiate(anchorPrefab, safeAnchorPos, Quaternion.identity);
                ApplyRandomSprite(safeAnchor, anchorSprites);
                safeAnchor.tag = "Untagged"; // Ensure it's not a hazard
                
                // Ensure safe anchor is in front of background
                SpriteRenderer safeSR = safeAnchor.GetComponent<SpriteRenderer>();
                if (safeSR != null)
                {
                    safeSR.sortingOrder = 5; // In front of background
                }
                
            }
        }
    }
    
    void SpawnSingleAnchor()
    {
        Vector3 pos = new Vector3(mainCamera.transform.position.x + anchorAheadDistance, Random.Range(minHeight, maxHeight), 0);
        SpawnSingleAnchorAt(pos);
    }
    
    void SpawnSingleAnchorAt(Vector3 position)
    {
        GameObject anchor = Instantiate(anchorPrefab, position, Quaternion.identity);
        
        // Apply random sprite if available
        ApplyRandomSprite(anchor, anchorSprites);
        
        // Ensure anchor is in front of background
        SpriteRenderer sr = anchor.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = 5; // In front of background (0) but behind UI
            Debug.Log($"[EnhancedSpawner] Set anchor sorting order to 5 for {anchor.name}");
        }
        else
        {
            Debug.LogWarning($"[EnhancedSpawner] No SpriteRenderer found on anchor {anchor.name}!");
        }
        
        // Make it a hazard sometimes
        if (Random.value < hazardChance)
        {
            anchor.tag = "Death";
            // Change color to red to indicate hazard
            if (sr != null)
            {
                sr.color = Color.red;
            }
        }
        
    }
    
    void SpawnClusterAt(Vector3 basePosition)
    {
        // Create a tight hexagonal cluster with touching anchors
        int ringCount = Random.Range(2, 5);
        float anchorSize = 0.95f; // Very tight spacing for touching cubes
        
        for (int ring = 0; ring < ringCount; ring++)
        {
            int anchorsInRing = ring == 0 ? 1 : ring * 6; // Center + hexagonal rings
            float radius = ring * anchorSize;
            
            for (int i = 0; i < anchorsInRing; i++)
            {
                Vector3 offset;
                if (ring == 0)
                {
                    // Center anchor
                    offset = Vector3.zero;
                }
                else
                {
                    // Hexagonal ring - tight and touching
                    float angle = (i / (float)anchorsInRing) * 360f * Mathf.Deg2Rad;
                    offset = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                }
                SpawnSingleAnchorAt(basePosition + offset);
            }
        }
        
        // Add some random scattered anchors around the cluster for more variety
        int extraAnchors = Random.Range(2, 5);
        for (int i = 0; i < extraAnchors; i++)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDistance = Random.Range(1.2f, 2.8f);
            Vector3 randomOffset = new Vector3(Mathf.Cos(randomAngle) * randomDistance, Mathf.Sin(randomAngle) * randomDistance, 0);
            SpawnSingleAnchorAt(basePosition + randomOffset);
        }
    }
    
    void SpawnLineAt(Vector3 basePosition)
    {
        int count = Random.Range(6, 12);
        float spacing = 0.95f; // Very tight spacing for touching cubes
        
        // Create solid connected line patterns with intentional gaps for variation
        for (int i = 0; i < count; i++)
        {
            // Skip some positions to create gaps (variation)
            if (Random.value < 0.15f) continue;
            
            // Create wave patterns
            float heightVariation = Mathf.Sin(i * 0.3f) * 1.5f + Mathf.Cos(i * 0.15f) * 0.8f;
            Vector3 pos = basePosition + Vector3.right * (i * spacing) + Vector3.up * heightVariation;
            SpawnSingleAnchorAt(pos);
        }
        
        // Add some branching lines for more variety
        int branchCount = Random.Range(1, 3);
        for (int b = 0; b < branchCount; b++)
        {
            int branchStart = Random.Range(2, count - 2);
            int branchLength = Random.Range(3, 6);
            float branchAngle = Random.Range(-45f, 45f) * Mathf.Deg2Rad;
            
            for (int i = 0; i < branchLength; i++)
            {
                Vector3 branchOffset = new Vector3(Mathf.Cos(branchAngle) * i * spacing, Mathf.Sin(branchAngle) * i * spacing, 0);
                Vector3 branchPos = basePosition + Vector3.right * (branchStart * spacing) + branchOffset;
                SpawnSingleAnchorAt(branchPos);
            }
        }
    }
    
    void SpawnStairsAt(Vector3 basePosition)
    {
        int count = Random.Range(5, 10);
        float stepHeight = 0.95f; // Very tight vertical spacing
        float stepWidth = 0.95f;  // Very tight horizontal spacing
        
        // Create solid connected stair patterns with intentional gaps for variation
        for (int i = 0; i < count; i++)
        {
            // Skip some steps to create gaps (variation)
            if (Random.value < 0.1f) continue;
            
            float heightOffset = i * stepHeight;
            float widthOffset = i * stepWidth;
            Vector3 pos = basePosition + Vector3.right * widthOffset + Vector3.up * heightOffset;
            SpawnSingleAnchorAt(pos);
            
            // Sometimes add double steps for more variety
            if (Random.value < 0.3f && i < count - 1)
            {
                Vector3 doubleStepPos = pos + Vector3.right * 0.95f;
                SpawnSingleAnchorAt(doubleStepPos);
            }
        }
    }
    
    void SpawnPyramidAt(Vector3 basePosition)
    {
        int levels = Random.Range(3, 6);
        float levelHeight = 0.95f; // Very tight vertical spacing
        float levelWidth = 0.95f;  // Very tight horizontal spacing
        
        for (int level = 0; level < levels; level++)
        {
            int anchorsInLevel = levels - level;
            float y = basePosition.y + (level * levelHeight);
            
            for (int i = 0; i < anchorsInLevel; i++)
            {
                // Skip some positions to create gaps (variation)
                if (Random.value < 0.1f) continue;
                
                float x = basePosition.x + (i - (anchorsInLevel - 1) * 0.5f) * levelWidth;
                Vector3 pos = new Vector3(x, y, 0);
                SpawnSingleAnchorAt(pos);
            }
        }
        
        // Add some scattered anchors around the pyramid for more variety
        int extraAnchors = Random.Range(2, 4);
        for (int i = 0; i < extraAnchors; i++)
        {
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float randomDistance = Random.Range(1.5f, 3f);
            Vector3 randomOffset = new Vector3(Mathf.Cos(randomAngle) * randomDistance, Mathf.Sin(randomAngle) * randomDistance, 0);
            SpawnSingleAnchorAt(basePosition + randomOffset);
        }
    }
    
    void SpawnWallAt(Vector3 basePosition)
    {
        int width = Random.Range(4, 8);
        int height = Random.Range(3, 6);
        float spacing = 0.95f; // Very tight spacing for touching cubes
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Skip some positions to create gaps (variation)
                if (Random.value < 0.1f) continue;
                
                float xOffset = x * spacing;
                float yOffset = y * spacing;
                Vector3 pos = basePosition + Vector3.right * xOffset + Vector3.up * yOffset;
                SpawnSingleAnchorAt(pos);
            }
        }
    }
    
    void SpawnSpiralAt(Vector3 basePosition)
    {
        int count = Random.Range(8, 12);
        float spiralRadius = 0.5f;
        float spiralGrowth = 0.4f; // Tighter spiral growth for touching cubes
        
        for (int i = 0; i < count; i++)
        {
            // Skip some positions to create gaps (variation)
            if (Random.value < 0.1f) continue;
            
            float angle = i * 0.5f; // Tighter spiral angle
            float radius = spiralRadius + (i * spiralGrowth);
            Vector3 offset = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            SpawnSingleAnchorAt(basePosition + offset);
        }
    }
    
    void SpawnCrossAt(Vector3 basePosition)
    {
        int size = Random.Range(3, 5);
        float spacing = 0.95f; // Very tight spacing for touching cubes
        
        // Horizontal line
        for (int x = -size; x <= size; x++)
        {
            // Skip some positions to create gaps (variation)
            if (Random.value < 0.1f) continue;
            
            Vector3 pos = basePosition + Vector3.right * (x * spacing);
            SpawnSingleAnchorAt(pos);
        }
        
        // Vertical line (skip center to avoid duplicate)
        for (int y = -size; y <= size; y++)
        {
            if (y != 0) // Skip center since it's already placed
            {
                // Skip some positions to create gaps (variation)
                if (Random.value < 0.1f) continue;
                
                Vector3 pos = basePosition + Vector3.up * (y * spacing);
                SpawnSingleAnchorAt(pos);
            }
        }
    }
    
    void SpawnDiamondAt(Vector3 basePosition)
    {
        int size = Random.Range(2, 4);
        float spacing = 0.95f; // Very tight spacing for touching cubes
        
        // Create diamond shape
        for (int y = 0; y <= size; y++)
        {
            int width = size - y;
            for (int x = -width; x <= width; x++)
            {
                // Skip some positions to create gaps (variation)
                if (Random.value < 0.1f) continue;
                
                // Top half
                Vector3 pos = basePosition + Vector3.right * (x * spacing) + Vector3.up * (y * spacing);
                SpawnSingleAnchorAt(pos);
                
                // Bottom half (skip center line)
                if (y > 0)
                {
                    Vector3 posBottom = basePosition + Vector3.right * (x * spacing) + Vector3.up * (-y * spacing);
                    SpawnSingleAnchorAt(posBottom);
                }
            }
        }
    }
    
    void SpawnSquareAt(Vector3 basePosition)
    {
        int size = Random.Range(3, 5);
        float spacing = 0.95f; // Very tight spacing for touching cubes
        
        // Create solid square with intentional gaps for variation
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                // Skip some positions to create gaps (variation)
                if (Random.value < 0.1f) continue;
                
                Vector3 pos = basePosition + Vector3.right * (x * spacing) + Vector3.up * (y * spacing);
                SpawnSingleAnchorAt(pos);
            }
        }
    }
    
    void SpawnSineWaveAt(Vector3 basePosition)
    {
        // Create a sine wave pattern of anchor points
        float amplitude = Random.Range(2f, 4f); // Wave height
        float frequency = Random.Range(0.5f, 1.5f); // How many waves
        float spacing = Random.Range(0.8f, 1.2f); // Distance between points
        int pointCount = Random.Range(8, 15); // Number of anchor points
        
        for (int i = 0; i < pointCount; i++)
        {
            float x = i * spacing;
            float y = Mathf.Sin(x * frequency) * amplitude;
            Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
            SpawnSingleAnchorAt(pos);
        }
    }
    
    void SpawnCosineWaveAt(Vector3 basePosition)
    {
        // Create a cosine wave pattern (shifted sine wave)
        float amplitude = Random.Range(2f, 4f);
        float frequency = Random.Range(0.5f, 1.5f);
        float spacing = Random.Range(0.8f, 1.2f);
        int pointCount = Random.Range(8, 15);
        
        for (int i = 0; i < pointCount; i++)
        {
            float x = i * spacing;
            float y = Mathf.Cos(x * frequency) * amplitude;
            Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
            SpawnSingleAnchorAt(pos);
        }
    }
    
    void SpawnLogarithmicSpiralAt(Vector3 basePosition)
    {
        // Create a logarithmic spiral using polar coordinates
        float a = Random.Range(0.1f, 0.3f); // Growth rate
        float b = Random.Range(0.5f, 1.5f); // Tightness
        int pointCount = Random.Range(12, 20);
        
        for (int i = 0; i < pointCount; i++)
        {
            float angle = i * 0.5f; // Angle in radians
            float radius = a * Mathf.Exp(b * angle); // Logarithmic spiral formula
            
            // Convert polar to cartesian
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            
            Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
            SpawnSingleAnchorAt(pos);
        }
    }
    
    void SpawnFibonacciSpiralAt(Vector3 basePosition)
    {
        // Create a Fibonacci spiral approximation using golden ratio
        float goldenRatio = 1.618f;
        int pointCount = Random.Range(10, 16);
        float scale = Random.Range(0.5f, 1.0f);
        
        for (int i = 0; i < pointCount; i++)
        {
            // Approximate Fibonacci spiral using golden ratio
            float angle = i * goldenRatio * 0.5f;
            float radius = Mathf.Pow(goldenRatio, i * 0.1f) * scale;
            
            // Convert polar to cartesian
            float x = radius * Mathf.Cos(angle);
            float y = radius * Mathf.Sin(angle);
            
            Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
            SpawnSingleAnchorAt(pos);
        }
    }
    
    void SpawnWavePatternAt(Vector3 basePosition)
    {
        // Create a complex wave pattern combining multiple sine waves
        float amplitude1 = Random.Range(1.5f, 3f);
        float frequency1 = Random.Range(0.3f, 0.8f);
        float amplitude2 = Random.Range(0.5f, 1.5f);
        float frequency2 = Random.Range(1.0f, 2.0f);
        float spacing = Random.Range(0.6f, 1.0f);
        int pointCount = Random.Range(10, 18);
        
        for (int i = 0; i < pointCount; i++)
        {
            float x = i * spacing;
            // Combine two sine waves for a more complex pattern
            float y = (Mathf.Sin(x * frequency1) * amplitude1) + (Mathf.Sin(x * frequency2) * amplitude2);
            Vector3 pos = basePosition + Vector3.right * x + Vector3.up * y;
            SpawnSingleAnchorAt(pos);
        }
    }
    
    void SpawnCollectible()
    {
        if (collectiblePrefab == null) return;
        
        Vector3 pos = FindSafeCollectiblePosition();
        if (pos == Vector3.zero) return; // No safe position found
        
        Quaternion rotation = Quaternion.Euler(0, 0, 90); // Rotate 90 degrees on Z axis for upright diamonds
        Instantiate(collectiblePrefab, pos, rotation);
        
    }
    
    Vector3 FindSafeCollectiblePosition()
    {
        float spawnX = mainCamera.transform.position.x + 25f;
        int maxAttempts = 15; // Increased attempts for more complex validation
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float randomY = Random.Range(minHeight, maxHeight);
            Vector3 testPos = new Vector3(spawnX, randomY, 0);
            
            // Check if this position is safe from anchor points AND has a grappleable anchor nearby
            if (IsPositionSafeFromAnchors(testPos, collectibleMinDistanceFromAnchors) && 
                HasGrappleableAnchorNearby(testPos, collectibleMaxDistanceFromAnchors))
            {
                return testPos;
            }
        }
        
        // If no ideal position found, try to find any position with a nearby anchor
        for (int attempt = 0; attempt < 10; attempt++)
        {
            float randomY = Random.Range(minHeight, maxHeight);
            Vector3 testPos = new Vector3(spawnX, randomY, 0);
            
            // Just check for nearby anchor, ignore minimum distance for fallback
            if (HasGrappleableAnchorNearby(testPos, collectibleMaxDistanceFromAnchors))
            {
                return testPos;
            }
        }
        
        // Last resort: spawn at middle height
        return new Vector3(spawnX, (minHeight + maxHeight) * 0.5f, 0);
    }
    
    bool IsPositionSafeFromAnchors(Vector3 position, float minDistance)
    {
        // Find all colliders within a reasonable range
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, minDistance * 2f);
        
        foreach (Collider2D col in nearbyColliders)
        {
            // Check if it's any type of anchor or hazard (by name only)
            bool isAnchor = col.name.ToLower().Contains("anchor") || 
                           col.name.ToLower().Contains("hazard") ||
                           col.name.ToLower().Contains("spike") ||
                           col.name.ToLower().Contains("death");
            
            if (isAnchor)
            {
                float distance = Vector2.Distance(position, col.transform.position);
                if (distance < minDistance)
                {
                    return false; // Too close to an anchor point
                }
            }
        }
        
        return true; // Safe position
    }
    
    bool HasGrappleableAnchorNearby(Vector3 position, float maxDistance)
    {
        // Find all anchor points within the maximum grapple distance
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(position, maxDistance);
        
        List<Vector3> safeAnchors = new List<Vector3>();
        List<Vector3> hazardAnchors = new List<Vector3>();
        
        foreach (Collider2D col in nearbyColliders)
        {
            // Check if it's an anchor point (by name)
            bool isAnchor = col.name.ToLower().Contains("anchor") || 
                           col.name.ToLower().Contains("hazard") ||
                           col.name.ToLower().Contains("spike") ||
                           col.name.ToLower().Contains("death");
            
            if (isAnchor)
            {
                float distance = Vector2.Distance(position, col.transform.position);
                if (distance <= maxDistance)
                {
                    // Check if the anchor is above or at the same level as the collectible
                    if (col.transform.position.y >= position.y - 1f) // Allow slight tolerance
                    {
                        if (col.name.ToLower().Contains("death"))
                        {
                            hazardAnchors.Add(col.transform.position);
                        }
                        else
                        {
                            safeAnchors.Add(col.transform.position);
                        }
                    }
                }
            }
        }
        
        // Prioritize safe anchors over hazard anchors
        if (safeAnchors.Count > 0)
        {
            return true;
        }
        
        // If no safe anchors, check if we can use hazard anchors with clear path
        if (hazardAnchors.Count > 0 && !ensureClearPath)
        {
            return true;
        }
        
        // If clear path is required, check for safe approach paths
        if (hazardAnchors.Count > 0 && ensureClearPath)
        {
            return HasClearPathToCollectible(position, hazardAnchors);
        }
        
        return false; // No grappleable anchor found
    }
    
    bool HasClearPathToCollectible(Vector3 collectiblePos, List<Vector3> hazardAnchors)
    {
        // Check if there are safe anchor points that can be used to approach the collectible
        // Look for anchors that are further away but provide a safe approach path
        Collider2D[] allAnchors = Physics2D.OverlapCircleAll(collectiblePos, collectibleMaxDistanceFromAnchors * 1.5f);
        
        foreach (Collider2D col in allAnchors)
        {
            bool isSafeAnchor = col.name.ToLower().Contains("anchor") && !col.name.ToLower().Contains("death");
            
            if (isSafeAnchor)
            {
                Vector3 anchorPos = col.transform.position;
                float distanceToCollectible = Vector2.Distance(anchorPos, collectiblePos);
                
                // Check if this safe anchor can reach the collectible
                if (distanceToCollectible <= collectibleMaxDistanceFromAnchors)
                {
                    // Check if there's a clear path (no hazards blocking the approach)
                    if (IsPathClear(anchorPos, collectiblePos))
                    {
                        return true;
                    }
                }
            }
        }
        
        return false;
    }
    
    bool IsPathClear(Vector3 from, Vector3 to)
    {
        // Cast a ray to check for hazards blocking the path
        Vector2 direction = (to - from).normalized;
        float distance = Vector2.Distance(from, to);
        
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, direction, distance);
        
        foreach (RaycastHit2D hit in hits)
        {
            // Check if there are hazard anchors blocking the path
            if (hit.collider.name.ToLower().Contains("death") && hit.collider.name.ToLower().Contains("anchor"))
            {
                // If there's a hazard anchor in the direct path, it's not clear
                return false;
            }
        }
        
        return true;
    }
    
    
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
    
    void OnDrawGizmosSelected()
    {
        // Draw spawn areas
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + Vector3.right * 15f + Vector3.up * (minHeight + maxHeight) / 2f, 
                           new Vector3(5f, maxHeight - minHeight, 0));
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + Vector3.right * 25f + Vector3.up * (minHeight + maxHeight) / 2f, 
                           new Vector3(5f, maxHeight - minHeight, 0));
    }
    
    // Multi-image sprite system methods
    void ApplyRandomSprite(GameObject obj, Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) 
        {
            Debug.LogWarning("[EnhancedSpawner] Sprite array is null or empty!");
            // Fallback to prefab sprite if available
            if (anchorPrefab != null)
            {
                SpriteRenderer prefabSR = anchorPrefab.GetComponent<SpriteRenderer>();
                if (prefabSR != null && prefabSR.sprite != null)
                {
                    SpriteRenderer objSR = obj.GetComponent<SpriteRenderer>();
                    if (objSR != null)
                    {
                        objSR.sprite = prefabSR.sprite;
                        Debug.Log($"[EnhancedSpawner] Using prefab sprite as fallback: {prefabSR.sprite.name}");
                    }
                }
            }
            return;
        }
        
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // Filter out null sprites and create a list of valid sprites
            System.Collections.Generic.List<Sprite> validSprites = new System.Collections.Generic.List<Sprite>();
            for (int i = 0; i < sprites.Length; i++)
            {
                if (sprites[i] != null)
                {
                    validSprites.Add(sprites[i]);
                }
                else
                {
                    Debug.LogWarning($"[EnhancedSpawner] Found null sprite at index {i}, skipping");
                }
            }
            
            Sprite randomSprite = null;
            
            // Select from valid sprites only
            if (validSprites.Count > 0)
            {
                randomSprite = validSprites[Random.Range(0, validSprites.Count)];
                Debug.Log($"[EnhancedSpawner] Selected from {validSprites.Count} valid sprites: {randomSprite.name}");
            }
            else
            {
                Debug.LogWarning("[EnhancedSpawner] No valid sprites found in array, using prefab sprite as fallback");
                // Use prefab sprite as fallback
                if (anchorPrefab != null)
                {
                    SpriteRenderer prefabSR = anchorPrefab.GetComponent<SpriteRenderer>();
                    if (prefabSR != null && prefabSR.sprite != null)
                    {
                        randomSprite = prefabSR.sprite;
                        Debug.Log($"[EnhancedSpawner] Using prefab sprite: {prefabSR.sprite.name}");
                    }
                }
            }
            
            if (randomSprite != null)
            {
                sr.sprite = randomSprite;
                Debug.Log($"[EnhancedSpawner] Applied sprite: {randomSprite.name} to {obj.name}");
            }
            else
            {
                Debug.LogError($"[EnhancedSpawner] No valid sprite available for {obj.name}!");
            }
            
            // Copy scale from anchor prefab and scale up 6x for better visibility
            Vector3 finalScale;
            if (anchorPrefab != null)
            {
                Vector3 prefabScale = anchorPrefab.transform.localScale;
                finalScale = prefabScale * 6f; // 6x larger
            }
            else
            {
                finalScale = Vector3.one * 6f; // Default 6x scale
            }
            
            obj.transform.localScale = finalScale;
            
            // Colliders will automatically scale with the transform, no manual adjustment needed
            Debug.Log($"[EnhancedSpawner] Scaled anchor to {finalScale} - colliders scale automatically");
        }
        else
        {
            Debug.LogWarning($"[EnhancedSpawner] No SpriteRenderer found on {obj.name}!");
        }
    }
    
    void SpawnRandomPlanet()
    {
        if (!enablePlanets || planetSprites == null || planetSprites.Length == 0) return;
        
        if (Random.value < planetSpawnChance)
        {
            float cameraX = mainCamera.transform.position.x;
            float spawnX = cameraX + Random.Range(15f, planetAheadDistance);
            float spawnY = Random.Range(planetHeightRange.x, planetHeightRange.y);
            float horizontalOffset = Random.Range(planetHorizontalRange.x, planetHorizontalRange.y);
            Vector3 planetPos = new Vector3(spawnX + horizontalOffset, spawnY, 5f); // Z=5 for background layer
            
            // Create a planet GameObject
            GameObject planet = new GameObject("Planet");
            planet.transform.position = planetPos;
            
            // Add SpriteRenderer
            SpriteRenderer sr = planet.AddComponent<SpriteRenderer>();
            sr.sprite = planetSprites[Random.Range(0, planetSprites.Length)];
            sr.sortingOrder = 5; // Background layer, behind everything else
            
            // Add random rotation for variety
            planet.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            
            // Add random scale for variety
            float randomScale = Random.Range(planetScaleRange.x, planetScaleRange.y);
            planet.transform.localScale = Vector3.one * randomScale;
            
            // Add subtle movement for more dynamic feel
            StartCoroutine(AnimatePlanet(planet));
        }
    }
    
    System.Collections.IEnumerator AnimatePlanet(GameObject planet)
    {
        if (planet == null) yield break;
        
        Vector3 startPos = planet.transform.position;
        float moveSpeed = Random.Range(0.5f, 2f);
        float moveDistance = Random.Range(1f, 3f);
        
        while (planet != null)
        {
            // Gentle floating motion
            float time = Time.time * moveSpeed;
            Vector3 offset = new Vector3(0, Mathf.Sin(time) * moveDistance * 0.1f, 0);
            planet.transform.position = startPos + offset;
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Spawn random decorations in the background
    /// </summary>
    void SpawnRandomDecoration()
    {
        if (!enableDecorations || decorationSprites == null || decorationSprites.Length == 0) return;
        
        if (Random.value < decorationSpawnChance)
        {
            float cameraX = mainCamera.transform.position.x;
            float spawnX = cameraX + Random.Range(15f, decorationAheadDistance);
            float spawnY = Random.Range(decorationHeightRange.x, decorationHeightRange.y);
            float horizontalOffset = Random.Range(decorationHorizontalRange.x, decorationHorizontalRange.y);
            Vector3 decorationPos = new Vector3(spawnX + horizontalOffset, spawnY, 4f); // Z=4 for decoration layer
            
            // Create a decoration GameObject
            GameObject decoration = new GameObject("Decoration");
            decoration.transform.position = decorationPos;
            
            // Add SpriteRenderer
            SpriteRenderer sr = decoration.AddComponent<SpriteRenderer>();
            sr.sprite = decorationSprites[Random.Range(0, decorationSprites.Length)];
            sr.sortingOrder = decorationSortingOrder; // Configurable sorting order
            
            // Add random rotation for variety
            decoration.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
            
            // Add random scale for variety
            float randomScale = Random.Range(decorationScaleRange.x, decorationScaleRange.y);
            decoration.transform.localScale = Vector3.one * randomScale;
            
            // Add subtle movement for more dynamic feel
            StartCoroutine(AnimateDecoration(decoration));
        }
    }
    
    /// <summary>
    /// Animate decoration with subtle movement
    /// </summary>
    System.Collections.IEnumerator AnimateDecoration(GameObject decoration)
    {
        if (decoration == null) yield break;
        
        Vector3 startPos = decoration.transform.position;
        float moveSpeed = Random.Range(0.3f, 1.5f);
        float moveDistance = Random.Range(0.5f, 2f);
        
        while (decoration != null)
        {
            // Gentle floating motion
            float time = Time.time * moveSpeed;
            Vector3 offset = new Vector3(0, Mathf.Sin(time) * moveDistance * 0.1f, 0);
            decoration.transform.position = startPos + offset;
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Spawn a background tile with random sprite
    /// </summary>
    void SpawnRandomBackgroundTile(Vector3 position)
    {
        // Create background tile GameObject
        GameObject backgroundTile = new GameObject("BackgroundTile");
        backgroundTile.transform.position = position;
        
        // Calculate scale to fit camera view with some padding
        if (mainCamera != null)
        {
            // Get camera's orthographic size and aspect ratio
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;
            
            // Add some padding (20% larger than camera view)
            float targetWidth = cameraWidth * 1.2f;
            float targetHeight = cameraHeight * 1.2f;
            
            // Get the sprite's original size
            SpriteRenderer prefabSR = backgroundPrefab?.GetComponent<SpriteRenderer>();
            if (prefabSR != null && prefabSR.sprite != null)
            {
                float spriteWidth = prefabSR.sprite.bounds.size.x;
                float spriteHeight = prefabSR.sprite.bounds.size.y;
                
                // Calculate scale needed to fit camera view
                float scaleX = targetWidth / spriteWidth;
                float scaleY = targetHeight / spriteHeight;
                
                // Use the larger scale to ensure full coverage
                float scale = Mathf.Max(scaleX, scaleY);
                backgroundTile.transform.localScale = Vector3.one * scale;
                
                Debug.Log($"[EnhancedSpawner] Background scale calculated: {scale:F2} (Camera: {cameraWidth:F1}x{cameraHeight:F1}, Sprite: {spriteWidth:F1}x{spriteHeight:F1})");
            }
            else if (backgroundPrefab != null)
            {
                // Fallback to prefab scale if no sprite info
                backgroundTile.transform.localScale = backgroundPrefab.transform.localScale;
            }
        }
        else if (backgroundPrefab != null)
        {
            // Fallback to prefab scale if no camera
            backgroundTile.transform.localScale = backgroundPrefab.transform.localScale;
        }
        
        // Add SpriteRenderer
        SpriteRenderer sr = backgroundTile.AddComponent<SpriteRenderer>();
        
        // Use random background sprite if available, otherwise use prefab
        if (backgroundSprites != null && backgroundSprites.Length > 0)
        {
            sr.sprite = backgroundSprites[Random.Range(0, backgroundSprites.Length)];
            Debug.Log($"[EnhancedSpawner] Applied random background sprite: {sr.sprite.name}");
        }
        else if (backgroundPrefab != null)
        {
            // Fallback to prefab if no sprites are assigned
            SpriteRenderer prefabSR = backgroundPrefab.GetComponent<SpriteRenderer>();
            if (prefabSR != null)
            {
                sr.sprite = prefabSR.sprite;
                Debug.Log("[EnhancedSpawner] Using background prefab sprite (no sprites array assigned)");
            }
        }
        else
        {
            Debug.LogWarning("[EnhancedSpawner] No background sprites or prefab assigned!");
        }
        
        // Set sorting order for background (behind everything)
        sr.sortingOrder = 0; // Background layer
        Debug.Log($"[EnhancedSpawner] Set background sorting order to 0 for {backgroundTile.name}");
    }
    
    // Upgrade system methods
    private float hazardSpawnRateMultiplier = 1f;
    private float collectibleValueMultiplier = 1f;
    
    public void SetHazardSpawnRateMultiplier(float multiplier)
    {
        hazardSpawnRateMultiplier = Mathf.Clamp(multiplier, 0.1f, 2f);
    }
    
    public void SetCollectibleValueMultiplier(float multiplier)
    {
        collectibleValueMultiplier = Mathf.Clamp(multiplier, 0.5f, 3f);
    }
    
    public void SetAnchorClusterSpacing(float spacing)
    {
        minAnchorDistance = Mathf.Clamp(spacing, 2f, 20f);
        maxAnchorDistance = minAnchorDistance + 5f;
    }
    
    public void SetCollectibleSpawnDistance(float distance)
    {
        minCollectibleDistance = Mathf.Clamp(distance, 5f, 50f);
        maxCollectibleDistance = minCollectibleDistance + 10f;
    }
    
    public void SetAnchorDensityMultiplier(float multiplier)
    {
        anchorDensityMultiplier = Mathf.Clamp(multiplier, 0.1f, 3f);
    }
    
    public float GetCollectibleValueMultiplier()
    {
        return collectibleValueMultiplier;
    }
    
    /// <summary>
    /// Set the collectible sound for all collectibles
    /// </summary>
    public void SetCollectibleSound(AudioClip sound)
    {
        collectibleSound = sound;
        if (sound != null)
        {
            EnhancedCollectible.SetGlobalCollectSound(sound);
            Debug.Log($"[EnhancedSpawner] Collectible sound set to: {sound.name}");
        }
    }
}
