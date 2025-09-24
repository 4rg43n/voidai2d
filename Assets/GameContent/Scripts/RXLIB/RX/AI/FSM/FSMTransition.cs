using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI.FSM
{
    public class FSMTransition
    {
        string name = "default transition";

        public delegate bool CheckTransition();

        public FSMState dest;
        public CheckTransition doCheck;

        public FSMTransition(string transName, CheckTransition checkt, FSMState state)
        {
            name = transName;
            doCheck = checkt;
            dest = state;
        }

        public bool Check()
        {
            if (doCheck())
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            return base.ToString() + ": " + name;
        }
    }
}
