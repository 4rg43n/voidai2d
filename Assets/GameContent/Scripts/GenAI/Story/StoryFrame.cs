using System;
using System.Collections.Generic;
using UnityEngine;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Story
{
    public class StoryFrame
    {
        public PlayerData PlayerData;
        public LocationData LocationData;
        public CharacterData CharacterData;
        public List<MemoryEntry> Memories = new();

        public StoryMessageLLM StoryResponse;

        public void AddMemory(string input)
        {
            Memories.Add(new MemoryEntry() { Full=input, Summary=input, });
        }

        public void Init(StoryContext storyContext)
        {
            PlayerData = storyContext.PlayerData.Clone() as PlayerData;
            LocationData = storyContext.InitialLocation;
            CharacterData = storyContext.AllCharacters[0];
        }

        public StoryFrame Clone()
        {
            List<MemoryEntry> clonedMemories = new List<MemoryEntry>();
            foreach (var memory in Memories)
            {
                clonedMemories.Add(MemoryEntry.Clone(memory));
            }
            StoryFrame newStoryFrame = new StoryFrame
            {
                PlayerData = PlayerData != null ? (PlayerData)PlayerData.Clone() : null,
                LocationData = LocationData != null ? (LocationData)LocationData.Clone() : null,
                CharacterData = CharacterData != null ? (CharacterData)CharacterData.Clone() : null,
                Memories = clonedMemories,
                StoryResponse=StoryResponse.Clone(),
            };

            return newStoryFrame;
        }
    }
}


