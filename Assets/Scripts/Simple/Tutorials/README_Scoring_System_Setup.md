# Scoring System Setup - Reference Guide

## What This Document Contains

This document explains how to build a scoring system that tracks points, manages death and pause screens, and saves high scores. It covers dynamic scoring based on game speed and how the scoring system connects with other game systems.

## The Scoring System Overview

### What Each Part Does
- **EnhancedScore.cs** - Manages scoring, death panel, and pause screen
- **Speed-based scoring** - Points increase as the game gets faster
- **Death panel** - Shows when the player dies with restart options
- **Pause screen** - Allows players to pause the game
- **Score saving** - Remembers high scores between game sessions

### How It Works
1. **Score Tracking** counts points based on time and speed
2. **Speed Multiplier** makes scoring faster as the game progresses
3. **Death Detection** shows death panel when player dies
4. **Pause System** stops the game when Escape is pressed
5. **Score Persistence** saves and loads high scores automatically

## How to Build Your Own Scoring System (Step by Step)

### Step 1: Create the Basic Scoring System
This script tracks points, manages the death panel, and handles pause functionality.

```csharp
public class EnhancedScore : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private int baseScorePerSecond = 10;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private int collectibleValue = 500;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject pauseScreen;
    
    [Header("Death Panel")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button mainMenuButton;
    
    [Header("Pause Screen")]
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseQuitButton;
    [SerializeField] private Button resumeButton;
    
    private int currentScore = 0;
    private int highScore = 0;
    private bool isScoreFrozen = false;
    private bool isPaused = false;
    
    public static EnhancedScore Instance { get; private set; }
    
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
    
    void Start()
    {
        LoadHighScore();
        UpdateUI();
        SetupButtons();
    }
    
    void Update()
    {
        if (!isScoreFrozen && !isPaused)
        {
            UpdateScore();
        }
        
        HandleEscapeKey();
    }
}
```

### Step 2: Score Calculation and Updates
```csharp
void UpdateScore()
{
    // Get current camera speed for dynamic scoring
    float currentSpeed = EnhancedCamera.Instance?.GetCurrentSpeed() ?? 1f;
    float baseSpeed = EnhancedCamera.Instance?.GetStartSpeed() ?? 1f;
    
    // Calculate speed multiplier (higher speed = more points)
    float speedRatio = currentSpeed / baseSpeed;
    float effectiveMultiplier = speedMultiplier * speedRatio;
    
    // Add points based on speed
    int pointsToAdd = Mathf.RoundToInt(baseScorePerSecond * effectiveMultiplier * Time.deltaTime);
    AddPoints(pointsToAdd);
}

public void AddPoints(int points)
{
    if (isScoreFrozen) return;
    
    currentScore += points;
    
    // Update high score if needed
    if (currentScore > highScore)
    {
        highScore = currentScore;
        SaveHighScore();
    }
    
    UpdateUI();
}

void UpdateUI()
{
    if (scoreText != null)
    {
        scoreText.text = currentScore.ToString("N0");
    }
    
    if (highScoreText != null)
    {
        highScoreText.text = highScore.ToString("N0");
    }
}
```

### Step 3: Death Panel Management
```csharp
public void ShowDeathPanel()
{
    if (deathPanel != null)
    {
        deathPanel.SetActive(true);
        isScoreFrozen = true;
        
        // Show death quote
        DeathQuoteManager.Instance?.ShowRandomQuote();
        
        // Update final score display
        UpdateUI();
    }
}

public void HideDeathPanel()
{
    if (deathPanel != null)
    {
        deathPanel.SetActive(false);
        isScoreFrozen = false;
    }
}

void SetupButtons()
{
    // Death panel buttons
    if (restartButton != null)
    {
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartGame);
    }
    
    if (quitButton != null)
    {
        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitToMainMenu);
    }
    
    if (mainMenuButton != null)
    {
        mainMenuButton.onClick.RemoveAllListeners();
        mainMenuButton.onClick.AddListener(QuitToMainMenu);
    }
    
    // Pause screen buttons
    if (pauseRestartButton != null)
    {
        pauseRestartButton.onClick.RemoveAllListeners();
        pauseRestartButton.onClick.AddListener(RestartGame);
    }
    
    if (pauseQuitButton != null)
    {
        pauseQuitButton.onClick.RemoveAllListeners();
        pauseQuitButton.onClick.AddListener(QuitToMainMenu);
    }
    
    if (resumeButton != null)
    {
        resumeButton.onClick.RemoveAllListeners();
        resumeButton.onClick.AddListener(ResumeGame);
    }
}

void RestartGame()
{
    // Reset score
    currentScore = 0;
    isScoreFrozen = false;
    isPaused = false;
    
    // Hide panels
    HideDeathPanel();
    HidePauseScreen();
    
    // Reload scene
    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}

void QuitToMainMenu()
{
    // Hide panels
    HideDeathPanel();
    HidePauseScreen();
    
    // Load main menu
    SceneManager.LoadScene("MainMenu");
}

void ResumeGame()
{
    HidePauseScreen();
}
```

### Step 4: Pause Screen Management
```csharp
void HandleEscapeKey()
{
    if (Input.GetKeyDown(KeyCode.Escape))
    {
        if (deathPanel != null && deathPanel.activeInHierarchy)
        {
            // If death panel is open, toggle its buttons
            ToggleDeathPanelButtons();
        }
        else if (!isScoreFrozen)
        {
            // If game is running, toggle pause
            TogglePauseScreen();
        }
    }
}

void TogglePauseScreen()
{
    if (isPaused)
    {
        HidePauseScreen();
    }
    else
    {
        ShowPauseScreen();
    }
}

void ShowPauseScreen()
{
    if (pauseScreen != null)
    {
        pauseScreen.SetActive(true);
        isPaused = true;
        Time.timeScale = 0f; // Pause game
    }
}

void HidePauseScreen()
{
    if (pauseScreen != null)
    {
        pauseScreen.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f; // Resume game
    }
}

void ToggleDeathPanelButtons()
{
    // Toggle visibility of death panel buttons
    if (restartButton != null)
    {
        restartButton.gameObject.SetActive(!restartButton.gameObject.activeInHierarchy);
    }
    
    if (quitButton != null)
    {
        quitButton.gameObject.SetActive(!quitButton.gameObject.activeInHierarchy);
    }
    
    if (mainMenuButton != null)
    {
        mainMenuButton.gameObject.SetActive(!mainMenuButton.gameObject.activeInHierarchy);
    }
}
```

### Step 5: Score Persistence
```csharp
void SaveHighScore()
{
    PlayerPrefs.SetInt("HighScore", highScore);
    PlayerPrefs.Save();
}

void LoadHighScore()
{
    highScore = PlayerPrefs.GetInt("HighScore", 0);
}

public void ResetHighScore()
{
    highScore = 0;
    SaveHighScore();
    UpdateUI();
}

public int GetCurrentScore()
{
    return currentScore;
}

public int GetHighScore()
{
    return highScore;
}
```

## Key Patterns Used

### 1. Singleton Pattern
```csharp
// Central score management
public class EnhancedScore : MonoBehaviour
{
    public static EnhancedScore Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
}
```

### 2. Speed-Based Scoring
```csharp
// Dynamic scoring based on game speed
void UpdateScore()
{
    float currentSpeed = EnhancedCamera.Instance?.GetCurrentSpeed() ?? 1f;
    float baseSpeed = EnhancedCamera.Instance?.GetStartSpeed() ?? 1f;
    float speedRatio = currentSpeed / baseSpeed;
    
    int pointsToAdd = Mathf.RoundToInt(baseScorePerSecond * speedMultiplier * speedRatio * Time.deltaTime);
    AddPoints(pointsToAdd);
}
```

### 3. State Management Pattern
```csharp
// Track game state for UI management
private bool isScoreFrozen = false;
private bool isPaused = false;

void Update()
{
    if (!isScoreFrozen && !isPaused)
    {
        UpdateScore();
    }
}
```

### 4. Button Setup Pattern
```csharp
// Consistent button setup
void SetupButtons()
{
    if (restartButton != null)
    {
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartGame);
    }
}
```

## What Was Learned

### Good Practices
- **Use singleton pattern** for global score access
- **Implement speed-based scoring** for dynamic gameplay
- **Separate UI management** from score logic
- **Save high scores persistently** using PlayerPrefs
- **Handle multiple game states** (playing, paused, dead)

### Common Pitfalls
- **Don't forget to unfreeze score** when restarting
- **Don't forget to reset Time.timeScale** when unpausing
- **Don't forget to remove button listeners** before adding new ones
- **Don't forget to check for null UI references**

### Performance Tips
- **Update UI only when score changes** - not every frame
- **Use efficient string formatting** for large numbers
- **Cache UI references** instead of finding them repeatedly
- **Use object pooling** for frequently created UI elements

## Integration Patterns

### With Camera System
```csharp
// Get speed for dynamic scoring
float currentSpeed = EnhancedCamera.Instance?.GetCurrentSpeed() ?? 1f;
```

### With Collectible System
```csharp
// Add points when collectible is collected
EnhancedScore.Instance?.AddPoints(collectibleValue);
```

### With Death Detection
```csharp
// Show death panel when player dies
void OnPlayerDeath()
{
    EnhancedScore.Instance?.ShowDeathPanel();
}
```

## Advanced Features

### Score Multipliers
```csharp
// Add temporary score multipliers
private float scoreMultiplier = 1f;
private float multiplierDuration = 0f;

public void AddScoreMultiplier(float multiplier, float duration)
{
    scoreMultiplier *= multiplier;
    multiplierDuration = Mathf.Max(multiplierDuration, duration);
}

void UpdateScore()
{
    if (multiplierDuration > 0)
    {
        multiplierDuration -= Time.deltaTime;
        if (multiplierDuration <= 0)
        {
            scoreMultiplier = 1f;
        }
    }
    
    int pointsToAdd = Mathf.RoundToInt(baseScorePerSecond * speedMultiplier * scoreMultiplier * Time.deltaTime);
    AddPoints(pointsToAdd);
}
```

### Score Streaks
```csharp
// Track consecutive collectibles for bonus points
private int collectibleStreak = 0;
private float streakTimer = 0f;
private float streakTimeout = 5f;

public void OnCollectibleCollected()
{
    collectibleStreak++;
    streakTimer = streakTimeout;
    
    // Bonus points for streaks
    int bonusPoints = collectibleValue * Mathf.Min(collectibleStreak, 5);
    AddPoints(bonusPoints);
}

void Update()
{
    if (streakTimer > 0)
    {
        streakTimer -= Time.deltaTime;
        if (streakTimer <= 0)
        {
            collectibleStreak = 0;
        }
    }
}
```

### Score Categories
```csharp
// Track different types of scores
public enum ScoreType
{
    Speed,
    Collectible,
    Survival,
    Bonus
}

private Dictionary<ScoreType, int> scoreBreakdown = new Dictionary<ScoreType, int>();

public void AddPoints(int points, ScoreType type)
{
    if (!scoreBreakdown.ContainsKey(type))
    {
        scoreBreakdown[type] = 0;
    }
    
    scoreBreakdown[type] += points;
    AddPoints(points);
}
```

## Future Improvements

### What Could Be Added Next Time
- **Score categories** - track different types of points
- **Achievement system** - unlock rewards for score milestones
- **Leaderboards** - compare scores with other players
- **Score animations** - visual feedback for point gains
- **Combo system** - bonus points for consecutive actions

### Advanced Features
- **Score prediction** - show potential score based on current performance
- **Score sharing** - allow players to share high scores
- **Score analysis** - break down score by game elements
- **Dynamic scoring** - adjust point values based on difficulty

## Quick Setup Checklist

For future projects, here's the quick scoring system setup:

1. Create EnhancedScore singleton
2. Implement basic score tracking
3. Add speed-based scoring
4. Create death panel UI
5. Create pause screen UI
6. Implement button functionality
7. Add score persistence
8. Integrate with other systems
9. Test all game states
10. Add visual feedback

This scoring system worked well for Quantum Thread and should work for other games that need dynamic, speed-based scoring with UI management.
