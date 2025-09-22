using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public bool ignoreEdge = false;

    public int neighborThreshold = 5;
    public int iterations = 4;

    public Tilemap tilemap;
    public TileBase[] tiles;

    public bool useRandomSeed = true;
    public int seed = 0;


    int[,] worldArea = null;

    public void Generate()
    {
        int[,] area = worldArea = WorldGeneration.CreateAreaArray(width, height, 0);
        area = WorldGeneration.RandomizeAreaArray(area, 0.4f, 1, true);
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
    }
}


