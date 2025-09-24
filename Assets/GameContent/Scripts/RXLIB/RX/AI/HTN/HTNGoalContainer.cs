using RX.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI.HTN
{
    // HTNGoalContainer 
    // The Goal Container encapsulates all the trees for an entity/object and is responsible for calling
    // all the trees to plan
    // Execution of the plans is left to something else (in this case the HTNSystem)
    public class HTNGoalContainer
    {
        public delegate void OnGoalCompleteDel(string stack, HTNGoalTree tree);

        static Dictionary<string, HTNGoalTreeContainerType> stackDef;

        // all the goal containers in the game and the objects they are components of
        public static Dictionary<GameObject, HTNGoalContainer> allGoalContainers = new Dictionary<GameObject, HTNGoalContainer>();

        // all the goal stacks this container is managing
        // the key to the map is the name of the stack
        Dictionary<string, HTNGoalTreeContainer> goalStacks = new Dictionary<string, HTNGoalTreeContainer>();
        Dictionary<string, Queue<HTNGoalTree>> goalStackFeeder = new Dictionary<string, Queue<HTNGoalTree>>();

        public HTNSystem Owner { get; set; }

        // flag to make sure we don't necessarily flag a newly created container as empty
        bool isFirstGoalAdded = false;

        public event OnGoalCompleteDel OnGoalCompleteEvent;

        public bool IsEmpty { get { return isFirstGoalAdded && goalStacks.Count < 1; } }


        // trees that need to be popped Plan generates this then after execute we pop
        // them to make sure we get events in the right order
        Dictionary<string, HTNGoalTree> toPopTree = null;

        public string GetStackName(HTNNode n)
        {
            HTNNode root = n.GetRoot();
            foreach(string key in goalStacks.Keys)
            {
                HTNGoalTreeContainer gtc = goalStacks[key];
                if (gtc.Peek().root == root)
                    return key;
            }

            return null;
        }

        public void AddDef(string stack, HTNGoalTreeContainerType stackType)
        {
            stackDef[stack] = stackType;
        }

        public bool IsStackEmpty(string stack)
        {
            if (!goalStacks.ContainsKey(stack))
                return true;
            if (goalStacks[stack].Count < 1)
                return true;

            return false;
        }

        public void Clear()
        {
            foreach(HTNGoalTreeContainer gtc in goalStacks.Values)
            {
                gtc.Clear();
            }

            foreach(Queue<HTNGoalTree> q in goalStackFeeder.Values)
            {
                q.Clear();
            }

            goalStacks.Clear();
            goalStackFeeder.Clear();
        }

        HTNGoalTreeContainerType GetDef(string stack)
        {
            if (!stackDef.ContainsKey(stack))
            {
                Debug.LogWarning("No definition found for stack " + stack);
                return HTNGoalTreeContainerType.STACK;
            }

            return stackDef[stack];
        }

        // adds a goal tree to the "default" stack. Default is the stack all containers have
        public void AddGoalTree(HTNGoalTree goalTree)
        {
            AddGoalTree("default", goalTree);
        }

        // adds a goal tree to a specific stack
        public void AddGoalTree(string stack, HTNGoalTree goalTree)
        {
            // if the stack does not exist create it
            if (!goalStacks.ContainsKey(stack))
            {
                goalStacks[stack] = new HTNGoalTreeContainer(GetDef(stack));
            }

            // add the stack
            goalStacks[stack].Push(goalTree);
            goalTree.GoalContainer = this;
            isFirstGoalAdded = true;

            // initialize the tree
            goalTree.Init();
        }

        public void FeedGoalTree(HTNGoalTree goalTree)
        {
            FeedGoalTree("default", goalTree);
        }

        public void FeedGoalTree(string stack, HTNGoalTree goalTree)
        {
            // if the quere does not exist creat it
            if (!goalStackFeeder.ContainsKey(stack))
            {
                goalStackFeeder[stack] = new Queue<HTNGoalTree>();
            }

            // add to the queue
            goalStackFeeder[stack].Enqueue(goalTree);
        }

        public void FeedStack()
        {
            FeedStack("default");
        }

        public bool FeedStack(string stack)
        {
            if (!goalStackFeeder.ContainsKey(stack))
                return false;
            if (goalStackFeeder[stack].Count < 1)
                return false;

            HTNGoalTree gt = goalStackFeeder[stack].Dequeue();
            AddGoalTree(stack, gt);

            return true;
        }

        // pops a tree off the specified stack
        public HTNGoalTree PopTree(string stack)
        {
            // if the stack doesn't exist then we have an error
            if (!goalStacks.ContainsKey(stack))
            {
                Debug.LogError("Cannot pop tree from stack. No stack named '" + stack + "'.");
                return null;
            }

            // get the specified goal stack
            HTNGoalTreeContainer goalStack = goalStacks[stack];

            // if the goal stack is empty we have an error. 
            // Empty stacks should have been removed
            if (goalStack.Count < 1)
            {
                Debug.LogError("Cannot pop tree from '" + stack + "' stack. Stack is empty.");
                goalStacks.Remove(stack);
                return null;
            }

            // pop the stack
            HTNGoalTree goalTree = goalStack.Remove();
            // if its empty remove the stack
            if (goalStack.Count < 1)
            {
                goalStacks.Remove(stack);
            }

            // Note: If the container is ever empty the play has "finished"
            // this might complicate depending on how subswarms are implemented

            return goalTree;
        }

        // Plan is most important method in the class. It iterates over all the stacks
        // and calls plan on all the trees. It then builds a map of all the resulting
        // plans. The key to the result map is the name of the stack that produced that
        // list of actions.
        public Dictionary<string, List<HTNNode>> Plan()
        {
            Dictionary<string, List<HTNNode>> goal = null;
            toPopTree = null;

            // iterate over the goal statcks and call plan on each one
            // then store the resulting plan in the result map
            foreach (string key in goalStacks.Keys)
            {
                HTNGoalTreeContainer goalStack = goalStacks[key];
                HTNGoalTree goalTree = goalStack.Peek();
                List<HTNNode> res = goalTree.Plan();

                // if the plan reached a goal node we need to remove the tree
                if (goalTree.LastResult == HTNNodeType.GOAL)
                {
                    if (toPopTree == null)
                    {
                        toPopTree = new Dictionary<string, HTNGoalTree>();
                    }
                    toPopTree[key] = goalTree;
                }

                // if the result produced something add it to the map
                if (res != null && res.Count > 0)
                {
                    if (goal == null)
                        goal = new Dictionary<string, List<HTNNode>>();
                    goal[key] = res;
                }
            }

            return goal;
        }

        public void CleanUpGoals()
        {
            // if we have to pop goals then iterate over the list
            // and pop each one
            if (toPopTree != null)
            {
                foreach (string pop in toPopTree.Keys)
                {
                    PopTree(pop);
                    if (OnGoalCompleteEvent != null)
                        OnGoalCompleteEvent(pop, toPopTree[pop]);
                }
            }
        }

        // Constructor
        public HTNGoalContainer(HTNSystem owner)
        {
            allGoalContainers[owner.gameObject] = this;

            Owner = owner;

            if (stackDef == null)
            {
                stackDef = new Dictionary<string, HTNGoalTreeContainerType>();
                stackDef["default"] = HTNGoalTreeContainerType.STACK;
            }
        }
    }

    public class HTNGoalTreeContainer
    {
        public Stack<HTNGoalTree> stack;
        public Queue<HTNGoalTree> queue;

        HTNGoalTreeContainerType type = HTNGoalTreeContainerType.STACK;

        public HTNGoalTreeContainer(HTNGoalTreeContainerType type)
        {
            this.type = type;

            if (type== HTNGoalTreeContainerType.QUEUE)
                queue = new Queue<HTNGoalTree>();
            else
                stack = new Stack<HTNGoalTree>();
        }

        public void Clear()
        {
            if (type == HTNGoalTreeContainerType.QUEUE)
                queue.Clear();
            else
                stack.Clear();
        }

        public int Count { get { if (type == HTNGoalTreeContainerType.STACK) return stack.Count; else return queue.Count; } }

        public void Push(HTNGoalTree tree)
        {
            if (type == HTNGoalTreeContainerType.STACK)
                stack.Push(tree);
            else
                queue.Enqueue(tree);
        }

        public HTNGoalTree Remove()
        {
            if (type == HTNGoalTreeContainerType.STACK)
                return stack.Pop();
            else
                return queue.Dequeue();
        }

        public HTNGoalTree Peek()
        {
            if (type == HTNGoalTreeContainerType.STACK)
                return stack.Peek();
            else
                return queue.Peek();
        }
    }

    public enum HTNGoalTreeContainerType
    {
        STACK = 0,
        QUEUE,
    }
}
