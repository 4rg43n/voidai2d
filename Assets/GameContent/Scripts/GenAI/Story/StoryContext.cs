using System;
using System.Collections.Generic;
using UnityEngine;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Story
{
    public class StoryContext
    {
        public string Title = "The Great Adventure";
        public string Tone = "modern";


        // global story information
        public string Introduction = "This is a story about a brave adventurer.";                 // displayed in user list
        public string[] Keywords = new string[] { "adventure", "heroism", "fantasy" };            // used for search

        public string locationResourcePath = "Locations/DefaultLocation";                         // resource path to load initial location data
        public LocationData InitialLocation;                                                    // starting location of the story


        public string[] charactersResourcePath = new string[] { "Characters/DefaultCharacters" }; // resource path to load initial character data
        public CharacterData[] AllCharacters = new CharacterData[] { };                         // characters in the story

        public string Scenario = "An epic quest in a mystical land.";                           // overall scenario of the story, descript roles and relationships for this story
        public string FirstMessage = "The journey begins.";                                     // first message to start the story. Added as history to the agents for when story starts
        public string DialogueStyle = "";                                                       // rules for how the LLM returns dialogue

        public PlayerData PlayerData;
        public NarratorData narrator;
        public List<StoryFrame> storyFrames = new();

        public StoryFrame CurrentFrame { get { return storyFrames[storyFrames.Count - 1]; } }

        public StoryContext()
        {
            storyFrames.Add(new StoryFrame());
        }

        public void LoadFromResourcePath(List<string[]> parsedData, string userName)
        {

            foreach (var entry in parsedData)
            {
                switch (entry[0])
                {
                    case "Title":
                        Title = entry.Length > 1 ? BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName) : Title;
                        break;
                    case "Tone":
                        Tone = entry.Length > 1 ? BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName) : Tone;
                        break;
                    case "Introduction":
                        Introduction = entry.Length > 1 ? BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName) : Introduction;
                        break;
                    case "Keywords":
                        if (entry[1].Length > 1)
                        {
                            Keywords = BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName).Split(',');
                        }
                        break;
                    case "locationResourcePath":
                        locationResourcePath = entry.Length > 1 ? BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName) : locationResourcePath;
                        break;
                    case "charactersResourcePath":
                        if (entry[1].Length > 1)
                        {
                            charactersResourcePath = BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName).Split(',');
                        }
                        break;
                    case "Scenario":
                        Scenario = entry.Length > 1 ? BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName) : Scenario;
                        break;
                    case "FirstMessage":
                        FirstMessage = entry.Length > 1 ? BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName) : FirstMessage;
                        break;
                    case "DialogueStyle":
                        DialogueStyle = entry.Length > 1 ? BaseData.ReplaceAll(entry[1], BaseData.user_descriptor, userName) : DialogueStyle;
                        break;
                }
            }

            if (!string.IsNullOrEmpty(locationResourcePath))
            {
                InitialLocation = DataUtils.LoadDataFromResources<LocationData>(locationResourcePath, userName);
            }

            if (charactersResourcePath != null && charactersResourcePath.Length > 0)
            {
                List<CharacterData> charactersList = new List<CharacterData>();
                foreach (var charPath in charactersResourcePath)
                {
                    CharacterData character = DataUtils.LoadDataFromResources<CharacterData>(charPath, userName);
                    charactersList.Add(character);
                }
                AllCharacters = charactersList.ToArray();
            }

            PlayerData = new PlayerData() { dataName = userName };
            narrator = new NarratorData() { agentRole = "narrator", dataName = "Narrator" };

            storyFrames.Clear();
            AddNewFrame();
        }

        public void AddNewFrame()
        {
            storyFrames.Add(new StoryFrame());
            CurrentFrame.Init(this);
        }
    }
}


