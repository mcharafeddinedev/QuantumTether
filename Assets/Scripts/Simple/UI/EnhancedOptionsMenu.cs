using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

/// <summary>
/// Options menu controller with audio settings - the improved version
/// </summary>
public class EnhancedOptionsMenu : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField, Tooltip("Master volume slider")]
    private Slider masterVolumeSlider;
    
    [SerializeField, Tooltip("Music volume slider")]
    private Slider musicVolumeSlider;
    
    [SerializeField, Tooltip("SFX volume slider")]
    private Slider sfxVolumeSlider;
    
    [SerializeField, Tooltip("Audio mixer (optional)")]
    private AudioMixer audioMixer;
    
    [Header("UI References")]
    [SerializeField, Tooltip("Back button")]
    private Button backButton;
    
    [SerializeField, Tooltip("Reset button")]
    private Button resetButton;
    
    [Header("Resolution Settings")]
    [SerializeField, Tooltip("Resolution manager component")]
    private ResolutionManager resolutionManager;
    
    [Header("Audio")]
    [SerializeField, Tooltip("Button click sound")]
    private AudioClip buttonClickSound;
    
    [SerializeField, Tooltip("Audio source for button sounds")]
    private AudioSource audioSource;
    
    [SerializeField, Tooltip("Button sound volume multiplier")]
    private float buttonVolume = 1f;
    
    [Header("Settings")]
    [SerializeField, Tooltip("Save settings to PlayerPrefs")]
    private bool saveToPlayerPrefs = true;
    
    
    void Start()
    {
        // Wait a frame to ensure EnhancedMusicManager is ready
        StartCoroutine(InitializeAfterDelay());
    }
    
    private System.Collections.IEnumerator InitializeAfterDelay()
    {
        // Wait one frame to ensure EnhancedMusicManager is ready
        yield return null;
        
        EnsureAudioSource();
        InitializeSettings();
        SetupUI();
    }
    
    void InitializeSettings()
    {
        // Load saved settings
        LoadSettings();
        
        // Initialize resolution manager
        InitializeResolutionManager();
    }
    
    void SetupUI()
    {
        // Audio sliders
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);
        
        // Buttons
        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
            
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetSettings);
    }
    
    void InitializeResolutionManager()
    {
        // Find resolution manager if not assigned
        if (resolutionManager == null)
        {
            // First try to find it on the same GameObject
            resolutionManager = GetComponent<ResolutionManager>();
            
            // If not found, try to find it on parent objects
            if (resolutionManager == null)
            {
                Transform parent = transform.parent;
                while (parent != null && resolutionManager == null)
                {
                    resolutionManager = parent.GetComponent<ResolutionManager>();
                    parent = parent.parent;
                }
            }
            
            // If still not found, search the entire scene
            if (resolutionManager == null)
            {
                resolutionManager = FindFirstObjectByType<ResolutionManager>();
            }
            
            // Last resort: try to find it by name
            if (resolutionManager == null)
            {
                GameObject optionsPanel = GameObject.Find("OptionsPanel");
                if (optionsPanel != null)
                {
                    resolutionManager = optionsPanel.GetComponent<ResolutionManager>();
                }
            }
        }
        
        if (resolutionManager == null)
        {
            Debug.LogWarning("[EnhancedOptionsMenu] ResolutionManager not found! Resolution settings will not be available.");
            Debug.LogWarning("[EnhancedOptionsMenu] Make sure ResolutionManager component is attached to your Options Panel GameObject.");
            Debug.LogWarning("[EnhancedOptionsMenu] Also make sure to assign the ResolutionManager to the 'Resolution Manager' field in EnhancedOptionsMenu component.");
        }
        else
        {
            Debug.Log("[EnhancedOptionsMenu] ResolutionManager initialized successfully.");
            Debug.Log($"[EnhancedOptionsMenu] Found ResolutionManager on GameObject: {resolutionManager.gameObject.name}");
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
    
    public void SetMasterVolume(float volume)
    {
        // Update music manager if available
        if (EnhancedMusicManager.Instance != null)
        {
            EnhancedMusicManager.Instance.SetMasterVolume(volume);
        }
        else
        {
            // Fallback to direct AudioListener control
            AudioListener.volume = volume;
            if (saveToPlayerPrefs)
                PlayerPrefs.SetFloat("MasterVolume", volume);
        }
        
        // Also update audio mixer if available
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
        }
    }
    
    public void SetMusicVolume(float volume)
    {
        // Update music manager if available
        if (EnhancedMusicManager.Instance != null)
        {
            EnhancedMusicManager.Instance.SetMusicVolume(volume);
        }
        
        // Also update audio mixer if available
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
        }
        
        if (saveToPlayerPrefs)
            PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        // Update music manager if available
        if (EnhancedMusicManager.Instance != null)
        {
            EnhancedMusicManager.Instance.SetSFXVolume(volume);
        }
        else
        {
            // Fallback to direct PlayerPrefs save
            if (saveToPlayerPrefs)
                PlayerPrefs.SetFloat("SFXVolume", volume);
        }
        
        // Also update audio mixer if available
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
        }
    }
    
    public void ResetSettings()
    {
        PlayButtonSound();
        Debug.Log("[EnhancedOptionsMenu] Reset button clicked!");
        
        // Reset to hardcoded default values
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 0.5f; // Hardcoded master volume
            Debug.Log("[EnhancedOptionsMenu] Resetting master volume to hardcoded value: 0.50");
            SetMasterVolume(0.5f);
        }
            
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.2f; // Hardcoded music volume
            Debug.Log("[EnhancedOptionsMenu] Resetting music volume to hardcoded value: 0.2");
            SetMusicVolume(0.2f);
        }
            
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 0.8f; // Hardcoded SFX volume
            Debug.Log("[EnhancedOptionsMenu] Resetting SFX volume to hardcoded value: 0.8");
            SetSFXVolume(0.8f);
        }
        
        // Reset resolution settings
        if (resolutionManager != null)
        {
            resolutionManager.ResetToDefault();
            Debug.Log("[EnhancedOptionsMenu] Reset resolution to default");
        }
    }
    
    public void GoBack()
    {
        PlayButtonSound();
        
        // Find the main menu controller and show main menu
        EnhancedMainMenu mainMenu = FindFirstObjectByType<EnhancedMainMenu>();
        if (mainMenu != null)
        {
            mainMenu.ShowMainMenu();
        }
        else
        {
        }
    }
    
    void LoadSettings()
    {
        if (!saveToPlayerPrefs) return;
        
        // For development: Always use default values on fresh start
        // Comment out the next 4 lines if you want settings to persist between editor sessions
        // PlayerPrefs.DeleteKey("MusicVolume");
        // PlayerPrefs.DeleteKey("MasterVolume");
        // PlayerPrefs.DeleteKey("SFXVolume");
        // PlayerPrefs.Save();
        
        // Set hardcoded default values
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = 0.5f; // Hardcoded master volume
            Debug.Log("[EnhancedOptionsMenu] Master volume set to hardcoded value: 0.50");
            SetMasterVolume(0.5f);
        }
        else
        {
            Debug.LogWarning("[EnhancedOptionsMenu] Master volume slider is null!");
        }
            
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = 0.2f; // Hardcoded music volume
            Debug.Log("[EnhancedOptionsMenu] Music volume set to hardcoded value: 0.2 (20%)");
            SetMusicVolume(0.2f);
        }
        else
        {
            Debug.LogWarning("[EnhancedOptionsMenu] Music volume slider is null!");
        }
            
        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = 0.8f; // Hardcoded SFX volume
            Debug.Log("[EnhancedOptionsMenu] SFX volume set to hardcoded value: 0.8");
            SetSFXVolume(0.8f);
        }
        else
        {
            Debug.LogWarning("[EnhancedOptionsMenu] SFX volume slider is null!");
        }
    }
    
    void OnDestroy()
    {
        // Clean up listeners
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);
            
        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            
        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);
        
        if (backButton != null)
            backButton.onClick.RemoveListener(GoBack);
            
        if (resetButton != null)
            resetButton.onClick.RemoveListener(ResetSettings);
    }
}
