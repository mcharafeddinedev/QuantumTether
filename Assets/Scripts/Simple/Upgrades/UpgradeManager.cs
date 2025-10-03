using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Manages the roguelike upgrade system timing and panel triggering - basically the brain of the upgrade system
/// </summary>
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
    [Header("Upgrade Settings")]
    [SerializeField, Tooltip("Upgrade library with all available upgrades")]
    private UpgradeLibrary upgradeLibrary;
    
    [SerializeField, Tooltip("Initial time between upgrades (seconds)")]
    private float initialUpgradeInterval = 30f;
    
    [SerializeField, Tooltip("Minimum game time before first upgrade can appear (seconds)")]
    private float minimumGameTimeBeforeUpgrade = 60f;
    
    [SerializeField, Tooltip("Time added to interval after each upgrade")]
    private float intervalStep = 5f;
    
    [SerializeField, Tooltip("Maximum time between upgrades")]
    private float maxUpgradeInterval = 60f;
    
    [Header("UI References")]
    [SerializeField, Tooltip("Upgrade panel UI")]
    private GameObject upgradePanel;
    
    [SerializeField, Tooltip("Upgrade applier service")]
    private UpgradeApplier upgradeApplier;
    
    [SerializeField, Tooltip("Upgrade feedback UI")]
    private UpgradeFeedbackUI feedbackUI;
    
    // State
    private RunUpgradeState upgradeState;
    private float gameStartTime;
    private float nextUpgradeTime;
    private float currentInterval;
    private bool isUpgradePanelOpen = false;
    private List<string> lastUpgradeIds = new List<string>();
    
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
        InitializeUpgradeSystem();
        
        // Try to find upgrade panel if not assigned
        if (upgradePanel == null)
        {
            upgradePanel = FindUpgradePanel();
        }
        
        // Subscribe to game restart events so we can reset when the player dies
        if (FindFirstObjectByType<EnhancedGameManager>() != null)
        {
            EnhancedGameManager.OnGameRestart += OnGameRestart;
        }
        
        // Subscribe to scene loaded events to reset when entering game scene
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (FindFirstObjectByType<EnhancedGameManager>() != null)
        {
            EnhancedGameManager.OnGameRestart -= OnGameRestart;
        }
        
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnGameRestart()
    {
        ResetUpgradeSystem();
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Reset upgrade system when entering game scene
        if (scene.name == "Game" || scene.name.ToLower().Contains("game"))
        {
            Debug.Log("[UpgradeManager] Game scene loaded, resetting upgrade system");
            ResetUpgradeSystem();
        }
    }
    
    void Update()
    {
        if (isUpgradePanelOpen) return;
        
        CheckForUpgradeTrigger();
    }
    
    void InitializeUpgradeSystem()
    {
        upgradeState = new RunUpgradeState();
        gameStartTime = Time.time;
        currentInterval = initialUpgradeInterval;
        nextUpgradeTime = gameStartTime + currentInterval;
        
        if (upgradeApplier == null)
            upgradeApplier = FindFirstObjectByType<UpgradeApplier>();
        
        // Disable background blur
        
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
    }
    
    void CheckForUpgradeTrigger()
    {
        // Don't trigger upgrades if player is dead
        if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
        {
            return;
        }
        
        // Wait until the minimum game time has passed before showing upgrades
        float gameTimeElapsed = Time.time - gameStartTime;
        if (gameTimeElapsed < minimumGameTimeBeforeUpgrade)
        {
            return;
        }
        
        if (Time.time >= nextUpgradeTime)
        {
            TriggerUpgradePanel();
        }
    }
    
    void TriggerUpgradePanel()
    {
        if (isUpgradePanelOpen) return;
        
        // Generate upgrade options first
        List<RunUpgrade> availableUpgrades = GetAvailableUpgrades();
        
        if (availableUpgrades.Count == 0)
        {
            return;
        }
        
        // Show the panel FIRST - before pausing
        if (upgradePanel != null)
        {
            // Activate the panel first
            upgradePanel.SetActive(true);
            
            // Get the UI script and populate the panel
            UpgradePanelUI panelUI = upgradePanel.GetComponent<UpgradePanelUI>();
            if (panelUI != null)
            {
                panelUI.ShowUpgradePanel();
                
                // Wait one frame to make sure UI is fully rendered and interactable
                StartCoroutine(PauseAfterUIRendered());
            }
        }
        else
        {
            // Try to find upgrade panel if not assigned
            upgradePanel = FindUpgradePanel();
            
            if (upgradePanel != null)
            {
                // Activate the panel first
                upgradePanel.SetActive(true);
                
                // Get the UI script and populate the panel
                UpgradePanelUI panelUI = upgradePanel.GetComponent<UpgradePanelUI>();
                if (panelUI != null)
                {
                    panelUI.ShowUpgradePanel();
                    
                    // Wait one frame to make sure UI is fully rendered and interactable
                    StartCoroutine(PauseAfterUIRendered());
                }
            }
            else
            {
                Debug.LogError("[UpgradeManager] Cannot find upgrade panel! Make sure it exists in the scene.");
            }
        }
    }
    
    List<RunUpgrade> GetAvailableUpgrades()
    {
        if (upgradeLibrary == null) return new List<RunUpgrade>();
        
        // Get current score
        int currentScore = GetCurrentScore();
        
        // Get taken upgrade IDs to avoid duplicates
        List<string> takenIds = upgradeState.GetTakenUpgradeIds();
        
        // Get all upgrades that can be taken
        List<RunUpgrade> allUpgrades = upgradeLibrary.GetAllUpgrades();
        List<RunUpgrade> availableUpgrades = new List<RunUpgrade>();
        
        foreach (var upgrade in allUpgrades)
        {
            if (upgradeState.CanTake(upgrade, currentScore))
            {
                availableUpgrades.Add(upgrade);
            }
        }
        
        // Try to avoid repeating the exact same trio
        List<RunUpgrade> filteredUpgrades = new List<RunUpgrade>();
        foreach (var upgrade in availableUpgrades)
        {
            if (!lastUpgradeIds.Contains(upgrade.id))
            {
                filteredUpgrades.Add(upgrade);
            }
        }
        
        // If we don't have enough after filtering, add some from the original list
        if (filteredUpgrades.Count < 3)
        {
            foreach (var upgrade in availableUpgrades)
            {
                if (!filteredUpgrades.Contains(upgrade))
                {
                    filteredUpgrades.Add(upgrade);
                }
            }
        }
        
        // Return up to 3 random upgrades
        return upgradeLibrary.GetRandomUpgrades(3, takenIds);
    }
    
    public int GetCurrentScore()
    {
        // Try to get score from upgradeApplier first
        if (upgradeApplier != null)
        {
            EnhancedScore scoreSystem = upgradeApplier.GetComponent<EnhancedScore>();
            if (scoreSystem != null)
            {
                return scoreSystem.GetCurrentScore();
            }
        }
        
        // Fallback: find EnhancedScore directly in the scene
        EnhancedScore directScore = FindFirstObjectByType<EnhancedScore>();
        if (directScore != null)
        {
            return directScore.GetCurrentScore();
        }
        
        return 1000; // Default score if nothing else works
    }
    
    public List<RunUpgrade> GetCurrentUpgradeOptions()
    {
        return GetAvailableUpgrades();
    }
    
    public void SelectUpgrade(RunUpgrade upgrade)
    {
        Debug.Log($"[UpgradeManager] SelectUpgrade called for: {upgrade?.displayName ?? "NULL"} (ID: {upgrade?.id ?? "NULL"})");
        
        if (upgrade == null) 
        {
            Debug.LogError("[UpgradeManager] Upgrade is null! Cannot process selection.");
            return;
        }
        
        // Don't process upgrade if player is dead
        if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
        {
            Debug.Log("[UpgradeManager] Player is dead - ignoring upgrade selection");
            return;
        }
        
        int currentScore = GetCurrentScore();
        Debug.Log($"[UpgradeManager] Current score: {currentScore}, Upgrade cost: {upgrade.cost}");
        
        if (!upgradeState.CanTake(upgrade, currentScore))
        {
            Debug.LogWarning($"[UpgradeManager] Cannot take upgrade {upgrade.displayName} - insufficient score or max stacks reached");
            return;
        }
        
        Debug.Log($"[UpgradeManager] Upgrade {upgrade.displayName} is valid, processing...");
        
        // Deduct cost from score
        Debug.Log($"[UpgradeManager] Deducting {upgrade.cost} score");
        DeductScore(upgrade.cost);
        
        // Add to state
        Debug.Log($"[UpgradeManager] Adding upgrade to state: {upgrade.id}");
        upgradeState.AddStack(upgrade.id);
        
        // Apply the upgrade
        Debug.Log($"[UpgradeManager] Applying upgrade effects...");
        if (upgradeApplier != null)
        {
            Debug.Log($"[UpgradeManager] Using assigned UpgradeApplier");
            upgradeApplier.ApplyUpgrade(upgrade);
        }
        else
        {
            Debug.LogWarning("[UpgradeManager] UpgradeApplier not assigned, searching for one...");
            // Fallback: find UpgradeApplier directly
            UpgradeApplier applier = FindFirstObjectByType<UpgradeApplier>();
            if (applier != null)
            {
                Debug.Log("[UpgradeManager] Found UpgradeApplier, applying upgrade");
                applier.ApplyUpgrade(upgrade);
            }
            else
            {
                Debug.LogError("[UpgradeManager] No UpgradeApplier found! Upgrade effects will not be applied!");
            }
        }
        
        // Show feedback to player
        Debug.Log($"[UpgradeManager] Showing upgrade feedback for: {upgrade.displayName}");
        ShowUpgradeFeedback(upgrade);
        
        // Update last upgrade IDs for variety
        lastUpgradeIds.Clear();
        lastUpgradeIds.Add(upgrade.id);
        Debug.Log($"[UpgradeManager] Updated last upgrade IDs for variety");
        
        // Close panel and schedule next upgrade
        Debug.Log($"[UpgradeManager] Closing upgrade panel and scheduling next upgrade");
        CloseUpgradePanel();
        
        Debug.Log($"[UpgradeManager] Upgrade selection complete for: {upgrade.displayName}");
    }
    
    void ShowUpgradeFeedback(RunUpgrade upgrade)
    {
        // Don't show feedback if player is dead
        if (EnhancedGameManager.Instance != null && EnhancedGameManager.Instance.IsDead)
        {
            Debug.Log("[UpgradeManager] Player is dead - skipping upgrade feedback");
            return;
        }
        
        if (feedbackUI != null)
        {
            feedbackUI.ShowUpgradeFeedback(upgrade);
        }
        else
        {
            // Fallback: find UpgradeFeedbackUI using robust search
            UpgradeFeedbackUI directFeedback = FindUpgradeFeedbackUI();
            if (directFeedback != null)
            {
                directFeedback.ShowUpgradeFeedback(upgrade);
            }
        }
    }
    
    UpgradeFeedbackUI FindUpgradeFeedbackUI()
    {
        // First try to find by component
        UpgradeFeedbackUI feedback = FindFirstObjectByType<UpgradeFeedbackUI>();
        if (feedback != null) return feedback;
        
        // Try to find by name in Canvas_HUD
        GameObject canvasHUD = GameObject.Find("Canvas_HUD");
        if (canvasHUD != null)
        {
            Transform feedbackTransform = canvasHUD.transform.Find("UpgradeFeedbackUI");
            if (feedbackTransform != null)
            {
                feedback = feedbackTransform.GetComponent<UpgradeFeedbackUI>();
                if (feedback != null) return feedback;
            }
        }
        
        // Try to find by name anywhere
        GameObject feedbackGO = GameObject.Find("UpgradeFeedbackUI");
        if (feedbackGO != null)
        {
            feedback = feedbackGO.GetComponent<UpgradeFeedbackUI>();
            if (feedback != null) return feedback;
        }
        
        return null;
    }
    
    void DeductScore(int amount)
    {
        // Try to get score system from upgradeApplier first
        if (upgradeApplier != null)
        {
            EnhancedScore scoreSystem = upgradeApplier.GetComponent<EnhancedScore>();
            if (scoreSystem != null)
            {
                scoreSystem.DeductScore(amount);
                return;
            }
        }
        
        // Fallback: find EnhancedScore directly in the scene
        EnhancedScore directScore = FindFirstObjectByType<EnhancedScore>();
        if (directScore != null)
        {
            directScore.DeductScore(amount);
        }
    }
    
    public void CloseUpgradePanel()
    {
        // Disable background blur
        
        if (upgradePanel != null)
            upgradePanel.SetActive(false);
        
        // Re-enable gameplay
        EnableGameplay();
        
        // Resume the game
        Time.timeScale = 1f;
        isUpgradePanelOpen = false;
        
        // Schedule next upgrade
        ScheduleNextUpgrade();
    }
    
    void ScheduleNextUpgrade()
    {
        // Increase interval
        currentInterval = Mathf.Min(currentInterval + intervalStep, maxUpgradeInterval);
        nextUpgradeTime = Time.time + currentInterval;
    }
    
    public void ResetUpgradeSystem()
    {
        Debug.Log($"[UpgradeManager] Resetting upgrade system - Game start time: {Time.time:F2}, Min time before upgrade: {minimumGameTimeBeforeUpgrade}s");
        
        upgradeState.Reset();
        gameStartTime = Time.time;
        currentInterval = initialUpgradeInterval;
        // Set next upgrade time to be after minimum game time + initial interval
        nextUpgradeTime = gameStartTime + minimumGameTimeBeforeUpgrade + currentInterval;
        lastUpgradeIds.Clear();
        isUpgradePanelOpen = false;
        
        Debug.Log($"[UpgradeManager] Next upgrade will be at time: {nextUpgradeTime:F2} (in {nextUpgradeTime - Time.time:F2} seconds)");
        
        // Try to find the upgrade panel if it's null
        if (upgradePanel == null)
        {
            upgradePanel = FindUpgradePanel();
            if (upgradePanel != null)
            {
                Debug.Log("[UpgradeManager] Found upgrade panel automatically");
            }
        }
        
        // Disable background blur
        
        if (upgradePanel != null)
        {
            upgradePanel.SetActive(false);
            Debug.Log("[UpgradeManager] Upgrade panel deactivated");
        }
        else
        {
            Debug.LogWarning("[UpgradeManager] Upgrade panel is null - will try to find it when needed");
        }
    }
    
    // Public getters for UI
    
    public bool IsUpgradePanelOpen()
    {
        return isUpgradePanelOpen;
    }
    
    public float GetTimeUntilNextUpgrade()
    {
        return Mathf.Max(0, nextUpgradeTime - Time.time);
    }
    
    void DisableGameplay()
    {
        // Disable player movement and input
        var playerSwing = FindFirstObjectByType<EnhancedPlayerSwing>();
        if (playerSwing != null)
        {
            playerSwing.enabled = false;
        }
        
        var playerDash = FindFirstObjectByType<EnhancedPlayerDash>();
        if (playerDash != null)
        {
            playerDash.enabled = false;
        }
        
        // Disable camera movement
        var camera = FindFirstObjectByType<EnhancedCamera>();
        if (camera != null)
        {
            camera.enabled = false;
        }
        
        // Disable spawner
        var spawner = FindFirstObjectByType<EnhancedSpawner>();
        if (spawner != null)
        {
            spawner.enabled = false;
        }
        
        // Disable score updates
        var score = FindFirstObjectByType<EnhancedScore>();
        if (score != null)
        {
            score.enabled = false;
        }
    }
    
    void EnableGameplay()
    {
        // Re-enable player movement and input
        var playerSwing = FindFirstObjectByType<EnhancedPlayerSwing>();
        if (playerSwing != null)
        {
            playerSwing.enabled = true;
        }
        
        var playerDash = FindFirstObjectByType<EnhancedPlayerDash>();
        if (playerDash != null)
        {
            playerDash.enabled = true;
        }
        
        // Re-enable camera movement
        var camera = FindFirstObjectByType<EnhancedCamera>();
        if (camera != null)
        {
            camera.enabled = true;
        }
        
        // Re-enable spawner
        var spawner = FindFirstObjectByType<EnhancedSpawner>();
        if (spawner != null)
        {
            spawner.enabled = true;
        }
        
        // Re-enable score updates
        var score = FindFirstObjectByType<EnhancedScore>();
        if (score != null)
        {
            score.enabled = true;
        }
    }
    
    IEnumerator PauseAfterUIRendered()
    {
        // Wait one frame to ensure UI is fully rendered and interactable
        yield return null;
        
        // NOW pause the game logic but keep UI responsive
        Time.timeScale = 0f;
        isUpgradePanelOpen = true;
        
        // Disable player input and movement
        DisableGameplay();
    }
    
    GameObject FindUpgradePanel()
    {
        Debug.Log("[UpgradeManager] Searching for upgrade panel...");
        
        // Try to find by name first
        GameObject foundPanel = GameObject.Find("UpgradePanel");
        if (foundPanel != null)
        {
            Debug.Log("[UpgradeManager] Found upgrade panel by name: UpgradePanel");
            return foundPanel;
        }
        
        // Try to find under Canvas_HUD (same as death panel pattern)
        GameObject canvasHUD = GameObject.Find("Canvas_HUD");
        if (canvasHUD != null)
        {
            Debug.Log("[UpgradeManager] Found Canvas_HUD, searching for UpgradePanel child...");
            Transform upgradePanelTransform = canvasHUD.transform.Find("UpgradePanel");
            if (upgradePanelTransform != null)
            {
                Debug.Log("[UpgradeManager] Found upgrade panel under Canvas_HUD");
                return upgradePanelTransform.gameObject;
            }
            else
            {
                Debug.LogWarning("[UpgradeManager] Canvas_HUD found but no UpgradePanel child found");
            }
        }
        else
        {
            Debug.LogWarning("[UpgradeManager] Canvas_HUD not found");
        }
        
        // Try to find by component
        UpgradePanelUI panelUI = FindFirstObjectByType<UpgradePanelUI>();
        if (panelUI != null)
        {
            Debug.Log("[UpgradeManager] Found upgrade panel by UpgradePanelUI component");
            return panelUI.gameObject;
        }
        
        Debug.LogError("[UpgradeManager] Could not find upgrade panel by any method!");
        return null;
    }
    
    
    /// <summary>
    /// Get current stack count for an upgrade
    /// </summary>
    public int GetUpgradeStackCount(string upgradeId)
    {
        if (upgradeState == null) return 0;
        return upgradeState.GetStackCount(upgradeId);
    }
    
    /// <summary>
    /// Force reset upgrade system (for testing)
    /// </summary>
    [ContextMenu("Force Reset Upgrade System")]
    public void ForceResetUpgradeSystem()
    {
        ResetUpgradeSystem();
    }
}
