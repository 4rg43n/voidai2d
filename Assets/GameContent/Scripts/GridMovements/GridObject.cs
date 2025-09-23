using System.Collections.Generic;
using UnityEngine;
using VoidAI.Pathfinding;

public class GridObject : MonoBehaviour
{
    public Vector2 OriginOffset = new Vector2(0.5f, 0);

    [HideInInspector]
    public TileCell Location=null;

    public void SetLocation(TileCell cell)
    {
        if (cell == null)
        {
            if (Location != null)
            {
                Location.ClearContents();
                Location = null;
            }
        }
        else
        {
            cell.SetContents(this);
            Location = cell;
        }
    }

    public virtual void OnLocationEntered() // the Location will be the one entered
    { }
    public virtual void OnLocationExited(TileCell oldLoc) // the Location will be null here
    { }

}

