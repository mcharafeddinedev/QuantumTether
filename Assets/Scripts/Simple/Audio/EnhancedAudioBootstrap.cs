using UnityEngine;

/// <summary>
/// Ensures the music manager is created and initialized
/// This is the improved version that actually works well
/// </summary>
public class EnhancedAudioBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Ensure music manager exists
        if (EnhancedMusicManager.Instance == null)
        {
            GameObject musicManager = new GameObject("EnhancedMusicManager");
            musicManager.AddComponent<EnhancedMusicManager>();
        }
    }
}
