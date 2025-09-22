using UnityEngine;

[System.Serializable]
public class TileCell
{
    public Vector2Int Position;
    public Vector3 WorldPosition;
    public int areaValue = -1; // comes from the WorldGeneration area array

    public GridObject Contents { get { return contents; } }

    GridObject contents;

    public void ClearContents()
    {
        if (contents!=null)
        {
            contents = null;
        }
        else
        {
            Debug.LogError("Tried to clear contents of location, but location is empty. Ignoring.");
            return;
        }
    }

    public void SetContents(GridObject gridObj)
    {
        if (Contents!=null)
        {
            Debug.LogError("Tried to enter location, but location is not empty. Ignoring.");
            return;
        }

        contents = gridObj;
        contents.transform.position = WorldPosition + (Vector3)gridObj.OriginOffset;
    }

    public virtual void OnSelect()
    { }

    public virtual void OnDeselect()
    { }
}
