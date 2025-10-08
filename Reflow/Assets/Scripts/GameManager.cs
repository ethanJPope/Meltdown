using UnityEngine;

/// <summary>
/// Singleton that tracks the number of levels the player has completed.
/// Persists across scene loads.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Number of levels the player has completed so far.
    /// </summary>
    public int levelsCompleted { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Reset count at the start of a new play session
        levelsCompleted = 0;
    }

    /// <summary>
    /// Call this when the player finishes a level.
    /// </summary>
    public void LevelCompleted()
    {
        levelsCompleted++;
    }
}