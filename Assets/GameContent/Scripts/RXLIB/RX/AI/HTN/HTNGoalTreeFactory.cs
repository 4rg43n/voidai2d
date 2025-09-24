using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI.HTN
{
    // factory system for building trees
    public static class HTNGoalTreeFactory
    {

        public static HTNGoalTree CreateGoalTree(HTNNode root)
        {
            return new HTNGoalTree(root);
        }

        #region TestTree
        // a test tree. A1 node only executes every other Plan
        public static HTNGoalTree CreateTestTree()
        {
            HTNNode root = new HTNNode("root");
            HTNNode p1 = new HTNNode("p1", (n) =>
            {
                int num = 0;// n.Blackboard.GetElement<int>("counter");
                //n.Blackboard.SetElement<int>("counter", num + 1);
                return num % 2 == 0;
            });
            HTNNode a1 = new HTNNode("a1", (n) =>
            {
                //Debug.Log("A1: " + n.Blackboard.GetElement<int>("counter"));
            });
            HTNNode a2 = new HTNNode("a2", (n) =>
            {
                Debug.Log("A2");
            });
            //a2.Type = HTNNodeType.GOAL;

            p1.FlipPrecon = true;
            root.AddChild(p1);
            root.AddChild(a2);
            p1.AddChild(a1);

            return new HTNGoalTree(root);
        }
        #endregion

        #region Utilities
        // print result of the plan
        public static void PrintResult(List<HTNNode> res)
        {
            string desc = "";
            foreach (HTNNode r in res)
                desc += r.ToString() + ", ";
            Debug.Log("Plan result: " + desc);
        }
        #endregion
    }
}
