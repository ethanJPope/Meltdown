using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LevelGenerator))]
public class WinChecker : MonoBehaviour
{
    [Tooltip("How often (in seconds) to re-check the puzzle state.")]
    public float checkInterval = 0.5f;

    private LevelGenerator _generator;
    private float _timer = 0f;
    private bool _hasLoggedSolved = false;

    private void Awake()
    {
        _generator = GetComponent<LevelGenerator>();
        if (_generator == null)
            Debug.LogError("WinChecker requires a LevelGenerator on the same GameObject.");
    }

    private void Update()
    {
        if (_hasLoggedSolved)
            return;

        _timer += Time.deltaTime;
        if (_timer < checkInterval)
            return;

        _timer = 0f;
        if (PropagateWaterAndCheckEnd())
        {
            _hasLoggedSolved = true;
            Debug.Log("SOLVED!!");
        }
    }

    private bool PropagateWaterAndCheckEnd()
    {
        var grid = _generator.GridPipes;
        int w = _generator.Width;
        int h = _generator.Height;
        Vector2Int start = _generator.StartCell;
        Vector2Int end   = _generator.EndCell;

        // Clear previous water state on all pipes
        for (int x = 0; x < w; x++)
        for (int y = 0; y < h; y++)
        {
            var p = grid[x, y];
            if (p != null)
                p.SetWater(false);
        }

        // BFS queue of (cell, entryDirection)
        var queue = new Queue<(Vector2Int cell, Direction entry)>();
        var visited = new bool[w, h];

        // Start at the start cell, entering from Right
        queue.Enqueue((start, Direction.Right));

        while (queue.Count > 0)
        {
            var (cell, entry) = queue.Dequeue();
            if (cell.x < 0 || cell.x >= w || cell.y < 0 || cell.y >= h)
                continue;
            if (visited[cell.x, cell.y])
                continue;

            var pipe = grid[cell.x, cell.y];
            if (pipe == null || !pipe.HasConnection(entry))
                continue;

            // mark visited and fill with water
            visited[cell.x, cell.y] = true;
            pipe.SetWater(true);

            // enqueue all other exits
            foreach (Direction exit in Enum.GetValues(typeof(Direction)))
            {
                if (exit == entry) 
                    continue;
                if (!pipe.HasConnection(exit)) 
                    continue;

                var nextCell = cell + DirToVec(exit);
                var nextEntry = Opposite(exit);
                // neighbor must exist and have matching connection
                if (nextCell.x < 0 || nextCell.x >= w || nextCell.y < 0 || nextCell.y >= h)
                    continue;
                var neighbor = grid[nextCell.x, nextCell.y];
                if (neighbor == null || !neighbor.HasConnection(nextEntry))
                    continue;

                queue.Enqueue((nextCell, nextEntry));
            }
        }

        // final win: check that the pipe at the end cell has water and connects Left into the end pipe
        var endPipe = grid[end.x, end.y];
        return endPipe != null && endPipe.HasWater && endPipe.HasConnection(Direction.Left);
    }

    private Direction Opposite(Direction d) => (Direction)(((int)d + 2) % 4);

    private Vector2Int DirToVec(Direction d)
    {
        switch (d)
        {
            case Direction.Up:    return Vector2Int.up;
            case Direction.Right: return Vector2Int.right;
            case Direction.Down:  return Vector2Int.down;
            case Direction.Left:  return Vector2Int.left;
        }
        return Vector2Int.zero;
    }
}