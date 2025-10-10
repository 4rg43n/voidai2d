using System.Collections.Generic;
using UnityEngine;
using VoidAI.Pathfinding;

public class GridObject : MonoBehaviour
{
    public Vector2 OriginOffset = new Vector2(0.5f, 0);

    [HideInInspector]
    public TileCell Location=null;

    public GameAbility attackAbility; // if set, this ability will be used for basic attacks
    public GameAbility magicAbility; // if set, this ability will be used for magic attacks
    public GameAbility skillAbility; // if set, this ability will be used for skill attacks
    public GameAbility itemAbility; // if set, this ability will be used for item use

    public void SetLocation(TileCell cell)
    {
        if (Location != null)
        {
            Location.ClearContents();
            Location = null;
        }

        if (cell != null)
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

