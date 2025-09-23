using System;
using System.Collections.Generic;

namespace VoidAI.Pathfinding
{
    public static class AStarSearch
    {
        // ---------------------------- A* (Manhattan, no diagonals) ----------------------------
        /// <summary>
        /// Finds the shortest path from src to dst using A* (Manhattan heuristic; no diagonals).
        /// Returns a list including src and dst (empty if no path).
        /// If testWalkable = true, nodes with IsWalkable == false are skipped.
        /// </summary>
        public static List<IAStarNode> FindShortestPath(IAStarNode src, IAStarNode dst, bool testWalkable)
        {
            if (src == null || dst == null) return _empty;

            var open = new List<IAStarNode>(64);
            var inOpen = new HashSet<IAStarNode>();
            var closed = new HashSet<IAStarNode>();
            var touched = new HashSet<IAStarNode>();

            src.G = 0f;
            src.H = Manhattan(src, dst);
            src.Parent = null;
            open.Add(src);
            inOpen.Add(src);
            touched.Add(src);

            while (open.Count > 0)
            {
                // ⬇️ pass src,dst so we can apply line-following tie-break
                var current = PopBest(open, inOpen, src, dst);

                if (current == dst)
                {
                    var path = Reconstruct(dst);
                    // ResetTouched(touched);
                    return path;
                }

                closed.Add(current);

                foreach (var nb in current.GetNeighbors4())
                {
                    if (nb == null) continue;
                    if (testWalkable && !nb.IsWalkable) continue;
                    if (closed.Contains(nb)) continue;

                    float tentativeG = current.G + Math.Max(0.0001f, nb.Distance);

                    bool discovered = !inOpen.Contains(nb);
                    if (discovered || tentativeG < nb.G)
                    {
                        nb.Parent = current;
                        nb.G = tentativeG;
                        nb.H = Manhattan(nb, dst);
                        touched.Add(nb);

                        if (discovered)
                        {
                            open.Add(nb);
                            inOpen.Add(nb);
                        }
                    }
                }
            }
            return _empty;
        }

        // ---------------------------- Dijkstra (range) ---------------------------------------
        /// <summary>
        /// Returns all tiles whose movement cost from src is <= range (excluding src).
        /// Uses Dijkstra (costs are node.Distance). If testWalkable, skips non-walkable tiles.
        /// </summary>
        public static List<IAStarNode> FindRange(IAStarNode src, int range, bool testWalkable)
        {
            if (src == null || range < 0) return _empty;

            var result = new List<IAStarNode>();
            var costSoFar = new Dictionary<IAStarNode, float>();
            var frontier = new List<IAStarNode>(); // min-FIFO by linear scan (fine for modest grids)
            costSoFar[src] = 0f;
            frontier.Add(src);

            while (frontier.Count > 0)
            {
                var current = PopLowestCost(frontier, costSoFar);
                float currentCost = costSoFar[current];

                foreach (var nb in current.GetNeighbors4())
                {
                    if (nb == null) continue;
                    if (testWalkable && !nb.IsWalkable) continue;

                    float newCost = currentCost + Math.Max(0.0001f, nb.Distance);
                    if (newCost > range) continue;

                    if (!costSoFar.TryGetValue(nb, out var prev) || newCost < prev)
                    {
                        costSoFar[nb] = newCost;
                        frontier.Add(nb);

                        if (nb != src && !result.Contains(nb))
                            result.Add(nb);
                    }
                }
            }

            return result;
        }

        // ---------------------------- Pattern (offset mask) -----------------------------------
        /// <summary>
        /// pattern[y,x]: -1 marks the source cell within the pattern, 1 = include, 0 = ignore.
        /// Returns all nodes at positions (src.X + dx, src.Y + dy) for each '1'.
        /// If testWalkable, skips nodes where IsWalkable is false. Obstacles between src and target are ignored.
        /// </summary>
        public static List<IAStarNode> FindPattern(IAStarNode src, int[,] pattern, bool testWalkable)
        {
            if (src == null || pattern == null) return _empty;

            int h = pattern.GetLength(0); // rows (Y)
            int w = pattern.GetLength(1); // cols (X)

            // locate origin (-1) in the pattern
            int ox = -1, oy = -1;
            for (int y = 0; y < h && oy < 0; y++)
                for (int x = 0; x < w; x++)
                    if (pattern[y, x] == -1) { ox = x; oy = y; break; }

            if (ox < 0 || oy < 0)
                throw new ArgumentException("FindPattern: pattern must contain exactly one -1 for the origin.");

            var list = new List<IAStarNode>();

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (pattern[y, x] != 1) continue;

                    int dx = x - ox;
                    int dy = y - oy;
                    int tx = src.X + dx;
                    int ty = src.Y + dy;

                    var node = src.GetNodeAt(tx, ty);
                    if (node == null) continue;
                    if (testWalkable && !node.IsWalkable) continue;

                    list.Add(node);
                }
            }

            return list;
        }

        // ---------------------------- Helpers -----------------------------------------------

        private static readonly List<IAStarNode> _empty = new();

        private static float Manhattan(IAStarNode a, IAStarNode b)
            => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);

        private static IAStarNode PopBest(List<IAStarNode> open, HashSet<IAStarNode> inOpen, IAStarNode start, IAStarNode goal)
        {
            // Direction from start to goal (used for line-distance & forward-progress)
            long dx = goal.X - start.X;
            long dy = goal.Y - start.Y;

            const float EPS = 1e-5f;

            int bestIndex = 0;
            float bestF = float.MaxValue;
            long bestDev = long.MaxValue;  // perpendicular distance proxy (area) to the start→goal line
            long bestProg = long.MinValue; // forward progress along the line
            float bestH = float.MaxValue;

            for (int i = 0; i < open.Count; i++)
            {
                var n = open[i];
                float f = n.G + n.H;

                if (f < bestF - EPS)
                {
                    bestF = f;
                    bestIndex = i;
                    // recompute tie-break metrics for new best
                    long px = n.X - start.X;
                    long py = n.Y - start.Y;
                    bestDev = LineDeviationArea(dx, dy, px, py);
                    bestProg = LineProgress(dx, dy, px, py);
                    bestH = n.H;
                    continue;
                }

                if (Math.Abs(f - bestF) <= EPS)
                {
                    long px = n.X - start.X;
                    long py = n.Y - start.Y;
                    long dev = LineDeviationArea(dx, dy, px, py); // smaller = closer to straight line
                    long prog = LineProgress(dx, dy, px, py);      // larger = more forward along line

                    if (dev < bestDev ||
                       (dev == bestDev && prog > bestProg) ||
                       (dev == bestDev && prog == bestProg && n.H < bestH))
                    {
                        bestIndex = i;
                        bestDev = dev;
                        bestProg = prog;
                        bestH = n.H;
                    }
                }
            }

            var best = open[bestIndex];
            int last = open.Count - 1;
            open[bestIndex] = open[last];
            open.RemoveAt(last);
            inOpen.Remove(best);
            return best;
        }

        // |dx*py - dy*px| == 2x triangle area ~ proportional to perpendicular distance.
        // We skip dividing by sqrt(dx^2+dy^2) because it's constant for this search.
        private static long LineDeviationArea(long dx, long dy, long px, long py)
            => Math.Abs(dx * py - dy * px);

        // Dot product with the (dx,dy) direction. Larger = further along start→goal.
        private static long LineProgress(long dx, long dy, long px, long py)
            => dx * px + dy * py;

        private static IAStarNode PopLowestCost(List<IAStarNode> list, Dictionary<IAStarNode, float> cost)
        {
            int idx = 0; float best = float.MaxValue;
            for (int i = 0; i < list.Count; i++)
            {
                var n = list[i];
                float c = cost.TryGetValue(n, out var v) ? v : float.MaxValue;
                if (c < best) { best = c; idx = i; }
            }
            var picked = list[idx];
            int last = list.Count - 1;
            list[idx] = list[last];
            list.RemoveAt(last);
            return picked;
        }

        private static List<IAStarNode> Reconstruct(IAStarNode end)
        {
            var path = new List<IAStarNode>();
            var cur = end;
            while (cur != null)
            {
                path.Add(cur);
                cur = cur.Parent;
            }
            path.Reverse();
            return path;
        }

        // If you want the algorithm to clean up node scratch fields after a search,
        // call this with a set of touched nodes you've tracked.
        public static void ResetTouched(HashSet<IAStarNode> touched)
        {
            if (touched == null) return;
            foreach (var n in touched)
            {
                n.G = 0f; n.H = 0f; n.Parent = null;
            }
            touched.Clear();
        }
    }
}
