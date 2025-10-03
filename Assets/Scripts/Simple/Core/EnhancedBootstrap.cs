using UnityEngine;

/// <summary>
/// Makes sure the GameManager exists before anything else tries to use it
/// Unity can be weird about load order, so this prevents crashes
/// </summary>
public class EnhancedBootstrap : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void EnsureGameManager()
    {
        // Don't create a duplicate if one already exists
        if (EnhancedGameManager.Instance != null) return;

        // Create the GameManager GameObject
        var go = new GameObject("EnhancedGameManager");
        go.AddComponent<EnhancedGameManager>();
    }
}
