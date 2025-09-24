using RX.AI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI.HTN
{
    // The goal tree contains the HTNNodes and is responsible for building the plan
    // described by the HTN network
    public class HTNGoalTree
    {
        // root of the HTN tree
        public HTNNode root;

        // the last result of the plan call
        public HTNNodeType LastResult { get; set; }
        public HTNNode LastResultNode { get; set; }
        // goal container that owns this tree. This is mostly
        // need to link to the blackboards
        public HTNGoalContainer GoalContainer { get; set; }

        // constructor
        public HTNGoalTree(HTNNode root)
        {
            this.root = root;
            this.root.goalTree = this;
        }

        // initialize anything that needs it
        public void Init()
        {
            _Init(root);
        }

        public void _Init(HTNNode node)
        {
            node.Initialize();

            if (node.Children.Count > 0)
            {
                foreach (HTNNode child in node.Children)
                {
                    _Init(child);
                }
            }
        }

        // build the plan
        public List<HTNNode> Plan()
        {
            List<HTNNode> goal = new List<HTNNode>();
            HTNNodeType res = HTNNodeType.DEFAULT;
            HTNNode nodeRes = null;

            // start the recursion
            _Plan(ref goal, root, ref res, ref nodeRes);

            LastResult = res;

            return goal;
        }

        // Recursive plan call
        // output = the plan as we build it
        // node = the current node we're exploring
        // returnRes = the result of the plan (ie, did we hit an Interrupt or Goal node)
        HTNNodeType _Plan(ref List<HTNNode> output, HTNNode node, ref HTNNodeType returnRes, ref HTNNode returnResNode)
        {
            // only leafs become part of the plan
            if (node.IsLeaf)
            {
                // if this is a leaf then this is something we need to execute
                output.Add(node);

                // if this is not a DEFAULT node then we're done with the Plan
                if (node.Type != HTNNodeType.DEFAULT)
                    returnRes = node.Type;
                returnResNode = node;
                return node.Type;
            }
            // if its just an internal or a precondition node that passed the precondition
            else if (!node.HasPrecondition || node.Precondition())
            {
                // if the precondition passes then pass to the children
                foreach (HTNNode child in node.Children)
                {
                    HTNNodeType res = _Plan(ref output, child, ref returnRes, ref returnResNode);
                    // if the result does not equal a DEFAULT node then stop
                    // processing
                    if (res != HTNNodeType.DEFAULT)
                    {
                        return res;
                    }
                }
            }

            // return DEFAULT
            return HTNNodeType.DEFAULT;
        }
    }
}
