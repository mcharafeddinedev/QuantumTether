# Game Manager System Setup - Reference Guide

## What This Document Contains

This document explains how to build a central game manager that controls the overall game state and coordinates all other systems. The game manager acts as a central hub that other systems can communicate with through events.

## The Game Manager Overview

### What Each Part Does
- **EnhancedGameManager.cs** - Manages overall game state (playing, dead, restart)
- **EnhancedBootstrap.cs** - Ensures the game manager exists before scenes load
- **Event system** - Allows systems to communicate without direct references

### How It Works
1. **Bootstrap** creates the game manager before any scene loads
2. **Game Manager** tracks game state and broadcasts events when things change
3. **Other Systems** listen to events instead of directly referencing the game manager
4. **Scene Transitions** are handled centrally with proper state resets

## How to Build Your Own Game Manager (Step by Step)

### Step 1: Create the Bootstrap
This script ensures the game manager exists before any scene loads. It runs automatically when the game starts.

```csharp
public class EnhancedBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureGameManager()
    {
        // Don't create duplicate if one already exists
        if (EnhancedGameManager.Instance != null) return;

        // Create the game manager
        var go = new GameObject("EnhancedGameManager");
        go.AddComponent<EnhancedGameManager>();
    }
}
```

### Step 2: Create the Game Manager
```csharp
// EnhancedGameManager.cs - Central game state management
public class EnhancedGameManager : MonoBehaviour
{
    [Header("Game State")]
    public static EnhancedGameManager Instance;
    
    [SerializeField, Tooltip("Current game state")]
    private bool isDead = false;
    
    // Events for loose coupling
    public static event Action OnPlayerDeath;
    public static event Action OnGameRestart;
    public static event Action OnGameStart;
    
    public bool IsDead => isDead;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnGameStart?.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Reset everything when the game scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        // Clean up when this gets destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When the game scene loads, reset the death state
        if (scene.name == "Game" || scene.name.ToLower().Contains("game"))
        {
            isDead = false;
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        isDead = true;
        OnPlayerDeath?.Invoke();
        
        // The death UI will handle showing the restart options
    }
    
    public void Restart()
    {
        isDead = false;
        OnGameRestart?.Invoke();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
```

### Step 3: Subscribe to Events in Other Systems
```csharp
// In any script that needs to respond to game events
public class SomeGameSystem : MonoBehaviour
{
    void Start()
    {
        // Subscribe to game events
        EnhancedGameManager.OnPlayerDeath += OnPlayerDeath;
        EnhancedGameManager.OnGameRestart += OnGameRestart;
    }
    
    void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        EnhancedGameManager.OnPlayerDeath -= OnPlayerDeath;
        EnhancedGameManager.OnGameRestart -= OnGameRestart;
    }
    
    void OnPlayerDeath()
    {
        // Handle player death
        // Stop systems, show UI, etc.
    }
    
    void OnGameRestart()
    {
        // Reset for new game
        // Clear data, reset positions, etc.
    }
}
```

## Key Patterns I Used

### 1. Singleton Pattern
```csharp
// Ensures only one GameManager exists
void Awake()
{
    if (Instance == null)
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    else
    {
        Destroy(gameObject);
    }
}
```

### 2. Event-Driven Architecture
```csharp
// Broadcast events instead of direct references
public static event Action OnPlayerDeath;
public static event Action OnGameRestart;

// Other systems subscribe to events
EnhancedGameManager.OnPlayerDeath += OnPlayerDeath;
```

### 3. Bootstrap Pattern
```csharp
// Ensures GameManager exists before any scene loads
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
private static void EnsureGameManager()
{
    if (EnhancedGameManager.Instance != null) return;
    // Create GameManager
}
```

### 4. Scene State Management
```csharp
// Reset state when game scene loads
void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    if (scene.name == "Game" || scene.name.ToLower().Contains("game"))
    {
        isDead = false;
    }
}
```

## What I Learned

### Good Practices
- **Use events for communication** instead of direct references
- **Always unsubscribe from events** to prevent memory leaks
- **Reset state on scene load** to prevent bugs
- **Use bootstrap pattern** to ensure proper initialization order
- **Keep GameManager focused** on core game state only

### Common Pitfalls
- **Don't forget to unsubscribe** from events (memory leaks)
- **Don't put too much logic** in GameManager (keep it focused)
- **Don't forget the bootstrap** (GameManager won't exist)
- **Don't forget scene state resets** (bugs carry over between games)

### Performance Tips
- **Events are lightweight** - use them liberally
- **Singleton pattern is efficient** - only one instance
- **Scene loading is expensive** - minimize scene transitions
- **Event subscriptions are fast** - don't worry about performance

## Integration Patterns

### With Audio System
```csharp
// GameManager triggers audio events
public void Die()
{
    isDead = true;
    OnPlayerDeath?.Invoke();
    // Audio system will hear this and play death sound
}
```

### With UI System
```csharp
// UI responds to game state changes
void OnPlayerDeath()
{
    ShowDeathPanel();
}

void OnGameRestart()
{
    HideDeathPanel();
    ResetUI();
}
```

### With Player Systems
```csharp
// Player systems check game state
void Update()
{
    // Don't allow input if player is dead
    if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
    {
        return;
    }
    
    // Handle input normally
}
```

## Event System Benefits

### Loose Coupling
- Systems don't need direct references to each other
- Easy to add new systems without modifying existing code
- Systems can be tested independently

### Extensibility
- Easy to add new death-triggered behaviors
- Easy to add new restart-triggered behaviors
- Easy to add new game start behaviors

### Maintainability
- Clear separation of concerns
- Easy to understand what happens when
- Easy to debug event flow

## Future Improvements

### What I Could Add Next Time
- **More game events** (OnLevelComplete, OnPowerUpCollected, etc.)
- **Game state machine** for more complex states
- **Save/load system** integration
- **Analytics integration** for tracking game events
- **Debug event logging** for development

### Advanced Features
- **Event priorities** (some events should happen before others)
- **Event queuing** (delay events until safe to process)
- **Event filtering** (only certain systems hear certain events)
- **Event history** (track what events happened when)

## Quick Setup Checklist

For future projects, here's my quick GameManager setup:

1. Create EnhancedBootstrap with RuntimeInitializeOnLoadMethod
2. Create EnhancedGameManager singleton
3. Define game events (OnPlayerDeath, OnGameRestart, etc.)
4. Implement scene state management
5. Add death and restart methods
6. Subscribe other systems to events
7. Test event flow and state resets
8. Add proper cleanup in OnDestroy
9. Test scene transitions work properly
10. Add any additional game events needed

This GameManager system worked really well for Quantum Thread and should work for managing game state in other projects I work on in the future.
