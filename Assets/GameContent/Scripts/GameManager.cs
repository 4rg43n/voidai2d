using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public MapManager mapManager;

    public GridObject testGridObjPrefab;
    public Vector2Int testGridPosition = new Vector2Int(0, 0);

    GridObject testGridObj;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapManager.worldGenerator.Generate();
        testGridObj = Instantiate(testGridObjPrefab);
        SetLocation(testGridObj, testGridPosition);
    }

    // Update is called once per frame
    void Update()
    {
        // Listen for left mouse button click using the old input system
        if (Input.GetMouseButtonDown(0))
        {
            mapManager.OnMouseClick(Input.mousePosition);
        }

        if (Input.GetMouseButtonDown(1))
        {
            mapManager.Deselect();
        }
    }

    public void SetLocation(GridObject gridObj, Vector2Int tilePos)
    {
        List<TileCell> testcells=mapManager.TileCells;
        TileCell cell = mapManager.GetCellAtPosition(tilePos);
        gridObj.SetLocation(cell);
    }
}
