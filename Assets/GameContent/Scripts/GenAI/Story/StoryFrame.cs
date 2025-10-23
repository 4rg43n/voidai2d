using System;
using System.Collections.Generic;
using UnityEngine;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Story
{
    [Serializable]
    public class StoryFrame
    {
        public PlayerData PlayerData;
        public LocationData LocationData;
        public List<MemoryEntry> memories = new();
    }
}


