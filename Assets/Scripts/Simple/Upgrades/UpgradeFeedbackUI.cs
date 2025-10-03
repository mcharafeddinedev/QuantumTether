using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Shows upgrade feedback messages to the player when they pick upgrades - pretty straightforward stuff
/// </summary>
public class UpgradeFeedbackUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject feedbackPanel;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float displayDuration = 2.5f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    [Header("Position Settings")]
    [SerializeField] private Vector2 screenPosition = new Vector2(0, 200);
    [SerializeField] private bool centerOnScreen = true;
    
    private Coroutine currentFeedbackCoroutine;
    
    void Awake()
    {
        // Set up the feedback panel if it's not assigned
        if (feedbackPanel == null)
        {
            feedbackPanel = gameObject;
        }
        
        // Need the canvas group for those smooth fade effects
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Find the text component if it's not assigned
        if (feedbackText == null)
        {
            feedbackText = GetComponentInChildren<TextMeshProUGUI>();
        }
        
        // Hide the feedback initially but keep the GameObject active for coroutines
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // Can't deactivate the main GameObject or coroutines won't work
        // Only hide the panel content if it's separate
        if (feedbackPanel != null && feedbackPanel != gameObject)
        {
            feedbackPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Shows feedback for a selected upgrade
    /// </summary>
    public void ShowUpgradeFeedback(RunUpgrade upgrade)
    {
        Debug.Log($"[UpgradeFeedbackUI] ShowUpgradeFeedback called for: {upgrade?.displayName ?? "NULL"}");
        
        if (upgrade == null) 
        {
            Debug.LogError("[UpgradeFeedbackUI] Upgrade is null! Cannot show feedback.");
            return;
        }
        
        string feedbackMessage = GetUpgradeFeedbackMessage(upgrade);
        Debug.Log($"[UpgradeFeedbackUI] Generated feedback message: '{feedbackMessage}'");
        ShowFeedback(feedbackMessage);
    }
    
    /// <summary>
    /// Shows a custom feedback message
    /// </summary>
    public void ShowFeedback(string message)
    {
        Debug.Log($"[UpgradeFeedbackUI] ShowFeedback called with message: '{message}'");
        
        if (string.IsNullOrEmpty(message)) 
        {
            Debug.LogWarning("[UpgradeFeedbackUI] Message is empty! Cannot show feedback.");
            return;
        }
        
        // Make sure the GameObject is active before starting coroutine
        if (!gameObject.activeInHierarchy)
        {
            gameObject.SetActive(true);
            Debug.Log("[UpgradeFeedbackUI] GameObject activated");
        }
        
        // Activate the feedback panel if it's separate from the main GameObject
        if (feedbackPanel != null && feedbackPanel != gameObject)
        {
            feedbackPanel.SetActive(true);
        }
        
        // Stop any current feedback first
        if (currentFeedbackCoroutine != null)
        {
            StopCoroutine(currentFeedbackCoroutine);
            Debug.Log("[UpgradeFeedbackUI] Stopped previous feedback coroutine");
        }
        
        Debug.Log("[UpgradeFeedbackUI] Starting new feedback display coroutine");
        
        // Double-check GameObject is active before starting coroutine
        if (gameObject.activeInHierarchy)
        {
            currentFeedbackCoroutine = StartCoroutine(DisplayFeedbackCoroutine(message));
        }
        else
        {
            Debug.LogError("[UpgradeFeedbackUI] GameObject is still inactive, cannot start coroutine!");
        }
    }
    
    /// <summary>
    /// Gets a detailed feedback message for an upgrade
    /// </summary>
    private string GetUpgradeFeedbackMessage(RunUpgrade upgrade)
    {
        switch (upgrade.id)
        {
            // Movement & grappling upgrades
            case "secondary_grapple":
                return "Secondary grapple enabled! You can now grapple to two points simultaneously.";
                
            case "dash_while_grappling":
                return "Dash while grappling unlocked! You can now dash even while attached to anchors.";
                
            case "auto_contract_grapple":
                return "Auto-contract enabled! Your grapple will automatically pull you toward anchors.";
                
            case "extended_grapple_range":
                return "Grapple range extended! You can now reach anchors from much farther away.";
                
            case "faster_dash_cooldown":
                return "Dash cooldown reduced! You can dash more frequently now.";
                
            case "stronger_dash_force":
                return "Dash force increased! Your dashes are now more powerful and cover more distance.";
                
            // Camera & speed upgrades
            case "faster_camera_speed":
                return "Camera speed increased! The world will scroll faster, increasing your score rate.";
                
            case "camera_grace_period":
                return "Grace period extended! You have more time before the camera catches up.";
                
            case "camera_death_distance":
                return "Death distance increased! You can survive being further from the camera.";
                
            // Scoring upgrades
            case "higher_score_rate":
                return "Score rate boosted! You'll earn points much faster now.";
                
            case "collectible_value_boost":
                return "Collectible value doubled! Each collectible is now worth 1000 points (was 500).";
                
            // Spawning & environment upgrades
            case "reduced_hazard_spawn_rate":
                return "Hazard spawn rate reduced! Fewer dangerous obstacles will appear.";
                
            case "reduced_anchor_density":
                return "Anchor density reduced! The environment will be less cluttered.";
                
            case "safer_collectible_spawning":
                return "Safer collectible spawning! Collectibles will avoid spawning near hazards.";
                
            // Default fallback for unrecognized upgrades
            default:
                return $"Upgrade applied: {upgrade.displayName}";
        }
    }
    
    /// <summary>
    /// Displays feedback with fade in/out animation using a coroutine
    /// </summary>
    private IEnumerator DisplayFeedbackCoroutine(string message)
    {
        // Set up the message text
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        
        // Position the feedback panel on screen
        if (centerOnScreen && feedbackPanel != null)
        {
            RectTransform rectTransform = feedbackPanel.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = screenPosition;
            }
        }
        
        // Show the panel
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(true);
        }
        
        // Fade in the feedback
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
        
        // Display the feedback for the specified duration
        yield return new WaitForSecondsRealtime(displayDuration);
        
        // Fade out the feedback
        if (canvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
        }
        
        // Hide the panel when done
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
        
        currentFeedbackCoroutine = null;
    }
    
    /// <summary>
    /// Force hides any current feedback
    /// </summary>
    public void HideFeedback()
    {
        if (currentFeedbackCoroutine != null)
        {
            StopCoroutine(currentFeedbackCoroutine);
            currentFeedbackCoroutine = null;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        if (feedbackPanel != null)
        {
            feedbackPanel.SetActive(false);
        }
    }
}
