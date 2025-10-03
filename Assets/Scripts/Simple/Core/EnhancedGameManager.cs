using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Main game manager - handles death, restart, and game state
/// This is basically the brain of the whole game, keeps track of whether you're dead or not
/// </summary>
public class EnhancedGameManager : MonoBehaviour
{
    [Header("Game State")]
    public static EnhancedGameManager Instance;
    
    [SerializeField, Tooltip("Current game state")]
    private bool isDead = false;
    
    
    // Events
    public static event Action OnPlayerDeath;
    public static event Action OnGameRestart;
    public static event Action OnGameStart;
    
    public bool IsDead => isDead;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnGameStart?.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // Reset everything when the game scene loads
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // I just handle the core game state here
        // The death UI stuff is handled by EnhancedScore
    }
    
    void OnDestroy()
    {
        // Clean up when this gets destroyed
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When the game scene loads, reset the death state
        if (scene.name == "Game" || scene.name.ToLower().Contains("game"))
        {
            isDead = false;
        }
    }
    
    public void Die()
    {
        if (isDead) return;
        
        Debug.Log("[EnhancedGameManager] Player died");
        isDead = true;
        OnPlayerDeath?.Invoke();
        
        
        // The death UI will handle showing the restart options
    }
    
    public void Restart()
    {
        Debug.Log("[EnhancedGameManager] Game restarting");
        isDead = false;
        OnGameRestart?.Invoke();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }
}
