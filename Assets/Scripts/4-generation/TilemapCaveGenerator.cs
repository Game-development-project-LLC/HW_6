using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Demonstrates the CaveGenerator on a Tilemap and
/// chooses a starting position for the player that is not stuck.
/// Requirement 2.b from the homework:
/// pick a random start cell from which at least N tiles are reachable.
/// </summary>
public class TilemapCaveGenerator : MonoBehaviour
{
    [Header("Tilemap")]
    [SerializeField] private Tilemap tilemap = null;

    [Tooltip("The tile that represents a wall (an impassable block).")]
    [SerializeField] private TileBase wallTile = null;

    [Tooltip("The tile that represents the floor (a walkable tile).")]
    [SerializeField] private TileBase floorTile = null;

    [Header("Cave generation")]
    [Range(0f, 1f)]
    [SerializeField] private float randomFillPercent = 0.5f;

    [SerializeField] private int gridSize = 100;

    [Tooltip("How many smoothing steps to run.")]
    [SerializeField] private int simulationSteps = 20;

    [Tooltip("Pause between smoothing steps (seconds).")]
    [SerializeField] private float pauseTime = 0.1f;

    [Tooltip("Random seed so that the cave is reproducible.")]
    [SerializeField] private int randomSeed = 100;

    [Header("Player spawn (requirement 2.b)")]
    [Tooltip("Transform of the player that will be placed in the cave.")]
    [SerializeField] private Transform playerTransform = null;

    [Tooltip("Minimal number of different floor tiles that must be reachable from the start.")]
    [SerializeField] private int minReachableTiles = 100;

    [Tooltip("Max number of attempts to find a good start position.")]
    [SerializeField] private int maxSpawnAttempts = 1000;

    private CaveGenerator caveGenerator;

    private static readonly Vector2Int[] Directions =
    {
        new Vector2Int(1, 0),
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
    };

    private void Start()
    {
        // Same seed -> same cave every run (unless you change the seed).
        UnityEngine.Random.InitState(randomSeed);

        caveGenerator = new CaveGenerator(randomFillPercent, gridSize);
        caveGenerator.RandomizeMap();

        // Initial visualization before smoothing
        ShowPatternOnTileMap(caveGenerator.GetMap());

        // Run smoothing steps + finally place the player
        StartCoroutine(SimulateCavePattern());
    }

    private IEnumerator SimulateCavePattern()
    {
        for (int i = 0; i < simulationSteps; i++)
        {
            yield return new WaitForSeconds(pauseTime);

            caveGenerator.SmoothMap();
            ShowPatternOnTileMap(caveGenerator.GetMap());
        }

        TryPlacePlayerInLargeCaveRoom();
    }

    /// <summary>
    /// Requirement 2.b:
    /// Pick a random floor tile from which we can reach at least minReachableTiles
    /// different floor tiles using BFS. Try up to maxSpawnAttempts times.
    /// </summary>
    private void TryPlacePlayerInLargeCaveRoom()
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("TilemapCaveGenerator: playerTransform is not assigned.");
            return;
        }

        int[,] map = caveGenerator.GetMap();
        int size = gridSize;

        System.Random random = new System.Random();

        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            int x = random.Next(1, size - 1);
            int y = random.Next(1, size - 1);

            // We only start on floor tiles
            if (map[x, y] == 1) // 1 = wall
                continue;

            int reachable = CountReachableFloorTiles(map, size, x, y);

            if (reachable >= minReachableTiles)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                Vector3 world = tilemap.CellToWorld(cell) + tilemap.cellSize / 2f;

                playerTransform.position = world;
                Debug.Log($"Spawned player at ({x},{y}), reachable tiles = {reachable}");
                return;
            }
        }

        Debug.LogWarning("TilemapCaveGenerator: could not find a good start position.");
    }

    /// <summary>
    /// BFS on the cave map: returns how many floor tiles are reachable
    /// from a given starting cell.
    /// </summary>
    private int CountReachableFloorTiles(int[,] map, int size, int startX, int startY)
    {
        if (map[startX, startY] == 1) // wall
            return 0;

        bool[,] visited = new bool[size, size];
        Queue<Vector2Int> queue = new Queue<Vector2Int>();

        visited[startX, startY] = true;
        queue.Enqueue(new Vector2Int(startX, startY));

        int count = 0;

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            count++;

            for (int i = 0; i < Directions.Length; i++)
            {
                int nx = current.x + Directions[i].x;
                int ny = current.y + Directions[i].y;

                if (nx < 0 || nx >= size || ny < 0 || ny >= size)
                    continue;

                if (visited[nx, ny])
                    continue;

                if (map[nx, ny] == 1) // wall
                    continue;

                visited[nx, ny] = true;
                queue.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return count;
    }

    /// <summary>
    /// Draw the cave map on the tilemap: 1 = wall, 0 = floor.
    /// </summary>
    private void ShowPatternOnTileMap(int[,] data)
    {
        for (int y = 0; y < gridSize; y++)
        {
            for (int x = 0; x < gridSize; x++)
            {
                var position = new Vector3Int(x, y, 0);
                var tile = data[x, y] == 1 ? wallTile : floorTile;
                tilemap.SetTile(position, tile);
            }
        }
    }
}
