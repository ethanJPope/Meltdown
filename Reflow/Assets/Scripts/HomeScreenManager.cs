using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to your Home (main menu) Canvas or an empty GameObject in the Home scene.
/// Provides a method to start the game by loading the specified game scene.
/// </summary>
public class HomeScreenManager : MonoBehaviour
{
    [Tooltip("Name of the scene that starts the game.")]
    public string gameSceneName = "Game";

    /// <summary>
    /// Call this from your Play button's OnClick() event to load the game scene.
    /// </summary>
    public void StartGame()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("Game scene name is not set on HomeScreenManager.");
            return;
        }
        SceneManager.LoadScene(gameSceneName);
    }

    /// <summary>
    /// Optional helper: call this from a Quit button to exit the application.
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