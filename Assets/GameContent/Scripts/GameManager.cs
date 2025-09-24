using RX.AI.Orders;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    public MapManager MapManager;

    public GridObject testGridObjPrefab;
    public Vector2Int testGridPosition = new Vector2Int(0, 0);

    GridObject testGridObj;
    public OrderHandler Orders = new OrderHandler();

    private void Awake()
    {
        Singleton = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MapManager.worldGenerator.Generate();
        testGridObj = Instantiate(testGridObjPrefab);
        SetLocation(testGridObj, testGridPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if (Orders.NumOfOrders > 0)
        {
            Orders.UpdateOrders(this);
            return;
        }

        // Listen for left mouse button click using the old input system
        if (Input.GetMouseButtonDown(0))
        {
            TileCell cell = MapManager.OnMouseClick(Input.mousePosition);

            if (MapManager.IsClickedSelectedCell(cell))
            {
                MapManager.DeselectPath();
            }
            else
            {
                MapManager.SelectCell(cell);
                MapManager.DeselectPath();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            if (MapManager.SelectedCell == null ||
                Input.GetKey(KeyCode.LeftControl))
            {
                MapManager.DeselectAll();
                return;
            }

            TileCell cell = MapManager.OnMouseClick(Input.mousePosition);

            bool isMove = MapManager.IsClickedSelectedPath(cell, true) && MapManager.SelectedCell.Contents != null;

            TileCell src = MapManager.SelectedCell;
            TileCell dst = cell;
            List<TileCell> path = MapManager.GetShortestPath(src, dst);
            MapManager.SelectPath(path);

            if (isMove)
            {
                MoveObject(src, dst, path);
            }
        }
    }

    public void MoveObject(TileCell src, TileCell dst, List<TileCell> path)
    {
        Orders.Add(new GridOrderUtils.GridMoveOrder(src.Contents, path, 0.5f));
        //Debug.Log("Move");
    }

    public void SetLocation(GridObject gridObj, Vector2Int tilePos)
    {
        List<TileCell> testcells=MapManager.TileCells;
        TileCell cell = MapManager.GetCellAtPosition(tilePos);
        gridObj.SetLocation(cell);
    }
}
