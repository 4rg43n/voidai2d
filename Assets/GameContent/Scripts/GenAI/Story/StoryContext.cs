using System;
using UnityEngine;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Story
{
    [Serializable]
    public class StoryContext
    {
        public PlayerData PlayerData;
        public string Tone = "modern";

        public LocationData LocationData;
    }
}


