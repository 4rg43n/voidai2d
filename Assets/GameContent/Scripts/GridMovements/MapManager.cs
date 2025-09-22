using System.Collections.Generic;
using UnityEngine;

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

    TileCell selected;
    SpriteRenderer selMark;

    private void Start()
    {
        gridLineOverlay.enabled = false;
    }

    public TileCell GetCellAtPosition(Vector2Int pos)
    {
        if (!IsPositionInMap(pos))
            return null;

        int index = pos.x + pos.y * Width;
        return TileCells[index];
    }

    public bool IsPositionInMap(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= Width ||
            pos.y < 0 || pos.x >= Height)
            return false;

        return true;
    }

    public TileCell OnMouseClick(Vector3 mousePosition)
    {
        Vector3 localPos = GetLocalMousePosition(mousePosition);
        Vector2Int tilePos = new Vector2Int(Mathf.FloorToInt(localPos.x), Mathf.FloorToInt(localPos.y));

        TileCell cell = GetCellAtPosition(tilePos);
        Select(cell);

        // You can use localPos as needed, e.g., log or pass to another method
        Debug.Log($"Local Position: {tilePos} => {cell.areaValue}");


        return Selected;
    }

    public void Deselect()
    {
        if (selected == null)
            return;

        if (selMark != null)
            Destroy(selMark.gameObject);
        selected.OnDeselect();
        selected = null;

        gridLineOverlay.enabled = false;
    }

    public void Select(TileCell cell)
    {
        if (selected == cell)
            return;

        Deselect();

        if (cell == null)
            return;

        gridLineOverlay.enabled = true;

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
