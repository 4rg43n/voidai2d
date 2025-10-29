using System;
using UnityEngine;

namespace VoidAI.GenAI.Agent
{
    public class PlayerData : AgentData
    {
        public string playerAge = "25";
        public string playerGender = "Male";
        public string playerHeight = "6 feet";

        public string playerAppearance = "Long brown hair, green eyes, athletic build";

        public override BaseData Clone()
        {
            return new PlayerData
            {
                dataId = this.dataId,
                dataName = this.dataName,
                agentRole = this.agentRole,
                playerAge=this.playerAge,
                playerGender=this.playerGender,
                playerHeight=this.playerHeight,
            };
        }
    }
}


