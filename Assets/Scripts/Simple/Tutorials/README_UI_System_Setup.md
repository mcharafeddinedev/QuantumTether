# UI System Setup - Reference Guide

## What This Document Contains

This document explains how to build a UI system that handles menus, settings, pause screens, and death panels. It covers navigation between screens, persistent settings, and how UI connects with other game systems.

## The UI System Overview

### What Each Part Does
- **EnhancedMainMenu.cs** - Controls main menu navigation
- **EnhancedOptionsMenu.cs** - Handles settings and volume controls
- **EnhancedCreditsMenu.cs** - Shows credits screen
- **EnhancedScore.cs** - Manages death panel and pause screen
- **ResolutionManager.cs** - Handles screen resolution changes
- **DeathQuoteManager.cs** - Shows random death quotes

### How It Works
1. **Menu Navigation** flows from main menu to options to credits
2. **Game UI** shows pause screen on Escape and death panel when player dies
3. **Settings** are saved and loaded automatically
4. **Resolution Changes** apply immediately with confirmation

## How to Build Your Own UI System (Step by Step)

### Step 1: Create the Main Menu
This script handles navigation between different menu screens.

```csharp
public class EnhancedMainMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioSource audioSource;
    
    void Start()
    {
        SetupButtons();
        EnsureAudioSource();
    }
    
    void SetupButtons()
    {
        if (playButton) playButton.onClick.AddListener(OnClickPlay);
        if (optionsButton) optionsButton.onClick.AddListener(OnClickOptions);
        if (creditsButton) creditsButton.onClick.AddListener(OnClickCredits);
        if (quitButton) quitButton.onClick.AddListener(OnClickQuit);
    }
}
```

### Step 2: Options Menu with Settings
```csharp
// EnhancedOptionsMenu.cs - Settings management
public class EnhancedOptionsMenu : MonoBehaviour
{
    [Header("Volume Controls")]
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    
    [Header("Resolution")]
    [SerializeField] private Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    
    void Start()
    {
        LoadSettings();
        SetupResolutionDropdown();
        SetupVolumeSliders();
    }
    
    void LoadSettings()
    {
        // Load from PlayerPrefs
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        
        musicVolumeSlider.value = musicVol;
        sfxVolumeSlider.value = sfxVol;
        fullscreenToggle.isOn = fullscreen;
    }
}
```

### Step 3: In-Game UI Management
```csharp
// EnhancedScore.cs - Death panel and pause screen
public class EnhancedScore : MonoBehaviour
{
    [Header("Death Panel")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button quitButton;
    
    [Header("Pause Screen")]
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private Button pauseRestartButton;
    [SerializeField] private Button pauseQuitButton;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeKey();
        }
    }
    
    void HandleEscapeKey()
    {
        if (deathPanel != null && deathPanel.activeInHierarchy)
        {
            ToggleDeathPanelButtons();
        }
        else if (!isScoreFrozen)
        {
            TogglePauseScreen();
        }
    }
}
```

## Key Patterns I Used

### 1. Button Setup Pattern
```csharp
// Always check for null and setup listeners
void SetupButtons()
{
    if (playButton) playButton.onClick.AddListener(OnClickPlay);
    if (optionsButton) optionsButton.onClick.AddListener(OnClickOptions);
    // etc...
}
```

### 2. Audio Integration Pattern
```csharp
// Play sound on button click
void OnClickButton()
{
    PlayButtonSound();
    // Do button action
}

void PlayButtonSound()
{
    EnsureAudioSource();
    if (audioSource && buttonClickSound)
    {
        float sfxVolume = EnhancedMusicManager.Instance != null ? 
            EnhancedMusicManager.Instance.GetSFXVolume() : 0.8f;
        audioSource.PlayOneShot(buttonClickSound, sfxVolume);
    }
}
```

### 3. Settings Persistence Pattern
```csharp
// Save settings when changed
public void OnMusicVolumeChanged(float volume)
{
    PlayerPrefs.SetFloat("MusicVolume", volume);
    PlayerPrefs.Save();
    EnhancedMusicManager.Instance?.SetMusicVolume(volume);
}

// Load settings on start
void LoadSettings()
{
    float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
    musicVolumeSlider.value = musicVol;
}
```

### 4. Resolution Management Pattern
```csharp
// ResolutionManager.cs - Handle resolution changes
public class ResolutionManager : MonoBehaviour
{
    Resolution[] resolutions;
    
    void Start()
    {
        resolutions = Screen.resolutions;
        SetupResolutionDropdown();
    }
    
    void SetupResolutionDropdown()
    {
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            
            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }
        
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }
}
```

## What I Learned

### Good Practices
- **Always check for null** before accessing UI elements
- **Use consistent naming** for UI elements (playButton, optionsButton, etc.)
- **Group related UI** in the same script
- **Save settings immediately** when changed
- **Provide default values** for all settings

### Common Pitfalls
- **Don't forget to call PlayerPrefs.Save()** after setting values
- **Check if UI elements exist** before trying to use them
- **Handle resolution changes** properly (some platforms are picky)
- **Test on different screen sizes** - UI can break on different resolutions

### Performance Tips
- **Use object pooling** for frequently created/destroyed UI elements
- **Cache UI references** instead of finding them every frame
- **Disable unused UI** instead of destroying it
- **Use Canvas Groups** for efficient show/hide operations

## UI Flow Patterns

### Main Menu Navigation
```
Main Menu → Play (loads game scene)
         → Options (shows options menu)
         → Credits (shows credits)
         → Quit (exits game)
```

### In-Game UI Flow
```
Game Playing → Escape → Pause Screen → Resume/Restart/Quit
            → Death → Death Panel → Restart/Quit/Main Menu
```

### Options Menu Flow
```
Options → Volume Controls (immediate effect)
       → Resolution (apply on change)
       → Graphics (apply on change)
       → Back to Main Menu
```

## Integration with Other Systems

### Audio System Integration
```csharp
// Volume sliders directly control audio system
public void OnMusicVolumeChanged(float volume)
{
    EnhancedMusicManager.Instance?.SetMusicVolume(volume);
}
```

### Game State Integration
```csharp
// UI responds to game state changes
void OnPlayerDeath()
{
    ShowDeathPanel();
}

void OnGameRestart()
{
    HideDeathPanel();
    HidePauseScreen();
}
```

## Future Improvements

### What I Could Add Next Time
- **UI animations** for smoother transitions
- **Accessibility options** (colorblind support, text size)
- **Controller support** for UI navigation
- **Localization system** for multiple languages
- **UI theme system** for different visual styles

### Advanced Features
- **Dynamic UI scaling** for different screen sizes
- **UI state machine** for complex menu flows
- **UI analytics** (track which options are used most)
- **Custom UI components** for specific game needs

## Quick Setup Checklist

For future projects, here's my quick UI setup:

1. Create main menu with navigation buttons
2. Create options menu with volume controls
3. Add resolution management
4. Implement settings persistence
5. Create in-game pause screen
6. Create death panel with restart options
7. Add audio feedback for all buttons
8. Test on different resolutions
9. Implement proper UI flow
10. Add error handling for missing UI elements

This UI system worked really well for Quantum Thread and should work for most games I'll make in the future.
