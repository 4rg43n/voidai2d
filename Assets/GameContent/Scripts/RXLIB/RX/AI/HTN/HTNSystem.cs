using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI.HTN
{
    // HTNSystem - This class manages the container and planning process. Its the interface provided to the 
    // game to create and execute plans
    public class HTNSystem : MonoBehaviour
    {
        // delegates for the Plan events
        public delegate void OnPlan(GameObject planIsFor, Dictionary<string, List<HTNNode>> plan);
        public delegate void OnPlanAndExecute(GameObject planIsFor);
        public delegate void OnPlanComplete();
        public delegate void OnGoalCompleteDel(string stack, HTNGoalTree tree);

        public string Name { get; set; }

        // when to update
        public HTNSystemUpdateType updateTimeType = HTNSystemUpdateType.EVERY_FRAME;
        // how to plan
        public HTNSystemType updateType = HTNSystemType.PLAN_AND_EXECUTE;
        // how often to update if we're planning on a fixed time interval
        public float updateTime = 0.25f;

        // goal container we're managing
        public HTNGoalContainer goalContainer;

        // a local blackboard used to store information local to the trees in this container
        public AIBlackboard Blackboard { get; set; }


        // events that other objects can tie into to get information when this
        // entity runs the planner
        public OnPlan OnPlanEvent;
        public OnPlanAndExecute OnPlanAndExecuteEvent;
        public event OnPlanComplete OnPlanCompleteEvent;
        public event OnGoalCompleteDel OnGoalCompleteEvent;

        // stop the tree from running
        bool stop = false;
        public bool Stop {
            get { return stop; }
            set {
                // if we're turning the tree back on and its a fixed time update
                // then restart the coroutine
                if (stop && !value && updateTimeType == HTNSystemUpdateType.TIMED)
                {
                    stop = value;
                    StartCoroutine(RunCo());
                }

                stop = value;
            }
        }

        void Awake()
        {
            // create the container
            goalContainer = new HTNGoalContainer(this);
            // TODO: This is a test tree. Will be removed later
            //AddGoalTree(HTNGoalTreeFactory.CreateTestTree());
        }

        // Use this for initialization
        void Start()
        {
            // if we're on a fixed time update then start the coroutine
            if (updateTimeType == HTNSystemUpdateType.TIMED)
                StartCoroutine(RunCo());

            goalContainer.OnGoalCompleteEvent += OnGoalComplete;
        }

        // Update is called once per frame
        private void Update()
        {
            // if we're updating every frame then update
            if (!Stop && updateTimeType == HTNSystemUpdateType.EVERY_FRAME)
            {
                Run();
            }
        }

        public void OnGoalComplete(string stack, HTNGoalTree tree)
        {
            goalContainer.FeedStack(stack);

            if (OnGoalCompleteEvent != null)
                OnGoalCompleteEvent(stack, tree);
        }

        public void FeedGoalTreeStack(string stack, HTNGoalTree goalTree)
        {
            goalContainer.FeedGoalTree(stack, goalTree);

            if (goalContainer.IsStackEmpty(stack))
            {
                OnGoalComplete(stack, null);
            }
        }

        public void Clear()
        {
            goalContainer.Clear();
        }

        // coroutine to call the run method
        IEnumerator RunCo()
        {
            while (!Stop)
            {
                yield return new WaitForSeconds(updateTime);
                Run();
            }
        }

        // Run - this is the most important method of the class is calls the planner
        public void Run()
        {
            if (updateType == HTNSystemType.PLAN_AND_EXECUTE)
            {
                PlanAndExecute();
            }
            else
            {
                Plan();
            }
        }

        // create the plan for this entity
        Dictionary<string, List<HTNNode>> Plan()
        {
            Dictionary<string, List<HTNNode>> res = goalContainer.Plan();

            // if someone is listening then notify them the plan was created
            if (OnPlanEvent != null)
                OnPlanEvent(gameObject, res);

            // is someone is listening and there are no more plans then notify that we're done
            if (goalContainer.IsEmpty && OnPlanCompleteEvent != null)
                OnPlanCompleteEvent();

            return res;
        }

        // create the plan for this entity and execute it
        void PlanAndExecute()
        {
            Dictionary<string, List<HTNNode>> res = goalContainer.Plan();

            // if the plan produced something then execute it
            if (res != null && res.Count > 0)
            {
                // execute the plan
                foreach (List<HTNNode> plan in res.Values)
                {
                    foreach (HTNNode node in plan)
                    {
                        if (node.HasExecute)
                            node.Execute();
                    }
                }
            }

            goalContainer.CleanUpGoals();

            // if anyone is listening, then inform them we created a plan and executed it
            if (OnPlanAndExecuteEvent != null)
                OnPlanAndExecuteEvent(gameObject);

            // is someone is listening and there are no more plans then notify that we're done
            if (goalContainer.IsEmpty && OnPlanCompleteEvent != null)
                OnPlanCompleteEvent();

        }

        // add a goal tree to the default stack
        public void AddGoalTree(HTNGoalTree goalTree)
        {
            AddGoalTree("default", goalTree);
        }

        // add a goal tree to the specified stack
        public void AddGoalTree(string treeStack, HTNGoalTree goalTree)
        {
            goalContainer.AddGoalTree(treeStack, goalTree);
        }
    }

    // htn system type
    public enum HTNSystemType
    {
        PLAN_AND_EXECUTE = 0, // plan and execute
        PLAN_ONLY,            // create the plan only
    }

    // htn system update type
    public enum HTNSystemUpdateType
    {
        EVERY_FRAME = 0, // plan every frame
        MANUAL,          // don't plan, something will call the plan its self
        TIMED,           // plan on a fixed time interval
    }
}
