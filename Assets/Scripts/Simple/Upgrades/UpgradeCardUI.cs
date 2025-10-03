using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Individual upgrade card UI component - handles each upgrade option display
/// </summary>
public class UpgradeCardUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField, Tooltip("Upgrade icon")]
    private Image iconImage;
    
    [SerializeField, Tooltip("Upgrade name")]
    private TextMeshProUGUI nameText;
    
    [SerializeField, Tooltip("Upgrade description")]
    private TextMeshProUGUI descriptionText;
    
    [SerializeField, Tooltip("Cost display")]
    private TextMeshProUGUI costText;
    
    [SerializeField, Tooltip("Stack count display")]
    private TextMeshProUGUI stackText;
    
    [SerializeField, Tooltip("Card button")]
    private Button cardButton;
    
    [SerializeField, Tooltip("Disabled overlay")]
    private GameObject disabledOverlay;
    
    [Header("Visual Effects")]
    [SerializeField, Tooltip("Hover scale effect")]
    private float hoverScale = 1.05f;
    
    [SerializeField, Tooltip("Normal scale")]
    private float normalScale = 1f;
    
    private RunUpgrade currentUpgrade;
    private System.Action<RunUpgrade> onUpgradeSelected;
    private bool isInteractable = true;
    
    void Start()
    {
        if (cardButton != null)
        {
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }
    
    public void SetupUpgrade(RunUpgrade upgrade, System.Action<RunUpgrade> onSelected)
    {
        currentUpgrade = upgrade;
        onUpgradeSelected = onSelected;
        
        if (upgrade == null)
        {
            gameObject.SetActive(false);
            return;
        }
        
        // Setup UI elements
        if (nameText != null)
            nameText.text = upgrade.displayName;
        
        if (descriptionText != null)
            descriptionText.text = upgrade.description;
        
        if (costText != null)
            costText.text = upgrade.cost.ToString();
        
        if (stackText != null)
        {
            // Get current stack count
            int currentStacks = 0;
            if (UpgradeManager.Instance != null)
            {
                currentStacks = UpgradeManager.Instance.GetUpgradeStackCount(upgrade.id);
            }
            string stackDisplay = $"{currentStacks}/{upgrade.maxStacks}";
            stackText.text = stackDisplay;
            Debug.Log($"[UpgradeCardUI] Initial stack display for {upgrade.displayName}: {stackDisplay} (maxStacks: {upgrade.maxStacks})");
        }
        
        if (iconImage != null && upgrade.icon != null)
            iconImage.sprite = upgrade.icon;
        
        // Check if upgrade can be taken
        CheckInteractability();
        
        // Show the card
        gameObject.SetActive(true);
    }
    
    void CheckInteractability()
    {
        if (UpgradeManager.Instance == null)
        {
            SetInteractable(false);
            return;
        }
        
        int currentScore = UpgradeManager.Instance.GetCurrentScore();
        bool canTake = currentUpgrade != null && currentScore >= currentUpgrade.cost;
        
        SetInteractable(canTake);
    }
    
    public void UpdateInteractability()
    {
        CheckInteractability();
    }
    
    public void UpdateStackDisplay()
    {
        if (currentUpgrade != null && stackText != null)
        {
            int currentStacks = 0;
            if (UpgradeManager.Instance != null)
            {
                currentStacks = UpgradeManager.Instance.GetUpgradeStackCount(currentUpgrade.id);
            }
            string stackDisplay = $"{currentStacks}/{currentUpgrade.maxStacks}";
            stackText.text = stackDisplay;
            Debug.Log($"[UpgradeCardUI] Updated stack display for {currentUpgrade.displayName}: {stackDisplay}");
        }
    }
    
    void SetInteractable(bool interactable)
    {
        isInteractable = interactable;
        
        if (cardButton != null)
            cardButton.interactable = interactable;
        
        // Only show disabled overlay when card is actually unaffordable
        if (disabledOverlay != null)
        {
            // Check if this is truly unaffordable (not just missing upgrade manager)
            bool isUnaffordable = !interactable && currentUpgrade != null && UpgradeManager.Instance != null;
            disabledOverlay.SetActive(isUnaffordable);
            
            // Make sure disabled overlay doesn't block raycast interactions
            if (isUnaffordable)
            {
                // Disable raycast target on the overlay so it doesn't block button clicks
                Image overlayImage = disabledOverlay.GetComponent<Image>();
                if (overlayImage != null)
                {
                    overlayImage.raycastTarget = false;
                }
            }
        }
        
        // Change visual appearance
        if (interactable)
        {
            transform.localScale = Vector3.one * normalScale;
        }
        else
        {
            transform.localScale = Vector3.one * (normalScale * 0.8f);
        }
    }
    
    void OnCardClicked()
    {
        Debug.Log($"[UpgradeCardUI] Card clicked - Interactable: {isInteractable}, Upgrade: {(currentUpgrade != null ? currentUpgrade.displayName : "NULL")}");
        
        if (!isInteractable || currentUpgrade == null) 
        {
            Debug.LogWarning($"[UpgradeCardUI] Card click ignored - Interactable: {isInteractable}, Upgrade: {(currentUpgrade != null ? "Present" : "NULL")}");
            return;
        }
        
        Debug.Log($"[UpgradeCardUI] Processing upgrade selection: {currentUpgrade.displayName} (ID: {currentUpgrade.id}, Cost: {currentUpgrade.cost})");
        
        // Play selection sound/effect
        PlaySelectionEffect();
        
        // Notify the parent panel
        onUpgradeSelected?.Invoke(currentUpgrade);
        
        Debug.Log($"[UpgradeCardUI] Upgrade selection event sent for: {currentUpgrade.displayName}");
    }
    
    void PlaySelectionEffect()
    {
        // Simple scale animation for feedback
        StartCoroutine(ScaleAnimation());
    }
    
    IEnumerator ScaleAnimation()
    {
        float duration = 0.1f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = startScale * hoverScale;
        
        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }
        
        // Scale back down
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(targetScale, startScale, t);
            yield return null;
        }
        
        transform.localScale = startScale;
    }
    
    void OnMouseEnter()
    {
        if (isInteractable)
        {
            transform.localScale = Vector3.one * hoverScale;
        }
    }
    
    void OnMouseExit()
    {
        if (isInteractable)
        {
            transform.localScale = Vector3.one * normalScale;
        }
    }
}
