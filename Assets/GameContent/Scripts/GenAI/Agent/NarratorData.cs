using System;
using UnityEngine;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Agent
{
    public class NarratorData : AgentData
    {
        public override BaseData Clone()
        {
            return new NarratorData
            {
                dataId = this.dataId,
                dataName = this.dataName,
                agentRole = this.agentRole
            };
        }
    }
}

