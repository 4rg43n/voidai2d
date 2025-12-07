using System;
using System.Collections.Generic;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Story
{
    [System.Serializable]
    public class StoryFrame
    {
        public string Id;

        public PlayerData PlayerData;
        public LocationData LocationData;
        public List<CharacterData> CharacterData = new();
        public List<MemoryEntry> Memories = new();

        public StoryMessageLLM StoryResponse;

        public StoryFrame()
        {
            Id = Guid.NewGuid().ToString();
        }

        public static StoryFrame CreateEmptyFrame(bool createId)
        {
            StoryFrame newStoryFrame = new StoryFrame();
            if (createId)
                newStoryFrame.Id = Guid.NewGuid().ToString();
            else
                newStoryFrame.Id = string.Empty;

            return newStoryFrame;
        }

        public void AddMemory(string input)
        {
            Memories.Add(new MemoryEntry() { Full = input, Summary = input, });
        }

        public static StoryFrame CreateInitialFrame(StoryContext storyContext)
        {
            StoryFrame frame = new StoryFrame();

            frame.PlayerData = storyContext.PlayerData.Clone() as PlayerData;
            frame.LocationData = storyContext.InitialLocation.Clone() as LocationData;

            if (storyContext.AllCharacters != null && storyContext.AllCharacters.Length > 0)
                for (int i = 0; i < storyContext.AllCharacters.Length; i++)
                    frame.CharacterData.Add(storyContext.AllCharacters[i].Clone() as CharacterData);

            return frame;
        }

        public static StoryFrame DuplicateFrame(StoryFrame storyFrame)
        {
            return storyFrame.Clone();
        }

        public StoryFrame Clone()
        {
            List<MemoryEntry> clonedMemories = new List<MemoryEntry>();
            foreach (var memory in Memories)
            {
                clonedMemories.Add(MemoryEntry.Clone(memory));
            }

            List<CharacterData> clonedCharacterData = new List<CharacterData>();
            foreach (var character in CharacterData)
            {
                clonedCharacterData.Add((CharacterData)character.Clone());
            }

            StoryFrame newStoryFrame = new StoryFrame
            {
                PlayerData = PlayerData != null ? (PlayerData)PlayerData.Clone() : null,
                LocationData = LocationData != null ? (LocationData)LocationData.Clone() : null,
                CharacterData = CharacterData != null ? clonedCharacterData : null,
                Memories = clonedMemories,
                StoryResponse = StoryResponse.Clone(),
            };

            return newStoryFrame;
        }
    }
}


