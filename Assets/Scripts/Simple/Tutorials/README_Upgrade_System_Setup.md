# Upgrade System Setup - Reference Guide

## What This Document Contains

This document covers the complete upgrade system built for Quantum Thread, which provides roguelike progression mechanics. Players earn upgrade points by collecting items and can choose from various upgrades that enhance their abilities. It explains the upgrade library system, UI components, upgrade application mechanics, and how the upgrade system integrates with player systems and scoring.

## The Upgrade System Architecture

### Core Components
- **UpgradeManager.cs** - Central upgrade system controller
- **UpgradeData.cs** - ScriptableObject for upgrade definitions
- **UpgradeUI.cs** - Interface for upgrade selection
- **Upgrade effects** - Various gameplay modifications
- **Progression tracking** - Points, unlocks, and upgrade history

### How It Works
1. **Players collect items** to earn upgrade points
2. **Upgrade points accumulate** and can be spent on upgrades
3. **Upgrade selection screen** appears when points are available
4. **Players choose upgrades** from available options
5. **Upgrade effects apply** immediately to gameplay
6. **Progression unlocks** new upgrade options

## Setting Up Upgrade System in Future Projects

### Step 1: Upgrade Data Structure
```csharp
// UpgradeData.cs - ScriptableObject for upgrade definitions
[CreateAssetMenu(fileName = "New Upgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    [Header("Basic Info")]
    public string upgradeName;
    public string description;
    public Sprite icon;
    public int cost = 1;
    
    [Header("Upgrade Type")]
    public UpgradeType upgradeType;
    public UpgradeCategory category;
    
    [Header("Effects")]
    public float value = 1f;
    public bool isPercentage = false;
    public bool isStackable = true;
    public int maxStacks = 0; // 0 = unlimited
    
    [Header("Requirements")]
    public UpgradeData[] prerequisites;
    public int requiredLevel = 0;
    public bool isUnlocked = true;
    
    [Header("Visual")]
    public Color upgradeColor = Color.white;
    public string flavorText;
}

public enum UpgradeType
{
    DashCooldown,
    DashDistance,
    GrappleRange,
    GrappleSpeed,
    MovementSpeed,
    Health,
    ScoreMultiplier,
    CollectibleValue,
    SpecialAbility
}

public enum UpgradeCategory
{
    Movement,
    Combat,
    Utility,
    Special
}
```

### Step 2: Upgrade Manager
```csharp
// UpgradeManager.cs - Central upgrade system controller
public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrade Settings")]
    [SerializeField] private UpgradeData[] allUpgrades;
    [SerializeField] private int maxUpgradesPerSelection = 3;
    [SerializeField] private float upgradeSelectionTime = 10f;
    
    [Header("UI References")]
    [SerializeField] private GameObject upgradeSelectionUI;
    [SerializeField] private Transform upgradeButtonParent;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button skipButton;
    
    [Header("Audio")]
    [SerializeField] private AudioClip upgradeSelectSound;
    [SerializeField] private AudioClip upgradeConfirmSound;
    [SerializeField] private AudioSource audioSource;
    
    private int currentUpgradePoints = 0;
    private Dictionary<UpgradeData, int> purchasedUpgrades = new Dictionary<UpgradeData, int>();
    private List<UpgradeData> availableUpgrades = new List<UpgradeData>();
    private UpgradeData selectedUpgrade;
    private Coroutine selectionTimerCoroutine;
    
    public static UpgradeManager Instance { get; private set; }
    
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
        EnsureAudioSource();
        LoadUpgradeProgress();
        RefreshAvailableUpgrades();
    }
    
    public void AddUpgradePoints(int points)
    {
        currentUpgradePoints += points;
        Debug.Log($"Added {points} upgrade points. Total: {currentUpgradePoints}");
        
        // Show upgrade selection if we have enough points
        if (currentUpgradePoints > 0 && availableUpgrades.Count > 0)
        {
            ShowUpgradeSelection();
        }
    }
    
    void ShowUpgradeSelection()
    {
        if (upgradeSelectionUI != null)
        {
            upgradeSelectionUI.SetActive(true);
            SetupUpgradeButtons();
            StartSelectionTimer();
        }
    }
    
    void SetupUpgradeButtons()
    {
        // Clear existing buttons
        foreach (Transform child in upgradeButtonParent)
        {
            Destroy(child.gameObject);
        }
        
        // Get random upgrades to choose from
        List<UpgradeData> selectionUpgrades = GetRandomUpgrades(maxUpgradesPerSelection);
        
        foreach (UpgradeData upgrade in selectionUpgrades)
        {
            CreateUpgradeButton(upgrade);
        }
        
        // Setup confirm/skip buttons
        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(ConfirmSelection);
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.RemoveAllListeners();
            skipButton.onClick.AddListener(SkipSelection);
        }
    }
    
    List<UpgradeData> GetRandomUpgrades(int count)
    {
        List<UpgradeData> result = new List<UpgradeData>();
        List<UpgradeData> available = new List<UpgradeData>(availableUpgrades);
        
        // Filter by affordability
        available = available.Where(u => u.cost <= currentUpgradePoints).ToList();
        
        // Shuffle and take random selection
        for (int i = 0; i < Mathf.Min(count, available.Count); i++)
        {
            int randomIndex = Random.Range(0, available.Count);
            result.Add(available[randomIndex]);
            available.RemoveAt(randomIndex);
        }
        
        return result;
    }
    
    void CreateUpgradeButton(UpgradeData upgrade)
    {
        GameObject buttonObj = Instantiate(upgradeButtonPrefab, upgradeButtonParent);
        Button button = buttonObj.GetComponent<Button>();
        UpgradeButton upgradeButton = buttonObj.GetComponent<UpgradeButton>();
        
        if (upgradeButton != null)
        {
            upgradeButton.SetupUpgrade(upgrade);
        }
        
        button.onClick.AddListener(() => SelectUpgrade(upgrade));
    }
    
    void SelectUpgrade(UpgradeData upgrade)
    {
        selectedUpgrade = upgrade;
        PlayButtonSound();
        
        // Visual feedback
        foreach (Transform child in upgradeButtonParent)
        {
            UpgradeButton upgradeButton = child.GetComponent<UpgradeButton>();
            if (upgradeButton != null)
            {
                upgradeButton.SetSelected(upgradeButton.GetUpgrade() == upgrade);
            }
        }
    }
    
    void ConfirmSelection()
    {
        if (selectedUpgrade != null)
        {
            PurchaseUpgrade(selectedUpgrade);
            PlayConfirmSound();
        }
        
        HideUpgradeSelection();
    }
    
    void SkipSelection()
    {
        HideUpgradeSelection();
    }
    
    void PurchaseUpgrade(UpgradeData upgrade)
    {
        if (currentUpgradePoints >= upgrade.cost)
        {
            currentUpgradePoints -= upgrade.cost;
            
            // Track purchase
            if (purchasedUpgrades.ContainsKey(upgrade))
            {
                purchasedUpgrades[upgrade]++;
            }
            else
            {
                purchasedUpgrades[upgrade] = 1;
            }
            
            // Apply upgrade effect
            ApplyUpgradeEffect(upgrade);
            
            // Refresh available upgrades
            RefreshAvailableUpgrades();
            
            Debug.Log($"Purchased {upgrade.upgradeName} for {upgrade.cost} points");
        }
    }
    
    void ApplyUpgradeEffect(UpgradeData upgrade)
    {
        switch (upgrade.upgradeType)
        {
            case UpgradeType.DashCooldown:
                EnhancedPlayerDash.Instance?.ReduceCooldown(upgrade.value);
                break;
                
            case UpgradeType.DashDistance:
                EnhancedPlayerDash.Instance?.IncreaseDistance(upgrade.value);
                break;
                
            case UpgradeType.GrappleRange:
                EnhancedPlayerSwing.Instance?.IncreaseRange(upgrade.value);
                break;
                
            case UpgradeType.GrappleSpeed:
                EnhancedPlayerSwing.Instance?.IncreaseSpeed(upgrade.value);
                break;
                
            case UpgradeType.MovementSpeed:
                EnhancedCamera.Instance?.IncreaseSpeed(upgrade.value);
                break;
                
            case UpgradeType.ScoreMultiplier:
                EnhancedScore.Instance?.AddScoreMultiplier(upgrade.value);
                break;
                
            case UpgradeType.CollectibleValue:
                EnhancedCollectible.Instance?.IncreaseValue(upgrade.value);
                break;
        }
    }
    
    void RefreshAvailableUpgrades()
    {
        availableUpgrades.Clear();
        
        foreach (UpgradeData upgrade in allUpgrades)
        {
            if (IsUpgradeAvailable(upgrade))
            {
                availableUpgrades.Add(upgrade);
            }
        }
    }
    
    bool IsUpgradeAvailable(UpgradeData upgrade)
    {
        // Check if unlocked
        if (!upgrade.isUnlocked)
        {
            return false;
        }
        
        // Check prerequisites
        foreach (UpgradeData prereq in upgrade.prerequisites)
        {
            if (!purchasedUpgrades.ContainsKey(prereq))
            {
                return false;
            }
        }
        
        // Check max stacks
        if (upgrade.maxStacks > 0 && purchasedUpgrades.ContainsKey(upgrade))
        {
            if (purchasedUpgrades[upgrade] >= upgrade.maxStacks)
            {
                return false;
            }
        }
        
        return true;
    }
    
    void StartSelectionTimer()
    {
        if (selectionTimerCoroutine != null)
        {
            StopCoroutine(selectionTimerCoroutine);
        }
        selectionTimerCoroutine = StartCoroutine(SelectionTimerCoroutine());
    }
    
    System.Collections.IEnumerator SelectionTimerCoroutine()
    {
        yield return new WaitForSeconds(upgradeSelectionTime);
        
        // Auto-select first upgrade if none selected
        if (selectedUpgrade == null && upgradeButtonParent.childCount > 0)
        {
            Button firstButton = upgradeButtonParent.GetChild(0).GetComponent<Button>();
            if (firstButton != null)
            {
                firstButton.onClick.Invoke();
            }
        }
        
        // Auto-confirm selection
        ConfirmSelection();
    }
    
    void HideUpgradeSelection()
    {
        if (upgradeSelectionUI != null)
        {
            upgradeSelectionUI.SetActive(false);
        }
        
        selectedUpgrade = null;
        
        if (selectionTimerCoroutine != null)
        {
            StopCoroutine(selectionTimerCoroutine);
            selectionTimerCoroutine = null;
        }
    }
}
```

### Step 3: Upgrade Button UI
```csharp
// UpgradeButton.cs - UI component for upgrade selection
public class UpgradeButton : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Button button;
    
    [Header("Visual Settings")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color affordableColor = Color.green;
    [SerializeField] private Color unaffordableColor = Color.red;
    
    private UpgradeData upgradeData;
    private bool isSelected = false;
    
    public void SetupUpgrade(UpgradeData upgrade)
    {
        upgradeData = upgrade;
        
        if (nameText != null)
        {
            nameText.text = upgrade.upgradeName;
        }
        
        if (descriptionText != null)
        {
            descriptionText.text = upgrade.description;
        }
        
        if (costText != null)
        {
            costText.text = upgrade.cost.ToString();
        }
        
        if (iconImage != null && upgrade.icon != null)
        {
            iconImage.sprite = upgrade.icon;
        }
        
        if (backgroundImage != null)
        {
            backgroundImage.color = upgrade.upgradeColor;
        }
        
        UpdateAffordability();
    }
    
    void UpdateAffordability()
    {
        if (upgradeData == null) return;
        
        int currentPoints = UpgradeManager.Instance?.GetCurrentPoints() ?? 0;
        bool canAfford = currentPoints >= upgradeData.cost;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = canAfford ? affordableColor : unaffordableColor;
        }
        
        if (button != null)
        {
            button.interactable = canAfford;
        }
    }
    
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        
        if (backgroundImage != null)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }
    
    public UpgradeData GetUpgrade()
    {
        return upgradeData;
    }
}
```

### Step 4: Integration with Other Systems
```csharp
// In EnhancedCollectible.cs - Award upgrade points
public class EnhancedCollectible : MonoBehaviour
{
    [Header("Upgrade Points")]
    [SerializeField] private int upgradePointsValue = 1;
    
    void Collect()
    {
        // Add upgrade points
        UpgradeManager.Instance?.AddUpgradePoints(upgradePointsValue);
        
        // Add regular score
        EnhancedScore.Instance?.AddPoints(pointsValue);
        
        // Play collection effects
        PlayCollectionEffects();
    }
}

// In EnhancedScore.cs - Track upgrade point collection
public class EnhancedScore : MonoBehaviour
{
    private int totalUpgradePointsCollected = 0;
    
    public void AddUpgradePoints(int points)
    {
        totalUpgradePointsCollected += points;
        Debug.Log($"Total upgrade points collected: {totalUpgradePointsCollected}");
    }
}
```

## Key Patterns I Used

### 1. ScriptableObject Data Pattern
```csharp
// Use ScriptableObjects for upgrade definitions
[CreateAssetMenu(fileName = "New Upgrade", menuName = "Game/Upgrade Data")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public UpgradeType upgradeType;
    public int cost;
    // etc...
}
```

### 2. Singleton Manager Pattern
```csharp
// Central upgrade management
public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance { get; private set; }
    
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

### 3. Event-Driven Upgrade Application
```csharp
// Apply upgrades through system references
void ApplyUpgradeEffect(UpgradeData upgrade)
{
    switch (upgrade.upgradeType)
    {
        case UpgradeType.DashCooldown:
            EnhancedPlayerDash.Instance?.ReduceCooldown(upgrade.value);
            break;
        // etc...
    }
}
```

### 4. Prerequisite System Pattern
```csharp
// Check prerequisites before making upgrades available
bool IsUpgradeAvailable(UpgradeData upgrade)
{
    foreach (UpgradeData prereq in upgrade.prerequisites)
    {
        if (!purchasedUpgrades.ContainsKey(prereq))
        {
            return false;
        }
    }
    return true;
}
```

## What I Learned

### Good Practices
- **Use ScriptableObjects for upgrade data** - easy to create and modify
- **Implement prerequisite systems** - creates meaningful progression
- **Provide visual feedback** - make upgrades feel impactful
- **Allow upgrade stacking** - increases replayability
- **Save upgrade progress** - maintain progression across sessions

### Common Pitfalls
- **Don't make upgrades too powerful** - breaks game balance
- **Don't forget to save progress** - players lose upgrades on restart
- **Don't make prerequisites too complex** - players get confused
- **Don't forget to reset upgrades** - for new game+ modes

### Performance Tips
- **Cache upgrade references** - avoid repeated lookups
- **Use object pooling for UI** - upgrade buttons are created frequently
- **Limit upgrade selection time** - prevents players from getting stuck
- **Batch upgrade effects** - apply multiple upgrades at once

## Integration Patterns

### With Collectible System
```csharp
// Award upgrade points when collecting items
void Collect()
{
    UpgradeManager.Instance?.AddUpgradePoints(upgradePointsValue);
}
```

### With Player Systems
```csharp
// Apply upgrade effects to player abilities
void ApplyUpgradeEffect(UpgradeData upgrade)
{
    switch (upgrade.upgradeType)
    {
        case UpgradeType.DashCooldown:
            EnhancedPlayerDash.Instance?.ReduceCooldown(upgrade.value);
            break;
    }
}
```

### With Save System
```csharp
// Save upgrade progress
void SaveUpgradeProgress()
{
    PlayerPrefs.SetInt("UpgradePoints", currentUpgradePoints);
    
    // Save purchased upgrades
    string upgradeData = JsonUtility.ToJson(purchasedUpgrades);
    PlayerPrefs.SetString("PurchasedUpgrades", upgradeData);
}
```

## Advanced Features

### Upgrade Trees
```csharp
// Create branching upgrade paths
public class UpgradeTree
{
    public UpgradeData rootUpgrade;
    public List<UpgradeTree> branches = new List<UpgradeTree>();
    
    public bool IsUnlocked()
    {
        if (rootUpgrade == null) return true;
        
        return UpgradeManager.Instance.IsUpgradePurchased(rootUpgrade);
    }
}
```

### Dynamic Upgrade Costs
```csharp
// Increase costs based on purchases
int CalculateUpgradeCost(UpgradeData upgrade)
{
    int baseCost = upgrade.cost;
    int purchasedCount = purchasedUpgrades.ContainsKey(upgrade) ? purchasedUpgrades[upgrade] : 0;
    
    // Increase cost by 50% for each purchase
    return Mathf.RoundToInt(baseCost * Mathf.Pow(1.5f, purchasedCount));
}
```

### Upgrade Categories
```csharp
// Organize upgrades by category
public class UpgradeCategory
{
    public string categoryName;
    public Color categoryColor;
    public List<UpgradeData> upgrades = new List<UpgradeData>();
    
    public bool IsUnlocked()
    {
        // Unlock category when first upgrade is available
        return upgrades.Any(u => UpgradeManager.Instance.IsUpgradeAvailable(u));
    }
}
```

## Future Improvements

### What I Could Add Next Time
- **Upgrade trees** - branching progression paths
- **Temporary upgrades** - upgrades that expire
- **Upgrade combinations** - synergies between upgrades
- **Upgrade previews** - show effects before purchasing
- **Upgrade refunds** - allow players to change their mind

### Advanced Features
- **Procedural upgrades** - randomly generated upgrade effects
- **Upgrade sets** - bonuses for having multiple related upgrades
- **Upgrade challenges** - special conditions to unlock upgrades
- **Upgrade analytics** - track which upgrades are most popular

## Quick Setup Checklist

For future projects, here's my quick upgrade system setup:

1. Create UpgradeData ScriptableObject
2. Create UpgradeManager singleton
3. Create UpgradeButton UI component
4. Setup upgrade selection interface
5. Implement upgrade effect application
6. Add prerequisite system
7. Integrate with collectible system
8. Add upgrade progress saving
9. Test upgrade balance and progression
10. Add visual feedback and polish

This upgrade system worked really well for Quantum Thread and should work for roguelike progression mechanics in other games I work on in the future.
