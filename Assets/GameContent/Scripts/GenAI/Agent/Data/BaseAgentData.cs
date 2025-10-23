using System;
using UnityEngine;

namespace VoidAI.GenAI.Agent.Data
{
    public class BaseAgentData
    {
        public string agentID = "";
        public string agentName = "Agent";
        public string agentRole = "A helpful assistant.";

        public BaseAgentData()
        {
            Guid.NewGuid().ToString();
        }
    }
}


