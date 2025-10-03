using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Manages random death quotes and taunts for the death panel - pretty cool feature
/// </summary>
public class DeathQuoteManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI quoteText;
    [SerializeField] private Text quoteTextLegacy; // Fallback for regular Text component
    
    [Header("Quote Settings")]
    [SerializeField] private List<string> deathQuotes = new List<string>();
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private CanvasGroup canvasGroup;
    
    void Awake()
    {
        InitializeQuotes();
        SetupCanvasGroup();
    }
    
    void Start()
    {
        // Hide quote initially
        if (quoteText != null)
        {
            quoteText.text = "";
        }
        else if (quoteTextLegacy != null)
        {
            quoteTextLegacy.text = "";
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // Subscribe to game restart events
        EnhancedGameManager.OnGameRestart += OnGameRestart;
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        EnhancedGameManager.OnGameRestart -= OnGameRestart;
    }
    
    void OnGameRestart()
    {
        // Reset the quote manager for new game
        if (quoteText != null)
        {
            quoteText.text = "";
        }
        else if (quoteTextLegacy != null)
        {
            quoteTextLegacy.text = "";
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        
        // Stop any running coroutines
        StopAllCoroutines();
    }
    
    void InitializeQuotes()
    {
        // Always ensure we have quotes, even if inspector has some
        if (deathQuotes.Count == 0)
        {
            Debug.Log("[DeathQuoteManager] No quotes in inspector, adding default quotes");
            deathQuotes.AddRange(GetDefaultQuotes());
        }
        else
        {
            Debug.Log($"[DeathQuoteManager] Found {deathQuotes.Count} quotes in inspector");
        }
        
        // Double-check we have quotes
        if (deathQuotes.Count == 0)
        {
            Debug.LogError("[DeathQuoteManager] Still no quotes after initialization! Adding emergency quotes.");
            deathQuotes.Add("Game Over!");
            deathQuotes.Add("Try Again!");
            deathQuotes.Add("Better luck next time!");
        }
        
        Debug.Log($"[DeathQuoteManager] Initialized with {deathQuotes.Count} quotes");
    }
    
    void SetupCanvasGroup()
    {
        // Try TextMeshPro first, then fallback to regular Text
        GameObject textObject = null;
        if (quoteText != null)
        {
            textObject = quoteText.gameObject;
        }
        else if (quoteTextLegacy != null)
        {
            textObject = quoteTextLegacy.gameObject;
        }
        
        if (textObject != null)
        {
            canvasGroup = textObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = textObject.AddComponent<CanvasGroup>();
            }
        }
        else
        {
            Debug.LogError("[DeathQuoteManager] No text component found for CanvasGroup setup!");
        }
    }
    
    /// <summary>
    /// Show a random death quote
    /// </summary>
    public void ShowRandomQuote()
    {
        ShowRandomQuote(displayDuration);
    }
    
    /// <summary>
    /// Show a random death quote with custom display duration
    /// </summary>
    public void ShowRandomQuote(float customDisplayDuration)
    {
        Debug.Log($"[DeathQuoteManager] ShowRandomQuote called with duration: {customDisplayDuration}s. Quote count: {deathQuotes.Count}");
        
        // Ensure we have quotes before proceeding
        if (deathQuotes.Count == 0) 
        {
            Debug.LogWarning("[DeathQuoteManager] No death quotes available! Re-initializing...");
            InitializeQuotes();
            
            if (deathQuotes.Count == 0)
            {
                Debug.LogError("[DeathQuoteManager] Still no quotes after re-initialization!");
                return;
            }
        }
        
        string randomQuote = deathQuotes[Random.Range(0, deathQuotes.Count)];
        Debug.Log($"[DeathQuoteManager] Selected quote: {randomQuote}");
        ShowQuote(randomQuote, customDisplayDuration);
    }
    
    /// <summary>
    /// Show a specific quote
    /// </summary>
    public void ShowQuote(string quote)
    {
        ShowQuote(quote, displayDuration);
    }
    
    /// <summary>
    /// Show a specific quote with custom display duration
    /// </summary>
    public void ShowQuote(string quote, float customDisplayDuration)
    {
        Debug.Log($"[DeathQuoteManager] ShowQuote called with: '{quote}' and duration: {customDisplayDuration}s");
        
        if (string.IsNullOrEmpty(quote)) 
        {
            Debug.LogError("[DeathQuoteManager] Quote is empty! Cannot show quote.");
            return;
        }
        
        // Try TextMeshPro first, then fallback to regular Text
        if (quoteText != null)
        {
            quoteText.text = quote;
            Debug.Log($"[DeathQuoteManager] Quote text set to TextMeshPro: '{quoteText.text}'");
        }
        else if (quoteTextLegacy != null)
        {
            quoteTextLegacy.text = quote;
            Debug.Log($"[DeathQuoteManager] Quote text set to Text: '{quoteTextLegacy.text}'");
        }
        else
        {
            Debug.LogError("[DeathQuoteManager] No quote text component found! Assign either quoteText (TextMeshPro) or quoteTextLegacy (Text) in inspector.");
            return;
        }
        
        // Start fade in animation with custom duration
        if (canvasGroup != null)
        {
            Debug.Log($"[DeathQuoteManager] Starting fade in animation with duration: {customDisplayDuration}s");
            StartCoroutine(FadeInQuote(customDisplayDuration));
        }
        else
        {
            Debug.LogError("[DeathQuoteManager] CanvasGroup is null! Cannot start fade animation.");
        }
    }
    
    /// <summary>
    /// Hide the current quote
    /// </summary>
    public void HideQuote()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOutQuote());
        }
    }
    
    System.Collections.IEnumerator FadeInQuote()
    {
        yield return StartCoroutine(FadeInQuote(displayDuration));
    }
    
    System.Collections.IEnumerator FadeInQuote(float customDisplayDuration)
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // Auto-hide after custom display duration
        Debug.Log($"[DeathQuoteManager] Quote will display for {customDisplayDuration}s before fading out");
        yield return new WaitForSecondsRealtime(customDisplayDuration);
        StartCoroutine(FadeOutQuote());
    }
    
    System.Collections.IEnumerator FadeOutQuote()
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
    
    /// <summary>
    /// Get default death quotes
    /// </summary>
    List<string> GetDefaultQuotes()
    {
        return new List<string>
        {
            // Space-themed taunts
            "that's all you've got?",
            "did you forget how to navigate?",
            "that was... underwhelming.",
            "maybe stick to simpler games?",
            "did you forget the laws of physics?",
            
            // Additional space-themed taunts
            "the cosmos isn't impressed.",
            "space travel requires skill... you have none this time.",
            "the stars are watching... keep trying.",
            "maybe try a different galaxy?",
            "the universe has standards... maybe try again?",
            
            // Game-specific taunts
            "maybe try looking where you're going?",
            "should've anchored to something else!",
            "timing is everything... you had none.",
            "that anchor point was right there!",
            "maybe read the manual next time?",
            "the void is not your friend. don't let it catch up to you.",
            "gravity always wins in the end.",
            "failure comes easy... success takes work.",
            "you're out of time.",
            
            // Additional game-specific taunts
            "maybe the void was just hungry today?",
            
            // Sarcastic/Encouraging
            "well, that was... something.",
            "at least you tried!",
            "practice makes perfect... you need practice.",
            "every space explorer was once a beginner... you're still a beginner.",
            "the journey of a thousand light-years begins with one step... you tripped.",
            "success is not final, failure is not fatal... this was pretty fatal.",
            "you miss 100% of the shots you don't take... you missed this one too.",
            "the only way to do great work is to love what you do... keep trying.",
            
            // Additional sarcastic/encouraging
            "every expert was once a beginner... you're still a beginner.",
            "fortune favors the bold... you were just unlucky.",
            
            // Philosophical/Deep
            "in the end, we all return to the cosmic dust.",
            "the infinite scroll never lies... you were too slow.",
            "time waits for no one... especially not you.",
            "the thread has been severed.",
            "your journey through space has ended... prematurely.",
            "the cosmos is unforgiving to the unprepared.",
            
            // Additional philosophical/deep
            "the void consumes all...",
            "the cosmic dance continues... without you.",
            "the stars burn bright, but you burned out.",
            
            // Complementary/Encouraging (acknowledging good effort with room for improvement)
            "not bad... but there's always room for improvement.",
            "decent attempt... could use some refinement though.",
            "you're on the right track... just need to stick the landing.",
            "solid try... maybe work on the rhythm a bit?",
            "you're getting there... just not quite there yet.",
            "promising start... the finish needs work.",
            "you understand the concept... now master the execution.",
            "good attempt... the cosmos demands precision.",
            "you're learning...",
            "good thinking... just need better timing.",
            "practice makes perfect.",
            "try playing on your own music in the background!",
            "there is a tutorial in the main menu!",
            "check out the settings menu!",
            
            // Additional complementary/encouraging
            "nice effort... space navigation takes time to master.",
            "you're making progress... keep at it.",
            "good try... the void is tricky for everyone.",
            "decent work... anchor points can be tricky.",
            "you're getting the hang of it... almost there.",
            "solid attempt... timing will come with practice.",
            "good instincts... just need a bit more precision.",
            "you're learning the patterns... that's the first step.",
            "nice approach... execution will improve with time.",
            "you're on the right path... just need more practice.",
            "good effort... the cosmos rewards persistence.",
            "decent navigation... rhythm comes with experience.",
            "you're improving... each try teaches you something new.",
            "nice try... space exploration is challenging.",
        };
    }
    
    /// <summary>
    /// Add a custom quote to the list
    /// </summary>
    public void AddQuote(string quote)
    {
        if (!string.IsNullOrEmpty(quote) && !deathQuotes.Contains(quote))
        {
            deathQuotes.Add(quote);
        }
    }
    
    /// <summary>
    /// Clear all quotes and reset to defaults
    /// </summary>
    public void ResetToDefaults()
    {
        deathQuotes.Clear();
        deathQuotes.AddRange(GetDefaultQuotes());
    }
}
