using System.Collections.Generic;
using UnityEngine;

namespace VoidAI.Pathfinding
{
    /// <summary>
    /// A single square-grid cell for pathfinding.
    /// IMPORTANT:
    ///  - GetNeighbors4() must return ONLY the 4-orthogonal neighbors (no diagonals).
    ///  - Distance is the cost to ENTER this node (typically 1).
    ///  - G/H are per-search scratch values that A* sets. F should be G+H in your implementation.
    ///  - GetNodeAt(x,y) is used by FindPattern (to translate offsets into nodes).
    /// </summary>
    public interface IAStarNode
    {
        int X { get; }
        int Y { get; }

        bool IsWalkable { get; }
        float Distance { get; } // movement cost to ENTER this node (1 for uniform grids)

        // Search scratch (set/reset by the algorithm for debugging/inspection, optional for you to use)
        float G { get; set; }   // cost from start to this node
        float H { get; set; }   // heuristic estimate to the goal (Manhattan)
        float F { get; }        // usually G + H
        IAStarNode Parent { get; set; }

        IEnumerable<IAStarNode> GetNeighbors4();   // up/down/left/right only
        IAStarNode GetNodeAt(int x, int y);        // look up any cell by grid coords (used by FindPattern)
    }
}
