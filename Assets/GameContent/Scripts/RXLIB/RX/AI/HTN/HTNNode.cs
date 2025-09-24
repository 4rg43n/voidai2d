using RX.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI.HTN
{
    // This is the heart of the HTN implementation. The node contains all the relationships and nodes
    // required to build the HTN plan
    public class HTNNode
    {
        // delegates for precondtions and actions
        public delegate bool PreconditionDel(HTNNode node);
        public delegate void ExecActionDel(HTNNode node);
        public delegate void InitializeDel(HTNNode node);

        // node name
        public string Name { get; set; }

        // parent node
        public HTNNode Parent { get; set; }
        // children of this node
        List<HTNNode> children = new List<HTNNode>();
        public List<HTNNode> Children { get { return children; } }

        // is this a leaf node
        public bool IsLeaf { get { return Children == null || Children.Count < 1; } }

        // the precondition delegate
        protected PreconditionDel precondition { get; set; }
        // the action delegate
        protected ExecActionDel execute { get; set; }
        public InitializeDel initialize { get; set; }
        public InitializeDel extraInitialize { get; set; }

        public bool HasPrecondition { get { return precondition != null; } }
        public bool HasExecute { get { return execute != null; } }

        // node type
        public HTNNodeType Type { get; set; }
        // tree that owns this node. This is only valid on the root node. To find the 
        // parent of the tree from any other node you need to recurse up the tree to
        // the root
        public HTNGoalTree goalTree;
        // link to the local blackboard for storing/retrieving information
        public AIBlackboard Blackboard { get { return GetTree().GoalContainer.Owner.Blackboard; } }
        public HTNSystem System { get { return GetTree().GoalContainer.Owner; } }

        // if it's a precondition we want to flip (saves us from writing !Precon nodes)
        bool flipPrecon = false;
        public bool FlipPrecon { get { return flipPrecon; } set { flipPrecon = value; } }

        // constructor
        public HTNNode(string name)
        {
            Name = name;
            Type = HTNNodeType.DEFAULT;
        }

        // constructor
        public HTNNode(string name, PreconditionDel precon)
        {
            Name = name;
            Type = HTNNodeType.DEFAULT;
            precondition = precon;
        }

        // constructor
        public HTNNode(string name, ExecActionDel exec)
        {
            Name = name;
            Type = HTNNodeType.DEFAULT;
            execute = exec;
        }

        public bool Precondition()
        {
            if (precondition == null)
                Debug.Log("WTF!");

            if (flipPrecon)
                return !precondition(this);

            return precondition(this);
        }

        public void Execute()
        {
            execute(this);
        }

        public void Initialize()
        {
            if (extraInitialize != null)
                extraInitialize(this);
            if (initialize != null)
                initialize(this);
        }

        // add a child to this node
        public HTNNode AddChild(HTNNode node)
        {
            node.Parent = this;
            children.Add(node);

            return node;
        }

        // ToString
        public override string ToString()
        {
            return "Node - " + Type + " - " + Name + " - Precon:" + HasPrecondition + " - Exec:" + HasExecute;
        }

        // get the tree this node is a part of
        public HTNGoalTree GetTree()
        {
            //HTNNode node = this;

            //while (node.Parent != null)
            //    node = node.Parent;

            //return node.goalTree;
            return GetRoot().goalTree;
        }

        // get the tree root node
        public HTNNode GetRoot()
        {
            HTNNode node = this;

            while (node.Parent != null)
                node = node.Parent;

            return node;
        }

        // get the path to the root
        public List<HTNNode> GetPath()
        {
            List<HTNNode> path = new List<HTNNode>();
            HTNNode node = this;

            while (node != null)
            {
                path.Add(node);
                node = node.Parent;
            }

            path.Reverse();

            return path;
        }

        public string GetPathDesc()
        {
            List<HTNNode> path = GetPath();
            string desc = "";
            foreach (HTNNode p in path)
            {
                desc += p.Name + ":";
            }

            return desc.Substring(0, desc.Length - 1);
        }
    }

    // Node type
    public enum HTNNodeType
    {
        DEFAULT = 0, // internal, precondition, leafs, etc.
        INTERUPT,    // if an action is an interupt then stop processing the tree
        GOAL,        // if an action is a goal then stop processing the tree and the tree is complete
    }
}
