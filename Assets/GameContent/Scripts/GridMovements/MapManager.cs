using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using VoidAI.Pathfinding;

public class MapManager : MonoBehaviour
{
    public WorldGenerator worldGenerator;
    public SpriteRenderer selPrefab;
    public GridLineOverlay gridLineOverlay;

    public Vector3 cellDims = new Vector3(1, 1, 0);

    public int Width { get { return worldGenerator.width; } }
    public int Height { get { return worldGenerator.height; } }
    public List<TileCell> TileCells { get { return worldGenerator.TileCells; } }
    public TileCell Selected { get { return selected; } }
    public List<TileCell> SelectedPath { get { return selectedPath; } }
    public WorldGenerator WorldGenerator { get { return worldGenerator; } }

    TileCell selected;
    List<TileCell> selectedPath = new();
    SpriteRenderer selMark;

    private void Start()
    {
        gridLineOverlay.drawGrid = gridLineOverlay.drawSquares = false;
    }

    public List<TileCell> GetCellsInRange(TileCell src, int[,] pattern)
    {
        List<IAStarNode> pathNodes = VoidAI.Pathfinding.AStarSearch.FindPattern(src, pattern, true);
        List<TileCell> path = new List<TileCell>();

        foreach (var n in pathNodes)
        {
            path.Add(GetCellAtPosition(new Vector2Int(n.X, n.Y)));
        }

        return path;
    }

    public List<TileCell> GetCellsInRange(TileCell src, int range)
    {
        List<IAStarNode> pathNodes = VoidAI.Pathfinding.AStarSearch.FindRange(src, range, true);
        List<TileCell> path = new List<TileCell>();

        foreach (var n in pathNodes)
        {
            path.Add(GetCellAtPosition(new Vector2Int(n.X, n.Y)));
        }

        return path;
    }

    public List<TileCell> GetShortestPath(TileCell src, TileCell dst)
    {
        List<IAStarNode> pathNodes = VoidAI.Pathfinding.AStarSearch.FindShortestPath(src, dst, true);
        List<TileCell> path = new List<TileCell>();

        foreach (var n in pathNodes)
        {
            path.Add(GetCellAtPosition(new Vector2Int(n.X, n.Y)));
        }

        return path;
    }

    public TileCell GetCellAtPosition(Vector2Int pos)
    {
        return worldGenerator.GetCellAtPosition(pos);
    }

    public TileCell IsPositionInMap(Vector2Int pos)
    {
        return worldGenerator.IsPositionInMap(pos) ? worldGenerator.GetCellAtPosition(pos) : null;
    }

    public TileCell OnMouseClick(Vector3 mousePosition, bool selectTile=true)
    {
        Vector3 localPos = GetLocalMousePosition(mousePosition);
        Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(localPos.x), Mathf.FloorToInt(localPos.y));

        TileCell cell = worldGenerator.GetCellAtPosition(tilePos);

        if (selectTile)
            Select(cell);

        // You can use localPos as needed, e.g., log or pass to another method
        Debug.Log($"Local Position: {tilePos} => {cell.areaValue}");


        return cell;
    }

    public void DeselectPath()
    {
        if (selectedPath.Count==0)
        {
            return;
        }

        foreach (TileCell cell in selectedPath)
            cell.OnDeselectPath();

        selectedPath.Clear();
        gridLineOverlay.drawSquares = false;
        gridLineOverlay.pathTiles.Clear();
    }

    public void SelectPath(List<TileCell> paths)
    {
        if (paths == null || paths.Count == 0)
            return;

        DeselectPath();

        selectedPath.AddRange(paths);
        gridLineOverlay.drawSquares = true;
        gridLineOverlay.pathTiles.AddRange(paths);
    }

    public void Deselect()
    {
        if (selected == null)
            return;

        if (selMark != null)
            Destroy(selMark.gameObject);
        selected.OnDeselect();
        selected = null;

        gridLineOverlay.drawGrid = false;
    }

    public void Select(TileCell cell)
    {
        if (selected == cell)
            return;

        Deselect();

        if (cell == null)
            return;

        gridLineOverlay.drawGrid = true;

        selMark = Instantiate(selPrefab);
        selMark.transform.parent = transform;
        selMark.transform.localPosition = new Vector3(cell.Position.x * cellDims.x, cell.Position.y * cellDims.y, 0) + (cellDims * 0.5f);

        Color c = Color.yellow;
        float a = selMark.color.a;

        selMark.color = new Color(c.r, c.g, c.b, a);

        selected = cell;
        selected.OnSelect();
    }

    // Returns the mouse position relative to this object's local space
    private Vector3 GetLocalMousePosition(Vector3 screenMousePos)
    {
        // Convert to world position
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(screenMousePos);
        // Convert to local position relative to this transform
        Vector3 localPos = transform.InverseTransformPoint(worldMousePos);
        // Z is not relevant for 2D, so set to 0
        localPos.z = 0;
        return localPos;
    }
}
