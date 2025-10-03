using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Enhanced score system with death panel and live HUD
/// This is the improved version that actually works well
/// </summary>
public class EnhancedScore : MonoBehaviour
{
    [Header("Score Settings")]
    [SerializeField] private float basePointsPerSecond = 10f;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float maxPointsPerSecond = 100f;
    
    [Header("Progressive Scoring")]
    [SerializeField] private float timeToMaxSpeed = 300f;
    [SerializeField] private float maxTimeSpeedMultiplier = 3f;
    
    [Header("UI")]
    [SerializeField] private Text scoreText;
    [SerializeField] private TextMeshProUGUI scoreTextMeshPro;
    [SerializeField] private string scoreFormat = "Score: {0}";
    [SerializeField] private bool showPointsPerSecond = false;
    
    [Header("Death Panel")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Text finalScoreText;
    [SerializeField] private TextMeshProUGUI finalScoreTextMeshPro;
    [SerializeField] private string finalScoreFormat = "FINAL SCORE: {0}";
    
    [SerializeField] private UnityEngine.UI.Button restartButton;
    [SerializeField] private UnityEngine.UI.Button quitButton;
    [SerializeField] private UnityEngine.UI.Button mainMenuButton;
    
    [Header("Death Quote Settings")]
    [SerializeField] private float deathQuoteDuration = 10f;
    
    [Header("Effects")]
    [SerializeField] private GameObject scorePopupPrefab;
    [SerializeField] private float popupDuration = 1f;
    [SerializeField] private Vector3 popupOffset = Vector3.up * 2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float deathVolume = 1f;
    [SerializeField] private float buttonVolume = 1f;
    
    [Header("Pause Screen")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private UnityEngine.UI.Button pauseRestartButton;
    [SerializeField] private UnityEngine.UI.Button pauseQuitButton;
    [SerializeField] private AudioClip pauseButtonClickSound;
    [SerializeField] private float pauseButtonVolume = 1f;
    
    
    private float currentScore;
    private float baseScore;
    private float gameTime;
    private EnhancedCamera cameraRef;
    private Camera mainCamera;
    private bool isScoreFrozen = false;
    private bool isPaused = false;
    
    public static EnhancedScore Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Find score text if not assigned
        if (scoreText == null && scoreTextMeshPro == null)
        {
            scoreText = FindFirstObjectByType<Text>();
            scoreTextMeshPro = FindFirstObjectByType<TextMeshProUGUI>();
        }
        
        // Find camera reference
        cameraRef = FindFirstObjectByType<EnhancedCamera>();
        mainCamera = Camera.main;
        
        // Ensure AudioSource exists
        EnsureAudioSource();
        
        // Find death panel if not assigned
        if (deathPanel == null)
        {
            // Try to find by name (searches all objects, including children)
            deathPanel = GameObject.Find("DeathPanel");
            if (deathPanel == null)
            {
                deathPanel = GameObject.Find("DeathPopupUI");
            }
            if (deathPanel == null)
            {
                // Try to find Canvas_HUD first, then look for DeathPanel as child
                GameObject canvasHUD = GameObject.Find("Canvas_HUD");
                if (canvasHUD != null)
                {
                    Transform deathPanelTransform = canvasHUD.transform.Find("DeathPanel");
                    if (deathPanelTransform != null)
                    {
                        deathPanel = deathPanelTransform.gameObject;
                    }
                }
            }
        }
        
        // Find final score text if not assigned
        if (finalScoreText == null && finalScoreTextMeshPro == null)
        {
            // Try to find by name first
            GameObject finalScoreObj = GameObject.Find("FinalScoreText");
            if (finalScoreObj != null)
            {
                finalScoreText = finalScoreObj.GetComponent<Text>();
                finalScoreTextMeshPro = finalScoreObj.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                // Try to find in death panel
                if (deathPanel != null)
                {
                    finalScoreText = deathPanel.GetComponentInChildren<Text>();
                    finalScoreTextMeshPro = deathPanel.GetComponentInChildren<TextMeshProUGUI>();
                }
            }
        }
        
        // Find buttons if not assigned
        if (restartButton == null)
        {
            // Try to find restart button by name first
            GameObject restartButtonObj = GameObject.Find("restart");
            if (restartButtonObj != null)
            {
                restartButton = restartButtonObj.GetComponent<UnityEngine.UI.Button>();
            }
            else
            {
                // Fallback to any button
                restartButton = FindFirstObjectByType<UnityEngine.UI.Button>();
            }
        }
        
        if (quitButton == null)
        {
            // Try to find quit button by name
            GameObject quitButtonObj = GameObject.Find("quit");
            if (quitButtonObj != null)
            {
                quitButton = quitButtonObj.GetComponent<UnityEngine.UI.Button>();
            }
        }
        
        if (mainMenuButton == null)
        {
            // Try to find main menu button by name
            GameObject mainMenuButtonObj = GameObject.Find("mainmenu") ?? GameObject.Find("main_menu") ?? GameObject.Find("MainMenu");
            if (mainMenuButtonObj != null)
            {
                mainMenuButton = mainMenuButtonObj.GetComponent<UnityEngine.UI.Button>();
            }
        }
        
        // Setup button listeners
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnClickRestart);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnClickQuit);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnClickMainMenu);
        }
        
        // Hide death panel initially
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }
        
        // Setup pause screen
        SetupPauseScreen();
        
        // Subscribe to game events
        EnhancedGameManager.OnGameRestart += OnGameRestart;
        EnhancedGameManager.OnPlayerDeath += OnPlayerDeath;
    }
    
    void OnDestroy()
    {
        EnhancedGameManager.OnGameRestart -= OnGameRestart;
        EnhancedGameManager.OnPlayerDeath -= OnPlayerDeath;
    }
    
    void Update()
    {
        // Handle Escape key for pause/death panel
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
        
        // Don't update scoring if frozen (player is dead)
        if (isScoreFrozen)
        {
            // Still update UI to show frozen score
            UpdateUI();
            return;
        }
        
        gameTime += Time.deltaTime;
        
        // Calculate time-based speed progression
        float timeProgress = Mathf.Clamp01(gameTime / timeToMaxSpeed);
        float timeSpeedMultiplier = Mathf.Lerp(1f, maxTimeSpeedMultiplier, timeProgress);
        
        // Calculate speed-based scoring using actual camera speed
        float speedBonus = 1f;
        if (cameraRef != null)
        {
            // Get the actual camera speed for proportional scoring
            float cameraSpeed = cameraRef.GetCurrentSpeed();
            float baseCameraSpeed = cameraRef.GetStartSpeed();
            
            // Calculate speed multiplier (1.0 = base speed, higher = faster)
            float speedRatio = cameraSpeed / baseCameraSpeed;
            
            // Apply speed bonus with some dampening to prevent ridiculous scores
            speedBonus = 1f + (speedRatio - 1f) * 0.5f; // 0.5f dampening factor
            
            // Clamp speed bonus to reasonable range
            speedBonus = Mathf.Clamp(speedBonus, 1f, 3f); // Max 3x scoring rate
        }
        
        // Combine time progression with camera speed and upgrade multiplier
        float finalSpeedMultiplier = timeSpeedMultiplier * speedBonus * speedMultiplier;
        
        
        // Add base score over time (apply final speed multiplier to base rate)
        float pointsThisFrame = basePointsPerSecond * finalSpeedMultiplier * Time.deltaTime;
        baseScore += pointsThisFrame;
        
        // Clamp to max points per second (adjusted for time progression)
        float maxPointsThisFrame = maxPointsPerSecond * Time.deltaTime;
        if (pointsThisFrame > maxPointsThisFrame)
        {
            baseScore = Mathf.Lerp(baseScore, baseScore - pointsThisFrame + maxPointsThisFrame, 0.1f);
        }
        
        // Calculate final score
        currentScore = Mathf.RoundToInt(baseScore);
        
        // Update UI
        UpdateUI();
        
        // Debug info
        
    }
    
    void UpdateUI()
    {
        string scoreDisplayText;
        if (showPointsPerSecond)
        {
            // Calculate current points per second using the same logic as Update()
            float currentPointsPerSecond = basePointsPerSecond;
            
            if (!isScoreFrozen)
            {
                // Time-based progression
                float timeProgress = Mathf.Clamp01(gameTime / timeToMaxSpeed);
                float timeSpeedMultiplier = Mathf.Lerp(1f, maxTimeSpeedMultiplier, timeProgress);
                
                // Camera speed bonus
                float speedBonus = 1f;
                if (cameraRef != null)
                {
                    float cameraSpeed = cameraRef.GetCurrentSpeed();
                    float baseCameraSpeed = cameraRef.GetStartSpeed();
                    float speedRatio = cameraSpeed / baseCameraSpeed;
                    speedBonus = 1f + (speedRatio - 1f) * 0.5f;
                    speedBonus = Mathf.Clamp(speedBonus, 1f, 3f);
                }
                
                // Combine all multipliers
                float finalSpeedMultiplier = timeSpeedMultiplier * speedBonus * speedMultiplier;
                currentPointsPerSecond *= finalSpeedMultiplier;
            }
            
            scoreDisplayText = string.Format(scoreFormat + " (+{1}/s)", currentScore, Mathf.RoundToInt(currentPointsPerSecond));
        }
        else
        {
            scoreDisplayText = string.Format(scoreFormat, currentScore);
        }
        
        // Update regular Text component
        if (scoreText != null)
        {
            scoreText.text = scoreDisplayText;
        }
        
        // Update TextMeshPro component
        if (scoreTextMeshPro != null)
        {
            scoreTextMeshPro.text = scoreDisplayText;
        }
    }
    
    public void AddPoints(int points)
    {
        baseScore += points;
        
        // Show score popup
        if (scorePopupPrefab != null)
        {
            ShowScorePopup(points);
        }
        
    }
    
    void ShowScorePopup(int points)
    {
        GameObject popup = Instantiate(scorePopupPrefab, transform.position + popupOffset, Quaternion.identity);
        
        // Set popup text
        Text popupText = popup.GetComponent<Text>();
        if (popupText != null)
        {
            popupText.text = "+" + points;
        }
        
        // Destroy popup after duration
        Destroy(popup, popupDuration);
    }
    
    public int GetScore()
    {
        return Mathf.RoundToInt(currentScore);
    }
    
    public float GetPointsPerSecond()
    {
        return basePointsPerSecond;
    }
    
    void OnPlayerDeath()
    {
        // Freeze the score at current value
        isScoreFrozen = true;
        
        // Show death panel when player dies
        ShowDeathPanel();
    }
    
    void ShowDeathPanel()
    {
        
        // Play death sound
        EnsureAudioSource();
        if (audioSource && deathSound)
        {
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            audioSource.PlayOneShot(deathSound, sfxVolume * deathVolume);
        }
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
            
            // Update final score display
            string finalScoreDisplayText = string.Format(finalScoreFormat, GetScore());
            
            if (finalScoreText != null)
            {
                finalScoreText.text = finalScoreDisplayText;
            }
            else if (finalScoreTextMeshPro != null)
            {
                finalScoreTextMeshPro.text = finalScoreDisplayText;
            }
            else
            {
            }
            
            // Show random death quote with a small delay to ensure panel is active
            StartCoroutine(ShowDeathQuoteDelayed());
            
            // Show death panel buttons by default (without pausing game)
            ShowDeathPanelButtonsWithoutPause();
        }
        else
        {
        }
    }
    
    System.Collections.IEnumerator ShowDeathQuoteDelayed()
    {
        // Wait one frame to ensure death panel is fully active
        yield return null;
        ShowDeathQuote();
    }
    
    void ShowDeathQuote()
    {
        DeathQuoteManager quoteManager = FindFirstObjectByType<DeathQuoteManager>();
        if (quoteManager != null)
        {
            quoteManager.ShowRandomQuote(deathQuoteDuration);
        }
        else if (deathPanel != null)
        {
            DeathQuoteManager panelQuoteManager = deathPanel.GetComponentInChildren<DeathQuoteManager>();
            if (panelQuoteManager != null)
            {
                panelQuoteManager.ShowRandomQuote(deathQuoteDuration);
            }
        }
    }
    
    void HideDeathPanel()
    {
        
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
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
        }
    }
    
    void PlayButtonSound()
    {
        EnsureAudioSource();
        if (audioSource && buttonClickSound)
        {
            float sfxVolume = EnhancedMusicManager.Instance != null ? 
                EnhancedMusicManager.Instance.GetSFXVolume() : 0.8f;
            audioSource.PlayOneShot(buttonClickSound, sfxVolume * buttonVolume);
        }
    }
    
    void PlayPauseButtonSound()
    {
        EnsureAudioSource();
        if (audioSource && pauseButtonClickSound)
        {
            float sfxVolume = EnhancedMusicManager.Instance != null ? 
                EnhancedMusicManager.Instance.GetSFXVolume() : 0.8f;
            audioSource.PlayOneShot(pauseButtonClickSound, sfxVolume * pauseButtonVolume);
        }
    }
    
    
    void OnClickRestart()
    {
        PlayButtonSound();
        
        // Hide death panel
        HideDeathPanel();
        
        // Restart the game
        EnhancedGameManager.Instance?.Restart();
    }
    
    void OnClickQuit()
    {
        PlayButtonSound();
        
        // Hide death panel
        HideDeathPanel();
        
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    void OnClickMainMenu()
    {
        PlayButtonSound();
        
        // Hide death panel
        HideDeathPanel();
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // Load scene at index 0 (main menu)
    }
    
    void SetupPauseScreen()
    {
        // Find pause screen if not assigned
        if (pauseScreen == null)
        {
            // Try to find by name
            pauseScreen = GameObject.Find("PauseScreen");
            if (pauseScreen == null)
            {
                // Try to find in Canvas_HUD
                GameObject canvasHUD = GameObject.Find("Canvas_HUD");
                if (canvasHUD != null)
                {
                    Transform pauseScreenTransform = canvasHUD.transform.Find("PauseScreen");
                    if (pauseScreenTransform != null)
                    {
                        pauseScreen = pauseScreenTransform.gameObject;
                    }
                }
            }
            
            // No fallback - pause screen must be properly assigned
            if (pauseScreen == null)
            {
                Debug.LogError("[EnhancedScore] No pause screen found! Please assign a PauseScreen GameObject in the Inspector.");
            }
        }
        
        // Setup pause screen buttons
        if (pauseRestartButton != null)
        {
            pauseRestartButton.onClick.AddListener(OnClickPauseRestart);
        }
        else if (pauseScreen != null)
        {
            // Try to find restart button in pause screen
            pauseRestartButton = pauseScreen.GetComponentInChildren<Button>();
            if (pauseRestartButton != null)
            {
                pauseRestartButton.onClick.AddListener(OnClickPauseRestart);
                Debug.Log("[EnhancedScore] Found restart button in pause screen");
            }
        }
        
        if (pauseQuitButton != null)
        {
            pauseQuitButton.onClick.AddListener(OnClickPauseQuit);
        }
        else if (pauseScreen != null)
        {
            // Try to find quit button in pause screen
            Button[] buttons = pauseScreen.GetComponentsInChildren<Button>();
            if (buttons.Length > 1)
            {
                pauseQuitButton = buttons[1]; // Second button is usually quit
                pauseQuitButton.onClick.AddListener(OnClickPauseQuit);
                Debug.Log("[EnhancedScore] Found quit button in pause screen");
            }
        }
        
        // Hide pause screen initially
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
    }
    
    void HandleEscapeKey()
    {
        // Handle Escape for pause screen (when not dead) or death panel (when dead)
        if (deathPanel != null && deathPanel.activeInHierarchy)
        {
            // Player is dead - toggle death panel buttons
            ToggleDeathPanelButtons();
        }
        else if (!isScoreFrozen)
        {
            // Player is alive - toggle pause screen
            TogglePauseScreen();
        }
    }
    
    void TogglePauseScreen()
    {
        if (pauseScreen == null) 
        {
            Debug.LogError("[EnhancedScore] Pause screen is null! Cannot toggle pause.");
            return;
        }
        
        if (isPaused)
        {
            // Currently paused - resume game
            ResumeGame();
        }
        else
        {
            // Currently playing - pause game
            PauseGame();
        }
    }
    
    void PauseGame()
    {
        if (pauseScreen == null) return;
        
        
        // Show pause screen first
        pauseScreen.SetActive(true);
        
        // Wait one frame to ensure UI is rendered and interactable
        StartCoroutine(PauseAfterUIRendered());
    }
    
    void ResumeGame()
    {
        if (pauseScreen == null) return;
        
        
        // Hide pause screen
        pauseScreen.SetActive(false);
        
        // Resume game immediately
        Time.timeScale = 1f;
        isPaused = false;
    }
    
    System.Collections.IEnumerator PauseAfterUIRendered()
    {
        // Wait one frame to ensure UI is fully rendered and interactable
        yield return null;
        
        // Now pause the game
        Time.timeScale = 0f;
        isPaused = true;
    }
    
    void OnClickPauseRestart()
    {
        PlayPauseButtonSound();
        
        // Resume game first
        ResumeGame();
        
        // Restart the game
        EnhancedGameManager.Instance?.Restart();
    }
    
    void OnClickPauseQuit()
    {
        PlayPauseButtonSound();
        
        // Resume game first
        ResumeGame();
        
        // Load main menu scene
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }
    
    void ToggleDeathPanelButtons()
    {
        if (deathPanel == null) return;
        
        // Find all buttons in the death panel
        Button[] buttons = deathPanel.GetComponentsInChildren<Button>();
        
        // Check if any buttons are currently visible
        bool anyButtonsVisible = false;
        foreach (Button button in buttons)
        {
            if (button != null && button.gameObject.activeInHierarchy)
            {
                anyButtonsVisible = true;
                break;
            }
        }
        
        // Toggle button visibility
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(!anyButtonsVisible);
            }
        }
        
        // Toggle game pause state
        if (anyButtonsVisible)
        {
            // Buttons were visible, now hiding them - unpause game
            Time.timeScale = 1f;
        }
        else
        {
            // Buttons were hidden, now showing them - pause game
            Time.timeScale = 0f;
        }
    }
    
    /// <summary>
    /// Show death panel buttons without pausing game
    /// </summary>
    public void ShowDeathPanelButtonsWithoutPause()
    {
        if (deathPanel == null) return;
        
        Button[] buttons = deathPanel.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
            }
        }
        
        // Do NOT pause the game - let it continue running
    }
    
    /// <summary>
    /// Show death panel buttons and pause game (for Escape key toggle)
    /// </summary>
    public void ShowDeathPanelButtons()
    {
        if (deathPanel == null) return;
        
        Button[] buttons = deathPanel.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
            }
        }
        
        // Pause the game when death panel buttons are shown via Escape
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// Hide death panel buttons and unpause game
    /// </summary>
    public void HideDeathPanelButtons()
    {
        if (deathPanel == null) return;
        
        Button[] buttons = deathPanel.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(false);
            }
        }
        
        Time.timeScale = 1f;
    }
    
    void OnGameRestart()
    {
        currentScore = 0f;
        baseScore = 0f;
        gameTime = 0f;
        isScoreFrozen = false; // Unfreeze score for new game
        isPaused = false; // Reset pause state
        
        // Hide death panel on restart
        HideDeathPanel();
        
        // Hide pause screen on restart
        if (pauseScreen != null)
        {
            pauseScreen.SetActive(false);
        }
        
        // Ensure game is not paused
        Time.timeScale = 1f;
    }
    
    // Upgrade system methods
    public int GetCurrentScore()
    {
        return Mathf.RoundToInt(currentScore);
    }
    
    public void DeductScore(int amount)
    {
        int oldScore = Mathf.RoundToInt(currentScore);
        currentScore = Mathf.Max(0, currentScore - amount);
        baseScore = currentScore; // Also update baseScore to keep them in sync
        UpdateUI(); // Update UI to show the change immediately
        
    }
    
    public void SetScoreRateMultiplier(float multiplier)
    {
        float oldMultiplier = speedMultiplier;
        speedMultiplier = Mathf.Clamp(multiplier, 0.5f, 3f);
        
        
        // Update UI to show new scoring rate
        UpdateUI();
    }
    
    public void SetCollectibleValueMultiplier(float multiplier)
    {
        // This affects collectible values through the spawner system
    }
    
    /// <summary>
    /// Check if the game is currently paused
    /// </summary>
    public bool IsPaused()
    {
        return isPaused;
    }
    
    /// <summary>
    /// Manually pause the game (for external systems)
    /// </summary>
    public void ForcePause()
    {
        if (!isPaused && !isScoreFrozen)
        {
            PauseGame();
        }
    }
    
    /// <summary>
    /// Manually resume the game (for external systems)
    /// </summary>
    public void ForceResume()
    {
        if (isPaused)
        {
            ResumeGame();
        }
    }
    
        // The actual implementation is in EnhancedSpawner.SetCollectibleValueMultiplier()
        // This method is here for consistency with the upgrade system
}
