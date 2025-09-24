using RX.AI.Orders;
using UnityEngine;

namespace RX.AI.Orders
{
    /// <summary>
    /// Factory class for creating compound orders
    /// </summary>
    public static class OrderUtils
    {
        public class WaitOrder : BasicOrder
        {
            float timeout;

            public WaitOrder(float time) : base("WaitOrder", new object[] { time })
            {
                timeout = time;
            }

            public override bool DoUpdate(object executer, OrderArgs args)
            {
                timeout -= Time.deltaTime;

                if (timeout < 0)
                    return true;
                else
                    return false;
            }
        }

        public abstract class BasicOrder : Order
        {
            public BasicOrder(string name, object[] args) : base(name, new OrderArgs(args))
            {
            }

            public override void DoStart(object executer, OrderArgs args)
            {
            }
            public override bool DoUpdate(object executer, OrderArgs args)
            {
                return true;
            }
            public override void DoComplete(object executer, OrderArgs args)
            {
            }
            public override void DoCancel(object executer, OrderArgs args)
            {
                DoComplete(executer, args);
            }
        }
    }
}



