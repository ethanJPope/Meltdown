using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single pipe piece in the grid.
/// You can configure which sides are open in the Inspector,
/// and it will correctly rotate those openings when the piece is rotated at runtime.
/// Also supports a “water” sprite that will be swapped in when water flows through.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Pipe : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Sprite to display when this pipe is filled with water.")]
    public Sprite waterSprite;

    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;

    [Header("Initial Openings")]
    [Tooltip("If true, this pipe is open on its Up side before any rotation.")]
    public bool openUp;
    [Tooltip("If true, this pipe is open on its Right side before any rotation.")]
    public bool openRight;
    [Tooltip("If true, this pipe is open on its Down side before any rotation.")]
    public bool openDown;
    [Tooltip("If true, this pipe is open on its Left side before any rotation.")]
    public bool openLeft;

    // Current set of open directions (after applying rotation)
    private HashSet<Direction> _connections = new HashSet<Direction>();

    // Water‐filled state
    private bool _hasWater = false;
    public bool HasWater => _hasWater;

    // Grid position for lookup by PuzzleManager, WinChecker, etc.
    public Vector2Int gridPosition { get; set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _originalSprite = _spriteRenderer.sprite;

        // 1) Initialize base connections from inspector flags
        _connections.Clear();
        if (openUp)    _connections.Add(Direction.Up);
        if (openRight) _connections.Add(Direction.Right);
        if (openDown)  _connections.Add(Direction.Down);
        if (openLeft)  _connections.Add(Direction.Left);

        // 2) Apply any initial rotation on the prefab so connections line up
        float zRot = transform.eulerAngles.z;
        int steps = Mathf.RoundToInt(zRot / 90f) % 4;
        for (int i = 0; i < steps; i++)
            RotateConnections();

        // 3) Ensure sprite matches original (no water at start)
        _spriteRenderer.sprite = _originalSprite;
    }

    /// <summary>
    /// Rotate this pipe clockwise by 90° and update the connection set.
    /// </summary>
    public void TryRotate()
    {
        transform.Rotate(0, 0, 90f);
        RotateConnections();
    }

    /// <summary>
    /// Internal helper to rotate all connection directions clockwise by 90°.
    /// </summary>
    private void RotateConnections()
    {
        var newSet = new HashSet<Direction>();
        foreach (var dir in _connections)
        {
            newSet.Add((Direction)(((int)dir + 3) % 4));
        }
        _connections = newSet;
    }

    /// <summary>
    /// Returns true if this pipe has an opening in the given direction.
    /// </summary>
    public bool HasConnection(Direction dir)
    {
        return _connections.Contains(dir);
    }

    /// <summary>
    /// Mark this pipe as filled (or unfilled) with water, swapping sprite.
    /// </summary>
    public void SetWater(bool water)
    {
        _hasWater = water;
        _spriteRenderer.sprite = water ? waterSprite : _originalSprite;
    }
}