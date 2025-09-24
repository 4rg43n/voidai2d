using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI.FSM
{
    //PlayerDieState dieState = new PlayerDieState("die", player);
    //transitions.Add(new FSMTransition(
    //            "Die if hp <= 0",
    //            () => { return player.charStats.hp <= 0; },
    //            dieState
    //            ));

    public class FSMStateMachine
    {
        public FSMState head;
        public FSMState current;
        public FSMTransition finalTransition;

        public void Clear()
        {
            head = null;
            current = null;
            finalTransition = null;
            Debug.Log("Clearing state machine");
        }

        public void Set(FSMState state)
        {
            if (head != null)
                Clear();
            if (state == null)
                return;

            head = state;
            Move(state);
            finalTransition = null;
            Debug.Log("Starting state machine: " + head.name);
        }

        void Move(FSMState newState)
        {
            current = newState;
            current.State = FSMStateType.ENTER_STATE;
            finalTransition = null;

            if (current.transitions == null || current.transitions.Count < 1)
            {
                Debug.LogWarning("FSM: Moved to state with no transitions. FSM can never leave.");
            }
        }


        public void Update()
        {
            switch (current.State)
            {
                case FSMStateType.ENTER_STATE:
                    // enter state
                    if (current.EnterState())
                    {
                        current.State = FSMStateType.IN_STATE;
                    }
                    return;

                case FSMStateType.IN_STATE:
                    // update the state
                    FSMTransition fsmTrans = current.UpdateState();
                    if (fsmTrans != null)
                    {
                        finalTransition = fsmTrans;
                        current.State = FSMStateType.EXIT_STATE;
                    }
                    return;

                case FSMStateType.EXIT_STATE:
                    // exit state
                    if (current.ExitState())
                    {
                        if (finalTransition.dest != null)
                            Move(finalTransition.dest);
                        else
                            Clear();
                    }
                    return;
            }
        }
    }

    public class FSMState
    {
        public bool debugState = false;
        public string name = "default";

        public delegate bool DoStateEnter();
        public delegate void DoStateUpdate();
        public delegate bool DoStateExit();

        bool changedThisFrame = false;
        public FSMStateType State { get { return fsmStateType; } set { changedThisFrame = true; fsmStateType = value; } }
        FSMStateType fsmStateType = FSMStateType.ENTER_STATE;
        public FSMTransitionCheck fsmTransitionCheck = FSMTransitionCheck.LAST;

        public List<FSMTransition> transitions = new List<FSMTransition>();

        public DoStateEnter doEnter;
        public DoStateExit doExit;
        public DoStateUpdate doUpdate;
        public DoStateUpdate doFixedUpdate;
        public DoStateUpdate doLateUpdate;

        public FSMState(string stateName)
        {
            name = stateName;
        }

        public void InitializeTransitions(FSMTransition[] fsmTransitions)
        {
            transitions = new List<FSMTransition>(fsmTransitions);
        }

        public void InitializeMethods(DoStateEnter enter, DoStateUpdate update, DoStateExit exit)
        {
            doEnter = enter;
            doUpdate = update;
            doExit = exit;
        }

        public FSMTransition CheckTransitions()
        {
            if (debugState)
            {
                Debug.Log("*********************************");
                Debug.Log("Checking transitions state: " + name);
            }

            if (transitions == null)
            {
                if (debugState)
                    Debug.Log("No transitions");
                return null;
            }

            foreach (FSMTransition fsmt in transitions)
            {
                if (fsmt.Check())
                {
                    if (debugState)
                        Debug.Log("Transition: " + name + " passed.");
                    return fsmt;
                }
                else
                {
                    if (debugState)
                        Debug.Log("Transition: " + name + " failed.");
                }
            }

            return null;
        }

        public bool EnterState()
        {
            if (doEnter == null)
                return true;

            return doEnter();
        }

        public void UpdateStateFixed()
        {
            if (changedThisFrame)
                return;

            if (doFixedUpdate != null && fsmStateType == FSMStateType.IN_STATE)
                doFixedUpdate();
        }

        public void UpdateStateLate()
        {
            if (changedThisFrame)
                return;

            if (doLateUpdate != null && fsmStateType == FSMStateType.IN_STATE)
                doLateUpdate();
        }

        public FSMTransition UpdateState()
        {
            changedThisFrame = false;
            if (fsmTransitionCheck == FSMTransitionCheck.FIRST)
            {
                FSMTransition fsmt = CheckTransitions();
                if (fsmt != null)
                {
                    return fsmt;
                }

                if (doUpdate != null)
                    doUpdate();
                return null;
            }
            else
            {
                if (doUpdate != null)
                    doUpdate();
                return CheckTransitions();
            }
        }

        public bool ExitState()
        {
            if (doExit == null)
                return true;

            return doExit();
        }

        public override string ToString()
        {
            return base.ToString() + ": " + name;
        }
    }

    public enum FSMTransitionCheck
    {
        FIRST,
        LAST,
    }

    public enum FSMStateType
    {
        ENTER_STATE,
        IN_STATE,
        EXIT_STATE,
    }
}
