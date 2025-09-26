using RX.AI.Orders;
using RX.Utils;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    public MapManager MapManager;

    public GridObject testGridObjPrefab;
    public Vector2Int testGridPosition = new Vector2Int(0, 0);

    public Color pathColor = new Color(1f, 0f, 0f, 0.35f);
    public Color pathColorAttack = new Color(1f, 0f, 0f, 0.35f);
    public Color pathColorMagic = new Color(0f, 0f, 1f, 0.35f);
    public Color pathColorSkill = new Color(0f, 1f, 0f, 0.35f);
    public Color pathColorItem = new Color(1f, 1f, 0f, 0.35f);

    public GameState GameState { get; set; } = GameState.INITIALIZE;
    public GameSubState GameSubState { get; set; } = GameSubState.MOVE;

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
        bool isOverUI = GameUtils.IsOverUI();

        if (Orders.NumOfOrders > 0)
        {
            Orders.UpdateOrders(this);
            return;
        }

        // Listen for left mouse button click using the old input system
        if (!isOverUI&&Input.GetMouseButtonDown(0))
        {
            TileCell cell = MapManager.OnMouseClick(Input.mousePosition);

            if (MapManager.IsClickedSelectedCell(cell))
            {
                MapManager.DeselectPath();
                SelectArea();
            }
            else
            {
                MapManager.SelectCell(cell);
                MapManager.DeselectPath();
                MapManager.DeselectArea();
            }
        }

        if (!isOverUI && Input.GetMouseButtonDown(1))
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

    void SelectArea()
    {
        if (GameSubState != GameSubState.MOVE && MapManager.SelectedCell != null)
        {
            List<TileCell> area = MapManager.GetRange(MapManager.SelectedCell, 3);
            MapManager.SelectArea(area, GetAreaColor());
        }
        else
        {
            MapManager.DeselectArea();
        }
    }

    Color GetAreaColor()
    {
        return GameSubState switch
        {
            GameSubState.MOVE => pathColor,
            GameSubState.ATTACK => pathColorAttack,
            GameSubState.MAGIC => pathColorMagic,
            GameSubState.SKILL => pathColorSkill,
            GameSubState.ITEM => pathColorItem,
            _ => pathColor,
        };
    }

    public GameSubState SetGameSubState(GameSubState subState)
    {
        GameSubState = subState;

        MapManager.DeselectAll();

        return GameSubState;
    }

    public void MoveObject(TileCell src, TileCell dst, List<TileCell> path)
    {
        Orders.Add(new GridOrderUtils.GridMoveOrder(src.Contents, path, 0.25f));
        //Debug.Log("Move");
    }

    public void SetLocation(GridObject gridObj, Vector2Int tilePos)
    {
        List<TileCell> testcells=MapManager.TileCells;
        TileCell cell = MapManager.GetCellAtPosition(tilePos);
        gridObj.SetLocation(cell);
    }
}

public enum GameState
{
    INITIALIZE,
    PLAYER_TURN,
    ENEMY_TURN,
}

public enum GameSubState
{
    MOVE,
    ATTACK,
    MAGIC,
    SKILL,
    ITEM,
}
