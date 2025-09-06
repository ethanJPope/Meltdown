using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to your Game Over UI Canvas (or an empty GameObject in that scene).
/// Wire up the Restart and Quit buttons to these methods.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Tooltip("Name of the main game scene to restart")]
    public string mainSceneName = "Main";

    /// <summary>
    /// Call from your UI button to restart the game.
    /// </summary>
    public void RestartGame()
    {
        SceneManager.LoadScene(mainSceneName);
    }

    /// <summary>
    /// Call from your UI button to quit the application.
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}