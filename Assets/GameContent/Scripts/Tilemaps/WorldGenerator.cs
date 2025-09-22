using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    [Header("World Settings")]
    [Tooltip("Settings for the world map.")]
    public int width = 10;
    public int height = 10;
    public bool ignoreEdge = false;
    public Vector2 cellDims = new Vector2(1, 1);

    [Header("Generation settings")]
    [Tooltip("Settings for the world generation algorhythms.")]
    public float fillPercent = 0.4f;
    public int neighborThreshold = 5;
    public int iterations = 4;

    [Header("Tilemap settings")]
    [Tooltip("Tilemaps and tiles for drawing the world.")]
    public Tilemap tilemap;
    public TileBase[] tiles;

    [Header("Random settings")]
    [Tooltip("Random seeds.")]
    public bool useRandomSeed = true;
    public int seed = 0;


    public List<TileCell> TileCells = new();
    int[,] worldArea = null;

    public void Generate()
    {
        Clear();

        int[,] area = worldArea = WorldGeneration.CreateAreaArray(width, height, 0);
        area = WorldGeneration.RandomizeAreaArray(area, fillPercent, 1, true);
        area = WorldGeneration.CellularAutomata(area, neighborThreshold, iterations);

        if (useRandomSeed)
        {
            seed = Random.Range(int.MinValue, int.MaxValue);
        }
        Random.InitState(seed);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int tileIndex = area[x, y];
                if (tileIndex >= 0 && tileIndex < tiles.Length)
                {
                    tilemap.SetTile(new Vector3Int(x, y, 0), tiles[tileIndex]);
                    TileCell cell = new TileCell
                    {
                        Position = new Vector2Int(x, y),
                        WorldPosition = new Vector3(x * cellDims.x + transform.position.x, y * cellDims.y + transform.position.y),
                        areaValue = tileIndex
                    };

                    TileCells.Add(cell);
                }
                else
                {
                    Debug.LogWarning($"Tile index {tileIndex} out of bounds for tiles array.");
                }
            }
        }
    }

    public void Clear()
    {
        tilemap.ClearAllTiles();
        TileCells.Clear();
    }
}


