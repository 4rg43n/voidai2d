using RX.AI.Orders;
using RX.UI;
using RX.Utils;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Singleton;

    public MapManager MapManager;

    public FloatingTextUI floatingTextPrefab;

    public GridObject[] testGridObjPrefab;
    public Vector2Int[] testGridPosition;

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

        for(int i=0;i<testGridObjPrefab.Length;i++)
        {
            testGridObj = Instantiate(testGridObjPrefab[i]);
            SetLocation(testGridObj, testGridPosition[i]);
        }
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
        if (!isOverUI && Input.GetMouseButtonDown(0))
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

            if (GameSubState == GameSubState.MOVE)
            {
                bool isMove = GameSubState == GameSubState.MOVE && MapManager.IsClickedSelectedPath(cell, true) && MapManager.SelectedCell.Contents != null;

                TileCell src = MapManager.SelectedCell;
                TileCell dst = cell;
                List<TileCell> path = MapManager.GetShortestPath(src, dst);
                MapManager.SelectPath(path);

                if (isMove)
                {
                    MoveObject(src, dst, path);
                }
            }
            else if (GameSubState == GameSubState.ATTACK)
            {
                if (MapManager.IsClickedSelectedArea(cell) && MapManager.SelectedCell.Contents != null && cell.Contents != null)
                {
                    UseAbilityObject(MapManager.SelectedCell, cell);
                }
                else
                {
                    MapManager.DeselectArea();
                }
            }
            else if (GameSubState == GameSubState.MAGIC)
            {
                if (MapManager.IsClickedSelectedArea(cell) && MapManager.SelectedCell.Contents != null && cell.Contents != null)
                {
                    UseAbilityObject(MapManager.SelectedCell, cell);
                }
                else
                {
                    MapManager.DeselectArea();
                }
            }
            else if (GameSubState == GameSubState.SKILL)
            {
                if (MapManager.IsClickedSelectedArea(cell) && MapManager.SelectedCell.Contents != null && cell.Contents != null)
                {
                    UseAbilityObject(MapManager.SelectedCell, cell);
                }
                else
                {
                    MapManager.DeselectArea();
                }
            }
            else if (GameSubState == GameSubState.ITEM)
            {
                if (MapManager.IsClickedSelectedArea(cell) && MapManager.SelectedCell.Contents != null && cell.Contents != null)
                {
                    UseAbilityObject(MapManager.SelectedCell, cell);
                }
                else
                {
                    MapManager.DeselectArea();
                }
            }
        }
    }

    public void ShowFloatingText(string text, Vector3 position, Color color)
    {
        FloatingTextUI floatingText = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        floatingText.SetText(color, text);
    }

    void SelectArea()
    {
        if (GameSubState != GameSubState.MOVE && MapManager.SelectedCell != null)
        {
            List<TileCell> area = MapManager.GetCellsInRange(MapManager.SelectedCell, 3, false);
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

    GameAbility GetAbilityByGameSubState()
    {
        return GameSubState switch
        {
            GameSubState.ATTACK => MapManager.SelectedCell.Contents.attackAbility,
            GameSubState.MAGIC => MapManager.SelectedCell.Contents.magicAbility,
            GameSubState.SKILL => MapManager.SelectedCell.Contents.skillAbility,
            GameSubState.ITEM => MapManager.SelectedCell.Contents.itemAbility,
            _ => null,
        };
    }

    public GameSubState SetGameSubState(GameSubState subState)
    {
        GameSubState = subState;

        MapManager.DeselectAll();

        return GameSubState;
    }

    public void UseAbilityObject(TileCell src, TileCell dst)
    {
        GameAbility ability = GetAbilityByGameSubState();
        if (ability != null)
        {
            UseAbilityObject(src, dst, ability);
        }
    }

    public void UseAbilityObject(TileCell src, TileCell dst, GameAbility ability)
    {
        GameAbility abilityInst = Instantiate(ability);
        abilityInst.transform.parent = src.Contents.transform;
        Orders.Add(new GridOrderUtils.UseGameAbilityOrder(src.Contents, dst, abilityInst));
    }

    public void MoveObject(TileCell src, TileCell dst, List<TileCell> path)
    {
        Orders.Add(new GridOrderUtils.GridMoveOrder(src.Contents, path, 0.25f));
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
