using System;
using System.Collections.Generic;

namespace VoidAI.GenAI.Agent
{

    [Serializable]
    public class CharacterData : AgentData
    {
        public string characterAge = "25";
        public string characterGender = "Female";
        public string characterHeight = "5 feet 7 inches";

        public string characterAppearance = "Long brown hair, green eyes, athletic build";
        public string characterRelationship = "Friendly and supportive towards others";
        public string characterPersonality = "Optimistic, curious, and compassionate";

        public string characterLikes = "Reading, hiking, and cooking";
        public string characterDislikes = "Loud noises and crowds";
        public string characterSpecialTraits = "Highly empathetic and a quick learner";
        public string characterGoals = "To help others and continuously improve herself";

        public override void LoadFromResourcePath(List<string[]> parsedData, string userName)
        {
            base.LoadFromResourcePath(parsedData, userName);

            foreach (var entry in parsedData)
            {
                switch (entry[0])
                {
                    case "characterAge":
                        characterAge = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterAge;
                        break;
                    case "characterGender":
                        characterGender = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterGender;
                        break;
                    case "characterHeight":
                        characterHeight = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterHeight;
                        break;

                    case "characterAppearance":
                        characterAppearance = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterAppearance;
                        break;
                    case "characterRelationship":
                        characterRelationship = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterRelationship;
                        break;
                    case "characterPersonality":
                        characterPersonality = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterPersonality;
                        break;

                    case "characterLikes":
                        characterLikes = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterLikes;
                        break;
                    case "characterDislikes":
                        characterDislikes = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterDislikes;
                        break;
                    case "characterSpecialTraits":
                        characterSpecialTraits = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterSpecialTraits;
                        break;
                    case "characterGoals":
                        characterGoals = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : characterGoals;
                        break;
                }
            }
        }
    }
}


