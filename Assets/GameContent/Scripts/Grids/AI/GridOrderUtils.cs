using RX.AI.Orders;
using System.Collections.Generic;
using UnityEngine;
using static RX.AI.Orders.OrderUtils;

public static class GridOrderUtils
{
    public class GridMoveOrder:GridBasicOrder
    {
        List<TileCell> path = null;
        float time = 0;

        float currentTime = 0;
        Vector3 startPosition;

        public GridMoveOrder(GridObject actor, List<TileCell> path, float time):base(actor, "GridMoveOrder", new object[] {path,time})
        {
        }

        public override void DoStart(object executer, OrderArgs args)
        {
            path = new List<TileCell>((List<TileCell>)args.args[0]);
            time = (float)args.args[1];

            if (path[0] == actor.Location)
                path.RemoveAt(0);

            startPosition = actor.transform.position;

            GameManager.Singleton.MapManager.DeselectCell();
        }

        public override bool DoUpdate(object executer, OrderArgs args)
        {
            float t = currentTime / time;

            Vector3 move = Vector3.Lerp(startPosition, path[0].WorldPosition + (Vector3)actor.OriginOffset, Mathf.Clamp01(t));
            actor.transform.position = move;

            if (currentTime >= time)
            {
                currentTime = 0;
                actor.SetLocation(path[0]);
                startPosition = actor.transform.position;

                path.RemoveAt(0);
                if (path.Count == 0)
                    return true;
            }
            else
                currentTime += Time.deltaTime;

            return false;
        }

        public override void DoComplete(object executer, OrderArgs args)
        {
            GameManager.Singleton.MapManager.DeselectPath();
            GameManager.Singleton.MapManager.SelectCell(actor.Location);
        }
    }

    public abstract class GridBasicOrder : BasicOrder
    {
        protected GridObject actor;

        public GridBasicOrder(GridObject actor, string name, object[] args):base(name, args)
        {
            this.actor = actor;
        }
    }
}
