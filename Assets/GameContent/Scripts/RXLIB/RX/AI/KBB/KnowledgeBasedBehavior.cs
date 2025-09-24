//using RX.AI.Actor;
//using RX.AI.HTN;
using UnityEngine;

namespace RX.AI.KBB
{
    public class KnowledgeBasedBehavior<T>
    {
        public string name;

        public delegate float UpdateUtilityMethod(T ai);
        public delegate void ExecuteUtilityMethod(T ai);

        public float utility;

        public UpdateUtilityMethod updateMeth;
        public ExecuteUtilityMethod executeMeth;

        public bool executing = false;

        public KnowledgeBasedBehavior(string name, UpdateUtilityMethod uum)
        {
            this.name = name;
            updateMeth = uum;
        }

        public KnowledgeBasedBehavior(string name, UpdateUtilityMethod uum, ExecuteUtilityMethod eum)
        {
            this.name = name;
            updateMeth = uum;
            executeMeth = eum;
        }

        public virtual void UpdateUtility(T ai)
        {
            utility = updateMeth(ai);
        }

        public virtual void Execute(T ai)
        {
            executeMeth(ai);
        }

        public override string ToString()
        {
            return base.ToString() + ":" + name;
        }
    }

    //public class HTN_KBB : KnowledgeBasedBehavior<BasicAIActor>
    //{
    //    public delegate HTNGoalTree CreateEXETree(BasicAIActor ai);

    //    public CreateEXETree exeMeth;

    //    HTNGoalTree tree;

    //    public HTN_KBB(string name, UpdateUtilityMethod um, CreateEXETree ex) : base(name, um)
    //    {
    //        exeMeth = ex;
    //    }

    //    public override void Execute(BasicAIActor ai)
    //    {
    //        tree = exeMeth(ai);

    //        ai.htnSystem.OnGoalCompleteEvent += HtnSystem_OnGoalCompleteEvent;
    //        ai.htnSystem.AddGoalTree(tree);

    //        executing = true;
    //    }

    //    private void HtnSystem_OnGoalCompleteEvent(string stack, HTNGoalTree tree)
    //    {
    //        if (tree == this.tree)
    //        {
    //            // executing
    //            executing = false;
    //        }
    //    }
    //}
}
