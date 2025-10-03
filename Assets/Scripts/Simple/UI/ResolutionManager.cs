using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages screen resolution settings with dropdown support
/// Supports common resolutions up to 3440x1440 ultrawide
/// </summary>
public class ResolutionManager : MonoBehaviour
{
    [System.Serializable]
    public class ResolutionOption
    {
        public int width;
        public int height;
        public string displayName;
        public bool isUltrawide;
        
        public ResolutionOption(int w, int h, bool ultrawide = false)
        {
            width = w;
            height = h;
            isUltrawide = ultrawide;
            displayName = $"{w}x{h}";
        }
    }
    
    [Header("UI References")]
    [SerializeField, Tooltip("Resolution dropdown component (TMP)")]
    private TMP_Dropdown resolutionDropdown;
    
    [SerializeField, Tooltip("Fullscreen toggle")]
    private Toggle fullscreenToggle;
    
    [Header("Settings")]
    [SerializeField, Tooltip("Save resolution settings to PlayerPrefs")]
    private bool saveToPlayerPrefs = true;
    
    [SerializeField, Tooltip("Apply resolution immediately when changed")]
    private bool applyImmediately = true;
    
    [Header("Audio")]
    [SerializeField, Tooltip("Button click sound")]
    private AudioClip buttonClickSound;
    
    [SerializeField, Tooltip("Audio source for button sounds")]
    private AudioSource audioSource;
    
    [SerializeField, Tooltip("Button sound volume multiplier")]
    private float buttonVolume = 1f;
    
    // Common resolutions including ultrawide support
    private readonly List<ResolutionOption> commonResolutions = new List<ResolutionOption>
    {
        // Standard resolutions
        new ResolutionOption(640, 480),
        new ResolutionOption(800, 600),
        new ResolutionOption(1024, 768),
        new ResolutionOption(1280, 720),
        new ResolutionOption(1280, 1024),
        new ResolutionOption(1366, 768),
        new ResolutionOption(1440, 900),
        new ResolutionOption(1600, 900),
        new ResolutionOption(1680, 1050),
        new ResolutionOption(1920, 1080),
        new ResolutionOption(1920, 1200),
        new ResolutionOption(2560, 1440),
        new ResolutionOption(2560, 1600),
        new ResolutionOption(3840, 2160),
        
        // Ultrawide resolutions
        new ResolutionOption(2560, 1080, true),
        new ResolutionOption(3440, 1440, true),
        new ResolutionOption(5120, 1440, true),
    };
    
    private Resolution currentResolution;
    private bool isFullscreen;
    private int currentResolutionIndex = -1;
    
    void Start()
    {
        EnsureAudioSource();
        InitializeResolutionSettings();
        SetupUI();
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
    
    void InitializeResolutionSettings()
    {
        // Get current resolution
        currentResolution = Screen.currentResolution;
        isFullscreen = Screen.fullScreen;
        
        // Load saved settings
        LoadResolutionSettings();
        
        // Populate dropdown with available resolutions
        PopulateResolutionDropdown();
        
        // Set current resolution in dropdown
        SetCurrentResolutionInDropdown();
    }
    
    void LoadResolutionSettings()
    {
        if (!saveToPlayerPrefs) return;
        
        // Default to 1920x1080 fullscreen if no saved settings
        int defaultWidth = 1920;
        int defaultHeight = 1080;
        bool defaultFullscreen = true;
        
        // Load saved resolution or use defaults
        int savedWidth = PlayerPrefs.GetInt("ResolutionWidth", defaultWidth);
        int savedHeight = PlayerPrefs.GetInt("ResolutionHeight", defaultHeight);
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", defaultFullscreen ? 1 : 0) == 1;
        
        // Apply saved settings
        ApplyResolution(savedWidth, savedHeight, savedFullscreen);
        
        Debug.Log($"[ResolutionManager] Loaded resolution: {savedWidth}x{savedHeight}, Fullscreen: {savedFullscreen}");
    }
    
    void SaveResolutionSettings()
    {
        if (!saveToPlayerPrefs) return;
        
        PlayerPrefs.SetInt("ResolutionWidth", currentResolution.width);
        PlayerPrefs.SetInt("ResolutionHeight", currentResolution.height);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
        
        Debug.Log($"[ResolutionManager] Saved resolution: {currentResolution.width}x{currentResolution.height}, Fullscreen: {isFullscreen}");
    }
    
    void PopulateResolutionDropdown()
    {
        if (resolutionDropdown == null) return;
        
        // Clear existing options
        resolutionDropdown.ClearOptions();
        
        // Create list of resolution names
        List<string> resolutionNames = new List<string>();
        
        foreach (var res in commonResolutions)
        {
            resolutionNames.Add(res.displayName);
        }
        
        // Add dropdown options
        resolutionDropdown.AddOptions(resolutionNames);
        
        // Force refresh the dropdown to fix TMP text rendering issues
        resolutionDropdown.RefreshShownValue();
        
        Debug.Log($"[ResolutionManager] Populated dropdown with {resolutionNames.Count} resolutions");
    }
    
    void SetCurrentResolutionInDropdown()
    {
        if (resolutionDropdown == null) return;
        
        // Find matching resolution in our list
        for (int i = 0; i < commonResolutions.Count; i++)
        {
            if (commonResolutions[i].width == currentResolution.width && 
                commonResolutions[i].height == currentResolution.height)
            {
                currentResolutionIndex = i;
                resolutionDropdown.value = i;
                break;
            }
        }
        
        // If no exact match found, find closest resolution
        if (currentResolutionIndex == -1)
        {
            currentResolutionIndex = FindClosestResolution(currentResolution.width, currentResolution.height);
            if (currentResolutionIndex >= 0)
            {
                resolutionDropdown.value = currentResolutionIndex;
            }
        }
        
        Debug.Log($"[ResolutionManager] Set dropdown to index {currentResolutionIndex} for {currentResolution.width}x{currentResolution.height}");
    }
    
    int FindClosestResolution(int width, int height)
    {
        int closestIndex = 0;
        int smallestDifference = int.MaxValue;
        
        for (int i = 0; i < commonResolutions.Count; i++)
        {
            var res = commonResolutions[i];
            int difference = Mathf.Abs(res.width - width) + Mathf.Abs(res.height - height);
            
            if (difference < smallestDifference)
            {
                smallestDifference = difference;
                closestIndex = i;
            }
        }
        
        return closestIndex;
    }
    
    void SetupUI()
    {
        // Resolution dropdown
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
        }
        
        // Fullscreen toggle
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = isFullscreen;
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }
    }
    
    public void OnResolutionChanged(int index)
    {
        PlayButtonSound();
        
        if (index < 0 || index >= commonResolutions.Count)
        {
            Debug.LogWarning($"[ResolutionManager] Invalid resolution index: {index}");
            return;
        }
        
        var selectedResolution = commonResolutions[index];
        currentResolutionIndex = index;
        
        Debug.Log($"[ResolutionManager] Resolution changed to: {selectedResolution.displayName}");
        
        // Always apply immediately - no confirmation needed
        ApplyResolution(selectedResolution.width, selectedResolution.height, isFullscreen);
        
        // Force refresh dropdown to fix any text rendering issues
        if (resolutionDropdown != null)
        {
            resolutionDropdown.RefreshShownValue();
        }
    }
    
    public void OnFullscreenChanged(bool fullscreen)
    {
        PlayButtonSound();
        
        isFullscreen = fullscreen;
        
        Debug.Log($"[ResolutionManager] Fullscreen changed to: {fullscreen}");
        
        if (applyImmediately)
        {
            ApplyResolution(currentResolution.width, currentResolution.height, fullscreen);
        }
    }
    
    void ApplyResolution(int width, int height, bool fullscreen)
    {
        try
        {
            // Apply the resolution
            Screen.SetResolution(width, height, fullscreen);
            
            // Update current resolution
            currentResolution = new Resolution { width = width, height = height };
            
            // Save settings
            SaveResolutionSettings();
            
            Debug.Log($"[ResolutionManager] Applied resolution: {width}x{height}, Fullscreen: {fullscreen}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ResolutionManager] Failed to apply resolution {width}x{height}: {e.Message}");
        }
    }
    
    /// <summary>
    /// Apply resolution without saving (for testing)
    /// </summary>
    public void ApplyResolutionTemporary(int width, int height, bool fullscreen)
    {
        try
        {
            Screen.SetResolution(width, height, fullscreen);
            Debug.Log($"[ResolutionManager] Temporarily applied resolution: {width}x{height}, Fullscreen: {fullscreen}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ResolutionManager] Failed to temporarily apply resolution {width}x{height}: {e.Message}");
        }
    }
    
    /// <summary>
    /// Reset to default resolution
    /// </summary>
    public void ResetToDefault()
    {
        PlayButtonSound();
        
        // Default to 1920x1080 fullscreen
        int defaultWidth = 1920;
        int defaultHeight = 1080;
        bool defaultFullscreen = true;
        
        // Find the default resolution in our list
        for (int i = 0; i < commonResolutions.Count; i++)
        {
            if (commonResolutions[i].width == defaultWidth && commonResolutions[i].height == defaultHeight)
            {
                currentResolutionIndex = i;
                if (resolutionDropdown != null)
                    resolutionDropdown.value = i;
                break;
            }
        }
        
        ApplyResolution(defaultWidth, defaultHeight, defaultFullscreen);
        
        if (fullscreenToggle != null)
            fullscreenToggle.isOn = defaultFullscreen;
        
        Debug.Log($"[ResolutionManager] Reset to default resolution: {defaultWidth}x{defaultHeight}, Fullscreen: {defaultFullscreen}");
    }
    
    /// <summary>
    /// Get current resolution info
    /// </summary>
    public Resolution GetCurrentResolution()
    {
        return currentResolution;
    }
    
    /// <summary>
    /// Get current fullscreen state
    /// </summary>
    public bool IsFullscreen()
    {
        return isFullscreen;
    }
    
    void OnDestroy()
    {
        // Clean up listeners
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
    }
}
