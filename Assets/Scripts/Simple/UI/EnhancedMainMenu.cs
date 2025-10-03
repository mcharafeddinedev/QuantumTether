using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Enhanced main menu controller
/// This is the improved version that actually works well
/// </summary>
public class EnhancedMainMenu : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject tutorialPanel;
    
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button tutorialButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitButton;
    
    
    [SerializeField] private Button creditsBackButton;
    [SerializeField] private Button tutorialBackButton;
    
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "Game";
    
    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float buttonVolume = 1f;
    
    void Start()
    {
        EnsureAudioSource();
        InitializeMainMenu();
        SetupButtonListeners();
    }
    
    void SetupButtonListeners()
    {
        if (startButton != null) startButton.onClick.AddListener(StartGame);
        if (tutorialButton != null) tutorialButton.onClick.AddListener(ShowTutorial);
        if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptions);
        if (creditsButton != null) creditsButton.onClick.AddListener(ShowCredits);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        
        if (creditsBackButton != null) creditsBackButton.onClick.AddListener(ShowMainMenu);
        if (tutorialBackButton != null) tutorialBackButton.onClick.AddListener(ShowMainMenu);
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
            float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            audioSource.PlayOneShot(buttonClickSound, sfxVolume * buttonVolume);
        }
    }
    
    public void StartGame()
    {
        PlayButtonSound();
        SceneManager.LoadScene(gameSceneName);
    }
    
    public void ShowOptions()
    {
        PlayButtonSound();
        HideAllPanels();
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }
    
    public void ShowCredits()
    {
        PlayButtonSound();
        HideAllPanels();
        if (creditsPanel != null) creditsPanel.SetActive(true);
    }
    
    public void ShowTutorial()
    {
        PlayButtonSound();
        HideAllPanels();
        if (tutorialPanel != null) tutorialPanel.SetActive(true);
    }
    
    void InitializeMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
    
    public void ShowMainMenu()
    {
        PlayButtonSound();
        HideAllPanels();
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
    
    public void QuitGame()
    {
        PlayButtonSound();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    
    void HideAllPanels()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (creditsPanel != null) creditsPanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
    }
    
    void OnDestroy()
    {
        if (startButton != null) startButton.onClick.RemoveListener(StartGame);
        if (optionsButton != null) optionsButton.onClick.RemoveListener(ShowOptions);
        if (creditsButton != null) creditsButton.onClick.RemoveListener(ShowCredits);
        if (quitButton != null) quitButton.onClick.RemoveListener(QuitGame);
        if (creditsBackButton != null) creditsBackButton.onClick.RemoveListener(ShowMainMenu);
    }
}
