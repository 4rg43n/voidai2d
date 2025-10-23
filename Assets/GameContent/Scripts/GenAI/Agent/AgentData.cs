using System;
using UnityEngine;

namespace VoidAI.GenAI.Agent
{

    [Serializable]
    public abstract class AgentData : BaseData
    {
        public string agentRole = "A helpful assistant.";
    }
}

