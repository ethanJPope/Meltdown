using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

public static class ListExtensions
{
    private static System.Random rng = new System.Random();
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            int k = rng.Next(n--);
            T tmp = list[k];
            list[k] = list[n];
            list[n] = tmp;
        }
    }
}

[RequireComponent(typeof(Transform))]
public class LevelGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 6;
    public int height = 6;
    public int unusedSpaces = 5;
    public float cellSize = 1.5f;

    [Header("Pipe Prefabs")]
    public GameObject startPipePrefab;
    public GameObject endPipePrefab;
    public GameObject straightPipePrefab;
    public GameObject elbowPipePrefab;

    [Header("Hierarchy")]
    public Transform gridParent;

    [Header("Debug / Test")]
    public bool loadOnStart = true;
    public float checkInterval = 0.5f;

    // Exposed for external checkers
    public Pipe[,] GridPipes => gridPipes;
    public Vector2Int StartCell => new Vector2Int(width - 1, 0);
    public Vector2Int EndCell   => new Vector2Int(0, height - 1);
    public int Width  => width;
    public int Height => height;

    private Pipe[,] gridPipes;
    private bool hasLoggedSolved = false;
    private float checkTimer = 0f;

    private void Awake()
    {
        gridPipes = new Pipe[width, height];
    }

    private void Start()
    {
        if (loadOnStart)
            GenerateLevel();
    }

    private void Update()
    {
        if (hasLoggedSolved)
            return;

        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;

        // recalculate which pipes should have water after any rotations
        FlowWater();

        if (CheckWin())
        {
            hasLoggedSolved = true;
            Debug.Log("SOLVED!!");
            StartCoroutine(HandleWin());
        }
    }

    [ContextMenu("Generate Level")]
    public void GenerateLevel()
    {
        hasLoggedSolved = false;
        checkTimer = 0f;

        if (unusedSpaces < 0 || unusedSpaces >= width * height - 2)
        {
            Debug.LogError("unusedSpaces out of range.");
            return;
        }

        // Clear old pipes
        foreach (Transform c in gridParent)
            Destroy(c.gameObject);

        gridPipes = new Pipe[width, height];

        // 1) Build main path
        var path = BuildRandomPath(StartCell, EndCell);
        if (path == null || path.Count == 0)
        {
            Debug.LogError("Failed to generate path.");
            return;
        }

        // 2) Determine empty cells
        var all = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                all.Add(new Vector2Int(x, y));

        var nonPath = new List<Vector2Int>(all);
        nonPath.RemoveAll(c => path.Contains(c));
        nonPath.Shuffle();

        var emptySet = new HashSet<Vector2Int>();
        for (int i = 0; i < unusedSpaces && i < nonPath.Count; i++)
            emptySet.Add(nonPath[i]);

        // 3) Place path pipes
        foreach (var idx in path.Select((cell, i) => new { cell, i }))
        {
            var cell = idx.cell;
            var i = idx.i;
            var worldPos = CellToWorld(cell);

            Direction inDir = i == 0
                ? Direction.Right
                : GetDirection(path[i - 1], cell);
            Direction outDir = i == path.Count - 1
                ? Direction.Left
                : GetDirection(cell, path[i + 1]);

            GameObject prefab;
            float angle = 0f;
            if (inDir == outDir || Opposite(inDir) == outDir)
            {
                prefab = straightPipePrefab;
                bool horizontal = (inDir == Direction.Left || inDir == Direction.Right);
                angle = horizontal ? 90f : 0f;
            }
            else
            {
                prefab = elbowPipePrefab;
                int di = (int)inDir, do_ = (int)outDir;
                int baseIn = (int)Direction.Up, baseOut = (int)Direction.Right;
                int steps = (di - baseIn + 4) % 4;
                angle = steps * 90f;
                if ((baseOut + steps) % 4 != do_)
                    angle += 180f;
            }

            var go = Instantiate(prefab, worldPos, Quaternion.Euler(0, 0, angle), gridParent);
            var pipe = go.GetComponent<Pipe>();
            pipe.gridPosition = cell;
            gridPipes[cell.x, cell.y] = pipe;
        }

        // 4) Decorate start/end
        Instantiate(startPipePrefab,
                    CellToWorld(StartCell) + Vector3.right * cellSize,
                    Quaternion.identity, gridParent);
        Instantiate(endPipePrefab,
                    CellToWorld(EndCell) + Vector3.left * cellSize,
                    Quaternion.identity, gridParent);

        // 5) Fill other cells (only straight and elbow now)
        var fillers = new GameObject[] { straightPipePrefab, elbowPipePrefab };
        foreach (var cell in nonPath)
        {
            if (emptySet.Contains(cell)) continue;

            var pos = CellToWorld(cell);
            var pf = fillers[UnityEngine.Random.Range(0, fillers.Length)];
            float r = UnityEngine.Random.Range(0, 4) * 90f;
            var go = Instantiate(pf, pos, Quaternion.Euler(0, 0, r), gridParent);
            var pipe = go.GetComponent<Pipe>();
            pipe.gridPosition = cell;
            gridPipes[cell.x, cell.y] = pipe;
        }

        // 6) immediately flow water for the new level
        FlowWater();
    }

    private IEnumerator HandleWin()
    {
        yield return new WaitForSeconds(0.5f);
        GenerateLevel();
    }

    private void FlowWater()
    {
        // Clear any old water on all pipes
        foreach (var pipe in gridParent.GetComponentsInChildren<Pipe>())
            pipe.SetWater(false);

        // Flood‐fill from the start cell
        var visited = new bool[width, height];
        FloodFill(StartCell, Direction.Right, visited);
    }

    private void FloodFill(Vector2Int cell, Direction entryDir, bool[,] visited)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return;
        if (visited[cell.x, cell.y])
            return;

        var pipe = gridPipes[cell.x, cell.y];
        if (pipe == null || !pipe.HasConnection(entryDir))
            return;

        visited[cell.x, cell.y] = true;
        pipe.SetWater(true);

        if (cell == EndCell)
            return;

        foreach (Direction exit in Enum.GetValues(typeof(Direction)))
        {
            if (exit == entryDir) continue;
            if (!pipe.HasConnection(exit)) continue;

            var next = cell + DirToVec(exit);
            FloodFill(next, Opposite(exit), visited);
        }
    }

    private Vector3 CellToWorld(Vector2Int cell)
    {
        float offsetX = (width - 1) * cellSize * 0.5f;
        float offsetY = (height - 1) * cellSize * 0.5f;
        float x = cell.x * cellSize - offsetX;
        float y = cell.y * cellSize - offsetY;
        return new Vector3(x, y, 0f) + gridParent.position;
    }

    private bool CheckWin()
    {
        var visited = new bool[width, height];
        return DFSReach(StartCell, Direction.Right, visited);
    }

    private bool DFSReach(Vector2Int cell, Direction entry, bool[,] visited)
    {
        if (cell.x < 0 || cell.x >= width || cell.y < 0 || cell.y >= height)
            return false;
        if (visited[cell.x, cell.y])
            return false;

        var pipe = gridPipes[cell.x, cell.y];
        if (pipe == null || !pipe.HasConnection(entry))
            return false;

        visited[cell.x, cell.y] = true;
        if (cell == EndCell)
            return pipe.HasConnection(Direction.Left);

        foreach (Direction exit in Enum.GetValues(typeof(Direction)))
        {
            if (exit == entry) continue;
            if (!pipe.HasConnection(exit)) continue;

            var next = cell + DirToVec(exit);
            if (DFSReach(next, Opposite(exit), visited))
                return true;
        }
        return false;
    }

    private Direction GetDirection(Vector2Int from, Vector2Int to)
    {
        var d = to - from;
        if (d == Vector2Int.up)    return Direction.Up;
        if (d == Vector2Int.right) return Direction.Right;
        if (d == Vector2Int.down)  return Direction.Down;
        return Direction.Left;
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

    private List<Vector2Int> BuildRandomPath(Vector2Int start, Vector2Int end)
    {
        var used = new bool[width, height];
        var path = new List<Vector2Int> { start };
        used[start.x, start.y] = true;
        return DFSPath(start, end, used, path) ? path : null;
    }

    private bool DFSPath(Vector2Int cur, Vector2Int tgt, bool[,] used, List<Vector2Int> path)
    {
        if (cur == tgt)
            return true;

        var dirs = new List<Direction> { Direction.Up, Direction.Right, Direction.Down, Direction.Left };
        dirs.Shuffle();
        foreach (var d in dirs)
        {
            var nxt = cur + DirToVec(d);
            if (nxt.x < 0 || nxt.x >= width || nxt.y < 0 || nxt.y >= height)
                continue;
            if (used[nxt.x, nxt.y])
                continue;

            used[nxt.x, nxt.y] = true;
            path.Add(nxt);
            if (DFSPath(nxt, tgt, used, path))
                return true;

            path.RemoveAt(path.Count - 1);
            used[nxt.x, nxt.y] = false;
        }
        return false;
    }
}