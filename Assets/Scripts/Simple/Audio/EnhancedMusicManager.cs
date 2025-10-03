using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages background music that plays continuously across scenes
/// This is the improved version that actually works well
/// Volume is controlled by the options menu music slider
/// </summary>
public class EnhancedMusicManager : MonoBehaviour
{
    [Header("Music Tracks")]
    [SerializeField, Tooltip("Main menu background music")]
    private AudioClip mainMenuMusic;
    
    [SerializeField, Tooltip("Gameplay background music")]
    private AudioClip gameplayMusic;
    
    [Header("Audio Settings")]
    [SerializeField, Tooltip("Music volume (0-1)")]
    [Range(0f, 1f)]
    private float musicVolume = 0.125f; // Hardcoded to 0.125
    
    [SerializeField, Tooltip("Master volume (0-1)")]
    [Range(0f, 1f)]
    private float masterVolume = 0.5f; // Hardcoded to 0.5
    
    [SerializeField, Tooltip("SFX volume (0-1)")]
    [Range(0f, 1f)]
    private float sfxVolume = 0.75f; // Hardcoded to 0.75
    
    [SerializeField, Tooltip("Audio source for music")]
    private AudioSource musicSource;
    
    [SerializeField, Tooltip("Fade in duration (seconds)")]
    private float fadeInDuration = 2f;
    
    [SerializeField, Tooltip("Fade out duration (seconds)")]
    private float fadeOutDuration = 1f;
    
    [SerializeField, Tooltip("Pause between music loops (seconds)")]
    private float loopPauseDuration = 0.5f;
    
    // Singleton instance
    public static EnhancedMusicManager Instance { get; private set; }
    
    // Current music state
    private AudioClip currentMusic;
    private bool isMusicEnabled = true;
    private Coroutine fadeCoroutine;
    private Coroutine loopCoroutine;
    
    // Scene names for music selection
    private const string MAIN_MENU_SCENE = "_MainMenu";
    private const string GAMEPLAY_SCENE = "Game"; // Adjust this to your gameplay scene name
    
    void Awake()
    {
        // Singleton pattern - only one music manager should exist
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeMusicManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Load saved volumes from PlayerPrefs
        LoadMasterVolume();
        LoadMusicVolume();
        LoadSFXVolume();
        
        // Start playing music for current scene
        PlayMusicForCurrentScene();
        
        Debug.Log("[EnhancedMusicManager] Music manager started successfully");
    }
    
    void OnEnable()
    {
        // Subscribe to scene loaded event
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        // Unsubscribe from scene loaded event
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// Initialize the music manager components
    /// </summary>
    private void InitializeMusicManager()
    {
        // Create audio source if not assigned
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Configure audio source for music
        musicSource.loop = false; // We'll handle looping manually
        musicSource.playOnAwake = false;
        musicSource.volume = 0f; // Start at 0 for fade-in
        musicSource.priority = 0; // High priority for music
        musicSource.spatialBlend = 0f; // 2D audio
        musicSource.rolloffMode = AudioRolloffMode.Linear;
        
        Debug.Log("[EnhancedMusicManager] Music manager initialized successfully");
    }
    
    /// <summary>
    /// Called when a new scene is loaded
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForCurrentScene();
    }
    
    /// <summary>
    /// Play appropriate music for the current scene
    /// </summary>
    private void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        AudioClip targetMusic = null;
        
        Debug.Log($"[EnhancedMusicManager] Current scene: '{currentSceneName}'");
        
        // Determine which music to play based on scene
        if (currentSceneName == MAIN_MENU_SCENE)
        {
            targetMusic = mainMenuMusic;
            Debug.Log("[EnhancedMusicManager] Playing main menu music");
        }
        else if (currentSceneName == GAMEPLAY_SCENE)
        {
            targetMusic = gameplayMusic;
            Debug.Log("[EnhancedMusicManager] Playing gameplay music");
        }
        else
        {
            // For other scenes, try to determine if it's gameplay or menu
            if (currentSceneName.ToLower().Contains("menu"))
            {
                targetMusic = mainMenuMusic;
                Debug.Log("[EnhancedMusicManager] Detected menu scene, playing main menu music");
            }
            else
            {
                targetMusic = gameplayMusic;
                Debug.Log("[EnhancedMusicManager] Detected gameplay scene, playing gameplay music");
            }
        }
        
        // Always play music if we have a target, even if it's the same
        if (targetMusic != null)
        {
            if (targetMusic != currentMusic)
            {
                Debug.Log($"[EnhancedMusicManager] Switching music from '{currentMusic?.name}' to '{targetMusic.name}'");
                PlayMusic(targetMusic);
            }
            else
            {
                Debug.Log($"[EnhancedMusicManager] Same music already playing: '{targetMusic.name}', ensuring it loops");
                // Make sure the music is still looping even if it's the same
                if (!musicSource.isPlaying && isMusicEnabled)
                {
                    Debug.Log("[EnhancedMusicManager] Music stopped, restarting loop");
                    if (loopCoroutine != null)
                    {
                        StopCoroutine(loopCoroutine);
                    }
                    loopCoroutine = StartCoroutine(PlayMusicWithLoop());
                }
            }
        }
        else
        {
            Debug.LogWarning("[EnhancedMusicManager] No target music found for current scene!");
        }
    }
    
    /// <summary>
    /// Play the specified music track with fade-in
    /// </summary>
    private void PlayMusic(AudioClip musicClip)
    {
        if (musicClip == null) 
        {
            Debug.LogWarning("[EnhancedMusicManager] Cannot play null music clip!");
            return;
        }
        
        Debug.Log($"[EnhancedMusicManager] Playing music: '{musicClip.name}' (length: {musicClip.length:F2}s)");
        
        // Stop any current fade
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }
        
        currentMusic = musicClip;
        musicSource.clip = musicClip;
        
        // Always ensure volume starts at 0 to prevent volume jumps
        musicSource.volume = 0f;
        
        if (isMusicEnabled)
        {
            // Stop any existing loop
            if (loopCoroutine != null)
            {
                StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
            
            // Start custom loop with pause
            loopCoroutine = StartCoroutine(PlayMusicWithLoop());
            Debug.Log("[EnhancedMusicManager] Started music loop coroutine");
        }
        else
        {
            Debug.LogWarning("[EnhancedMusicManager] Music is disabled, not starting playback");
        }
    }
    
    /// <summary>
    /// Fade music volume over time
    /// </summary>
    private System.Collections.IEnumerator FadeMusic(float startVolume, float endVolume, float duration)
    {
        float timer = 0f;
        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            // Use smooth step for more natural fade curve
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);
            musicSource.volume = Mathf.Lerp(startVolume, endVolume, smoothProgress);
            yield return null;
        }
        
        musicSource.volume = endVolume;
        fadeCoroutine = null;
    }
    
    /// <summary>
    /// Play music with custom loop and pause
    /// </summary>
    private System.Collections.IEnumerator PlayMusicWithLoop()
    {
        Debug.Log("[EnhancedMusicManager] Starting music loop coroutine");
        
        while (isMusicEnabled && currentMusic != null)
        {
            Debug.Log($"[EnhancedMusicManager] Playing music loop iteration - '{currentMusic.name}'");
            
            // Ensure volume is 0 before playing to prevent volume jump
            musicSource.volume = 0f;
            
            // Start the music
            musicSource.Play();
            
            // Start fade-in immediately from 0 to target volume (music volume * master volume)
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }
            float targetVolume = musicVolume * masterVolume;
            fadeCoroutine = StartCoroutine(FadeMusic(0f, targetVolume, fadeInDuration));
            
            Debug.Log($"[EnhancedMusicManager] AudioSource.isPlaying: {musicSource.isPlaying}, Volume: {musicSource.volume:F2}, Target: {targetVolume:F2}");
            
            // Wait for the music to finish playing
            Debug.Log($"[EnhancedMusicManager] Waiting for music to finish ({currentMusic.length:F2}s)");
            yield return new WaitForSeconds(currentMusic.length);
            
            // Wait for the pause duration before looping
            Debug.Log($"[EnhancedMusicManager] Music finished, waiting {loopPauseDuration:F2}s before next loop");
            yield return new WaitForSeconds(loopPauseDuration);
            
            Debug.Log("[EnhancedMusicManager] Starting next loop iteration");
        }
        
        Debug.Log("[EnhancedMusicManager] Music loop coroutine ended");
        loopCoroutine = null;
    }
    
    /// <summary>
    /// Set the music volume (called by options menu)
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        Debug.Log($"[EnhancedMusicManager] Setting music volume to: {musicVolume:F2}");
        
        // Apply volume to AudioSource if it exists (regardless of playing state)
        if (musicSource != null)
        {
            // Apply master volume to the music volume
            musicSource.volume = musicVolume * masterVolume;
            Debug.Log($"[EnhancedMusicManager] Applied volume to AudioSource: {musicSource.volume:F2} (Music: {musicVolume:F2} * Master: {masterVolume:F2})");
        }
        else
        {
            Debug.Log("[EnhancedMusicManager] MusicSource not ready yet, volume will be applied when music starts");
        }
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Get the current music volume
    /// </summary>
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    /// <summary>
    /// Set the master volume (called by options menu)
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        Debug.Log($"[EnhancedMusicManager] Setting master volume to: {masterVolume:F2}");
        
        // Apply master volume to AudioListener (affects all audio)
        AudioListener.volume = masterVolume;
        Debug.Log($"[EnhancedMusicManager] Applied master volume to AudioListener: {AudioListener.volume:F2}");
        
        // Reapply music volume with new master volume
        if (musicSource != null)
        {
            musicSource.volume = musicVolume * masterVolume;
            Debug.Log($"[EnhancedMusicManager] Updated music volume with new master: {musicSource.volume:F2}");
        }
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("MasterVolume", masterVolume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Get the current master volume
    /// </summary>
    public float GetMasterVolume()
    {
        return masterVolume;
    }
    
    /// <summary>
    /// Set the SFX volume (called by options menu)
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        Debug.Log($"[EnhancedMusicManager] Setting SFX volume to: {sfxVolume:F2}");
        
        // SFX volume is applied by individual SFX systems (like AudioHelper)
        // This just stores the value for them to use
        
        // Save to PlayerPrefs
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Get the current SFX volume
    /// </summary>
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    
    /// <summary>
    /// Enable or disable music playback
    /// </summary>
    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        
        if (enabled && currentMusic != null)
        {
            musicSource.Play();
        }
        else
        {
            musicSource.Stop();
        }
        
    }
    
    /// <summary>
    /// Check if music is currently enabled
    /// </summary>
    public bool IsMusicEnabled()
    {
        return isMusicEnabled;
    }
    
    /// <summary>
    /// Load music volume from PlayerPrefs
    /// </summary>
    private void LoadMusicVolume()
    {
        // Load music volume from PlayerPrefs, default to 0.25f if not set
        musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.25f);
        
        // Don't apply volume immediately - let the fade-in handle it
        // This ensures music starts at 0 and fades to the target volume
        if (musicSource != null)
        {
            musicSource.volume = 0f; // Start at 0 for proper fade-in
        }
        
        Debug.Log($"[EnhancedMusicManager] Loaded music volume: {musicVolume:F2} (will fade in to this volume)");
    }
    
    /// <summary>
    /// Load master volume from PlayerPrefs
    /// </summary>
    private void LoadMasterVolume()
    {
        // Load master volume from PlayerPrefs, default to 0.75f if not set
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.75f);
        
        // Apply master volume to AudioListener
        AudioListener.volume = masterVolume;
        
        Debug.Log($"[EnhancedMusicManager] Loaded master volume: {masterVolume:F2}");
    }
    
    /// <summary>
    /// Load SFX volume from PlayerPrefs
    /// </summary>
    private void LoadSFXVolume()
    {
        // Load SFX volume from PlayerPrefs, default to 0.75f if not set
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.75f);
        
        // SFX volume is applied by individual SFX systems
        // This just loads the value for them to use
        
        Debug.Log($"[EnhancedMusicManager] Loaded SFX volume: {sfxVolume:F2}");
    }
    
    /// <summary>
    /// Force play main menu music (for testing)
    /// </summary>
    [ContextMenu("Play Main Menu Music")]
    public void PlayMainMenuMusic()
    {
        if (mainMenuMusic != null)
        {
            PlayMusic(mainMenuMusic);
        }
    }
    
    /// <summary>
    /// Force play gameplay music (for testing)
    /// </summary>
    [ContextMenu("Play Gameplay Music")]
    public void PlayGameplayMusic()
    {
        if (gameplayMusic != null)
        {
            PlayMusic(gameplayMusic);
        }
    }
    
    /// <summary>
    /// Stop all music with fade-out
    /// </summary>
    [ContextMenu("Stop Music")]
    public void StopMusic()
    {
        // Stop loop coroutine
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }
        
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        
        fadeCoroutine = StartCoroutine(FadeMusic(musicSource.volume, 0f, fadeOutDuration));
        currentMusic = null;
    }
    
    /// <summary>
    /// Force restart the current music loop (for debugging)
    /// </summary>
    [ContextMenu("Restart Music Loop")]
    public void RestartMusicLoop()
    {
        Debug.Log("[EnhancedMusicManager] Force restarting music loop");
        
        if (currentMusic != null && isMusicEnabled)
        {
            // Stop any existing loop
            if (loopCoroutine != null)
            {
                StopCoroutine(loopCoroutine);
                loopCoroutine = null;
            }
            
            // Start new loop
            loopCoroutine = StartCoroutine(PlayMusicWithLoop());
        }
        else
        {
            Debug.LogWarning("[EnhancedMusicManager] Cannot restart loop - no current music or music disabled");
        }
    }
    
    /// <summary>
    /// Debug info about current music state
    /// </summary>
    [ContextMenu("Debug Music State")]
    public void DebugMusicState()
    {
        Debug.Log($"[EnhancedMusicManager] === MUSIC DEBUG INFO ===");
        Debug.Log($"Current Music: {(currentMusic != null ? currentMusic.name : "None")}");
        Debug.Log($"Music Enabled: {isMusicEnabled}");
        Debug.Log($"AudioSource Playing: {musicSource.isPlaying}");
        Debug.Log($"AudioSource Volume: {musicSource.volume:F2}");
        Debug.Log($"Target Volume: {musicVolume:F2}");
        Debug.Log($"Loop Coroutine Active: {loopCoroutine != null}");
        Debug.Log($"Fade Coroutine Active: {fadeCoroutine != null}");
        Debug.Log($"Current Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Main Menu Music: {(mainMenuMusic != null ? mainMenuMusic.name : "None")}");
        Debug.Log($"Gameplay Music: {(gameplayMusic != null ? gameplayMusic.name : "None")}");
        Debug.Log($"=====================================");
    }
    
    /// <summary>
    /// Get the master volume value from inspector (not runtime)
    /// </summary>
    public float GetInspectorMasterVolume()
    {
        return masterVolume;
    }
    
    /// <summary>
    /// Get the music volume value from inspector (not runtime)
    /// </summary>
    public float GetInspectorMusicVolume()
    {
        return musicVolume;
    }
    
    /// <summary>
    /// Get the SFX volume value from inspector (not runtime)
    /// </summary>
    public float GetInspectorSFXVolume()
    {
        return sfxVolume;
    }
}
