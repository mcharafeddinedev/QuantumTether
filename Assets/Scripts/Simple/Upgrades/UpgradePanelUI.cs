using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// UI controller for the upgrade panel that displays 3 upgrade options - handles all the UI stuff
/// </summary>
public class UpgradePanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField, Tooltip("Upgrade card 1")]
    private UpgradeCardUI card1;
    
    [SerializeField, Tooltip("Upgrade card 2")]
    private UpgradeCardUI card2;
    
    [SerializeField, Tooltip("Upgrade card 3")]
    private UpgradeCardUI card3;
    
    [SerializeField, Tooltip("Current score display")]
    private TextMeshProUGUI scoreText;
    
    [SerializeField, Tooltip("Skip button (optional)")]
    private Button skipButton;
    
    [SerializeField, Tooltip("Panel title")]
    private TextMeshProUGUI titleText;
    
    [Header("Settings")]
    [SerializeField, Tooltip("Panel title text")]
    private string panelTitle = "Choose an Inflection of Time";
    
    private List<RunUpgrade> currentUpgrades = new List<RunUpgrade>();
    
    void Start()
    {
        SetupUI();
    }
    
    void SetupUI()
    {
        // Setup title
        if (titleText != null)
            titleText.text = panelTitle;
        
        // Setup skip button
        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipClicked);
        
        // Hide the panel initially
        gameObject.SetActive(false);
    }
    
    public void ShowUpgradePanel()
    {
        // Find the UpgradeManager in the scene
        UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (upgradeManager == null) 
        {
            return;
        }
        
        // Get available upgrades
        currentUpgrades = upgradeManager.GetCurrentUpgradeOptions();
        
        if (currentUpgrades.Count == 0)
        {
            return;
        }
        
        // Update score display
        UpdateScoreDisplay();
        
        // Setup the upgrade cards
        SetupUpgradeCards();
        
        // Refresh stack displays
        RefreshCardStackDisplays();
    }
    
    void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();
            if (upgradeManager != null)
            {
                int currentScore = upgradeManager.GetCurrentScore();
                scoreText.text = $"Score: {currentScore}";
            }
        }
        
        // Update card interactability when score changes
        RefreshCardInteractability();
    }
    
    void RefreshCardInteractability()
    {
        // Update all cards to reflect current score
        if (card1 != null)
            card1.UpdateInteractability();
        if (card2 != null)
            card2.UpdateInteractability();
        if (card3 != null)
            card3.UpdateInteractability();
    }
    
    void RefreshCardStackDisplays()
    {
        // Update all cards to reflect current stack counts
        if (card1 != null)
            card1.UpdateStackDisplay();
        if (card2 != null)
            card2.UpdateStackDisplay();
        if (card3 != null)
            card3.UpdateStackDisplay();
    }
    
    void SetupUpgradeCards()
    {
        // Setup card 1
        if (card1 != null && currentUpgrades.Count > 0)
        {
            card1.SetupUpgrade(currentUpgrades[0], OnUpgradeSelected);
        }
        
        // Setup card 2
        if (card2 != null && currentUpgrades.Count > 1)
        {
            card2.SetupUpgrade(currentUpgrades[1], OnUpgradeSelected);
        }
        
        // Setup card 3
        if (card3 != null && currentUpgrades.Count > 2)
        {
            card3.SetupUpgrade(currentUpgrades[2], OnUpgradeSelected);
        }
    }
    
    void OnUpgradeSelected(RunUpgrade upgrade)
    {
        Debug.Log($"[UpgradePanelUI] Upgrade selected: {upgrade.displayName} (ID: {upgrade.id}, Cost: {upgrade.cost})");
        
        UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (upgradeManager != null)
        {
            Debug.Log($"[UpgradePanelUI] Found UpgradeManager, calling SelectUpgrade");
            upgradeManager.SelectUpgrade(upgrade);
        }
        else
        {
            Debug.LogError("[UpgradePanelUI] UpgradeManager not found! Cannot process upgrade selection.");
        }
        
        // Refresh score display and remaining cards after selection
        UpdateScoreDisplay();
        RefreshCardInteractability();
        RefreshCardStackDisplays();
        
        Debug.Log($"[UpgradePanelUI] Upgrade selection processing complete for: {upgrade.displayName}");
        
        // Panel will be hidden by the UpgradeManager
    }
    
    void OnSkipClicked()
    {
        Debug.Log("[UpgradePanelUI] Skip button clicked!");
        
        UpgradeManager upgradeManager = FindFirstObjectByType<UpgradeManager>();
        if (upgradeManager != null)
        {
            Debug.Log("[UpgradePanelUI] Found UpgradeManager, closing panel...");
            upgradeManager.CloseUpgradePanel();
        }
        else
        {
            Debug.LogError("[UpgradePanelUI] UpgradeManager not found! Skip button cannot work.");
            // Fallback: hide panel directly
            gameObject.SetActive(false);
        }
        
        // Panel will be hidden by the UpgradeManager
    }
    
    /// <summary>
    /// Public method to trigger skip functionality (for testing or external calls)
    /// </summary>
    public void TriggerSkip()
    {
        OnSkipClicked();
    }
    
    void Update()
    {
        // Update score display while panel is open
        if (gameObject.activeInHierarchy)
        {
            UpdateScoreDisplay();
        }
    }
}

