using UnityEngine;

public static class WorldGeneration
{
    /// <summary>
    /// Creates a 2D int array of the specified dimensions, filled with the specified value.
    /// </summary>
    public static int[,] CreateAreaArray(int width, int height, int value)
    {
        int[,] array = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                array[x, y] = value;
            }
        }
        return array;
    }

    /// <summary>
    /// Creates a deep copy of the specified 2D int array.
    /// </summary>
    public static int[,] CopyAreaArray(int[,] array)
    {
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        int[,] copy = new int[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                copy[x, y] = array[x, y];
            }
        }
        return copy;
    }

    /// <summary>
    /// Creates a copy of the input array and sets a random number of values to 'value' based on perc (0-1).
    /// Uses UnityEngine.Random for randomness.
    /// If ignoreEdge is true, edges are excluded from randomization.
    /// </summary>
    public static int[,] RandomizeAreaArray(int[,] array, float perc, int value, bool ignoreEdge = false)
    {
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        int[,] randomized = CopyAreaArray(array);

        // Generate a list of all possible positions, optionally ignoring edges
        var positions = new System.Collections.Generic.List<(int x, int y)>();
        int minX = ignoreEdge ? 1 : 0;
        int maxX = ignoreEdge ? width - 2 : width - 1;
        int minY = ignoreEdge ? 1 : 0;
        int maxY = ignoreEdge ? height - 2 : height - 1;

        for (int x = minX; x <= maxX; x++)
            for (int y = minY; y <= maxY; y++)
                positions.Add((x, y));

        int totalCells = positions.Count;
        int cellsToSet = Mathf.RoundToInt(totalCells * Mathf.Clamp01(perc));

        // Shuffle and set the required number of cells using UnityEngine.Random
        for (int i = 0; i < cellsToSet && positions.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, positions.Count);
            var pos = positions[idx];
            randomized[pos.x, pos.y] = value;
            positions.RemoveAt(idx);
        }

        return randomized;
    }

    /// <summary>
    /// Applies cellular automata to the input array and returns a new modified array.
    /// </summary>
    /// <param name="array">The input 2D int array.</param>
    /// <param name="neighborThreshold">Number of neighbors required to set cell to 1 (default: 5).</param>
    /// <param name="loops">Number of iterations to run the automata (default: 4).</param>
    /// <returns>A new 2D int array after cellular automata processing.</returns>
    public static int[,] CellularAutomata(int[,] array, int neighborThreshold = 5, int loops = 4)
    {
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        int[,] result = CopyAreaArray(array);

        for (int l = 0; l < loops; l++)
        {
            int[,] temp = CopyAreaArray(result);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int neighbors = CountNeighbors(result, x, y);
                    if (neighbors >= neighborThreshold)
                        temp[x, y] = 1;
                    else
                        temp[x, y] = 0;
                }
            }
            result = temp;
        }
        return result;
    }

    /// <summary>
    /// Counts the number of neighboring cells with value 1 around (x, y).
    /// Uses 8-way connectivity.
    /// </summary>
    private static int CountNeighbors(int[,] array, int x, int y)
    {
        int count = 0;
        int width = array.GetLength(0);
        int height = array.GetLength(1);
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                int nx = x + dx;
                int ny = y + dy;
                if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                {
                    if (array[nx, ny] == 1)
                        count++;
                }
            }
        }
        return count;
    }
}


