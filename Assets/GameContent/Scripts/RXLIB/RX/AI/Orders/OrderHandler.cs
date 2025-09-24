using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RX.AI.Orders
{
    /// <summary>
    /// This class represents a system that tracks and executes "Orders". Small execution packages that can be built up
    /// to get the entity to achieve complex actions. It's a way of telling an AI to just do something without thinking
    /// 
    /// Aside from telling an AI to just do something without having to write AI methods this system can be used by the
    /// AI classes (HTN, KBB, FSM, etc) as their "execution engine" this method can simplify the code in the BasicActor
    /// classes and move the complexity to the orders as well as provide a way for using more than one AI system without
    /// letting them "stomp" on each other. They can use the OrderHander as a means to control the logic flow
    /// 
    /// </summary>
    public class OrderHandler
    {
        public delegate void AllOrderComplete(Order lastOrder);
        public delegate void OrderComplete(Order order, bool cancelled, bool ordersFinished);

        // list of order to execute
        Queue<Order> orderList = new Queue<Order>();

        // shortcut to the current order
        public Order CurrentOrder { get { return orderList.Count > 0 ? orderList.Peek() : null; } }

        public int NumOfOrders { get { return orderList.Count; } }

        public event AllOrderComplete OnAllOrdersCompleteEvent;
        public event OrderComplete OnOrderCompleteEvent;

        public void ClearOrders()
        {
            orderList.Clear();
        }

        public void CancelOrders()
        {
            if (orderList.Count > 0)
            {
                Order topOrder = orderList.Peek();
                topOrder.execution = OrderExecutionType.CANCEL;
                orderList.Clear();
                orderList.Enqueue(topOrder);
            }
        }

        public void Insert(Order order)
        {
            Insert(new Order[] { order });
        }

        public void Insert(Order[] orders)
        {
            if (orderList.Count > 0)
            {
                orderList.Peek().execution = OrderExecutionType.START;

                List<Order> newList = new List<Order>(orderList.ToArray());
                for (int i = orders.Length - 1; i >= 0; i--)
                {
                    newList.Insert(0, orders[i]);
                }
                orderList.Clear();

                foreach (Order order in newList)
                    orderList.Enqueue(order);
            }
            else
            {
                Add(orders);
            }

        }

        /// <summary>
        /// Add orders to the queue
        /// </summary>
        /// <param name="orders">list of orders to add</param>
        public void Add(Order[] orders)
        {
            foreach (Order ord in orders)
                Add(ord);
        }

        /// <summary>
        /// Add a specific order
        /// </summary>
        /// <param name="order">order to add</param>
        public void Add(Order order)
        {
            orderList.Enqueue(order);
        }

        /// <summary>
        /// Orders are updated during the update loop
        /// </summary>
        /// <param name="sender">the object the handler is attached to, usually 'this'. It's used to send
        /// order events</param>
        public void UpdateOrders(object sender)
        {
            if (orderList.Count > 0)
            {
                if (orderList.Peek().execution == OrderExecutionType.REMOVE)
                {
                    Order ord = orderList.Dequeue();

                    if (OnOrderCompleteEvent != null)
                        OnOrderCompleteEvent(ord, ord.cancelled, orderList.Count==0);

                    if (orderList.Count == 0 && OnAllOrdersCompleteEvent != null)
                        OnAllOrdersCompleteEvent(ord);
                    return;
                }
                else
                    orderList.Peek().Update(sender);
            }
        }
    }

    /// <summary>
    /// Arguments to the order event system
    /// </summary>
    public class OrderArgs
    {
        public object[] args;

        public OrderArgs(object[] args)
        {
            this.args = args;
        }
    }

    /// <summary>
    /// This is the base order class. When implementing a new order extend this class
    /// </summary>
    public abstract class Order
    {
        // order delegate
        public delegate void OrderEventDel(object executer, OrderArgs args);

        // order name
        public string name = "Default";

        // called when the order starts executing
        public abstract void DoStart(object executer, OrderArgs args);
        // called on each successive update call. Used to check when the order is complete if the order
        // requires specific conditions (ie, moving from point a to point b) 
        // if the order is just a simple action like adding a keyword you can just implement the DoStart
        // and do it their
        public abstract bool DoUpdate(object executer, OrderArgs args);
        // called when the order is complete
        public abstract void DoComplete(object executer, OrderArgs args);
        // sometimes orders are cancelled or interrupted, this method gets called if their is any clean
        // up that needs to be done. For example: interrupting an order that moves the entity may require
        // stopping it
        public abstract void DoCancel(object executer, OrderArgs args);

        // arguments to the order
        OrderArgs args;

        // execution step
        public OrderExecutionType execution = OrderExecutionType.START;

        public bool cancelled = false;

        public Order(string name, OrderArgs args)
        {
            this.name = name;
            this.args = args;
        }

        // update the order and do anything it needs to do
        public void Update(object executer)
        {
            switch (execution)
            {
                case OrderExecutionType.START:
                    DoStart(executer, args);
                    execution = OrderExecutionType.UPDATE;
                    break;
                case OrderExecutionType.UPDATE:
                    if (DoUpdate(executer, args))
                    {
                        execution = OrderExecutionType.COMPLETE;
                    }
                    break;
                case OrderExecutionType.COMPLETE:
                    DoComplete(executer, args);
                    execution = OrderExecutionType.REMOVE;
                    break;
                case OrderExecutionType.CANCEL:
                    DoCancel(executer, args);
                    cancelled = true;
                    execution = OrderExecutionType.REMOVE;
                    break;
            }
        }
    }

    // execution states
    public enum OrderExecutionType
    {
        START,    // order is starting
        UPDATE,   // order needs updating
        COMPLETE, // order is complete
        REMOVE,   // order needs to be removed
        CANCEL,   // order needs to be cancelled
    }
}
