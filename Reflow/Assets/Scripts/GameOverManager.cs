using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Show "Game Over", display how many levels were completed, then return to home.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    [Tooltip("Name of the home/main menu scene.")]
    public string homeSceneName = "Home";

    [Tooltip("Delay in seconds before returning to the home screen.")]
    public float returnDelay = 2f;

    [Tooltip("UI Text element for displaying the levels completed.")]
    public Text levelsCompletedText;

    private void Start()
    {
        // Show how many levels the player completed
        if (levelsCompletedText != null && GameManager.Instance != null)
        {
            levelsCompletedText.text = 
                $"You completed {GameManager.Instance.levelsCompleted} level" +
                $"{(GameManager.Instance.levelsCompleted == 1 ? "" : "s")}.";
        }

        // After a delay, go back to home
        StartCoroutine(AutoReturnHome());
    }

    private IEnumerator AutoReturnHome()
    {
        yield return new WaitForSeconds(returnDelay);
        SceneManager.LoadScene(homeSceneName);
    }
}