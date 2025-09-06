using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this to a cube (or any box‐shaped object).
/// It will grow its Y‐scale from its starting height up to maxHeight,
/// move its position upward so the bottom stays in place,
/// and when it reaches maxHeight it will load the Game Over screen.
/// </summary>
[RequireComponent(typeof(Transform))]
public class BoxGrower : MonoBehaviour
{
    [Header("Growth Settings")]
    [Tooltip("The maximum Y‐scale (height) the box will grow to.")]
    public float maxHeight = 5f;

    [Tooltip("Units per second to increase the box height.")]
    public float growSpeed = 1f;

    [Tooltip("Name of the GameOver scene to load when growth completes.")]
    public string gameOverSceneName = "GameOver";

    // Current height (Y‐scale)
    private float currentHeight;

    // Original X and Z scales (we only modify Y)
    private float originalX;
    private float originalZ;

    // Cached transform
    private Transform _t;

    private void Awake()
    {
        _t = transform;
        originalX = _t.localScale.x;
        originalZ = _t.localScale.z;
        currentHeight = _t.localScale.y;
    }

    private void Update()
    {
        if (currentHeight < maxHeight)
        {
            // Compute new height this frame
            float newHeight = currentHeight + growSpeed * Time.deltaTime;
            if (newHeight >= maxHeight)
            {
                newHeight = maxHeight;
            }

            // Calculate how much taller we've become
            float deltaHeight = newHeight - currentHeight;

            // Apply scale change (only Y axis)
            _t.localScale = new Vector3(originalX, newHeight, originalZ);

            // Move the object up by half the delta so the bottom stays fixed
            _t.position += Vector3.up * (deltaHeight * 0.5f);

            currentHeight = newHeight;

            // If we've hit max height, load Game Over
            if (Mathf.Approximately(currentHeight, maxHeight))
            {
                SceneManager.LoadScene(gameOverSceneName);
            }
        }
    }
}