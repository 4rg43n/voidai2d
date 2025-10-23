using System;
using System.Collections.Generic;
using UnityEngine;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Story
{
    [Serializable]
    public class StoryContext
    {
        public string Tone = "modern";

        public List<StoryFrame> storyFrames = new();

        public StoryFrame CurrentFrame { get { return storyFrames[storyFrames.Count - 1]; } }

        public StoryContext()
        {
            storyFrames.Add(new StoryFrame());
        }
    }
}


