using System;
using System.Data;
using TreeEditor;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;
using VoidAI.GenAI.Agent;
using VoidAI.GenAI.Story;
using static Unity.VisualScripting.Icons;
using static UnityEngine.EventSystems.EventTrigger;

namespace VoidAI.GenAI.Story
{
    public enum PromptType
    {
        Narration,
        CharacterDialogue,
        CharacterThought,
        WorldEvent,
    }

    public static class PromptUtils
    {
        public static string BuildPrompt(string input, PromptType promptType, StoryContext storyContext)
        {
            switch (promptType)
            {
                case PromptType.Narration:
                    return BuildPrompt_Narration(input, storyContext);
                //case PromptType.CharacterDialogue:
                //    return BuildPrompt_CharacterDialogue(input);
                //case PromptType.CharacterThought:
                //    return BuildPrompt_CharacterThought(input);
                //case PromptType.WorldEvent:
                //    return BuildPrompt_WorldEvent(input);
                default:
                    Debug.LogError($"Unsupported PromptType: {promptType}");
                    return "...";
            }
        }

        static string BuildCharacterDescriptions(StoryContext storyContext)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("Current Characters in the Scene:");
            sb.AppendLine(BuildCharacterDescription(storyContext.CurrentFrame.CharacterData));

            return sb.ToString();
        }

        static string BuildCharacterDescription(CharacterData characterData)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"    Character Name: {characterData.dataName}");
            sb.AppendLine($"        * Age: {characterData.characterAge}");
            sb.AppendLine($"        * Height: {characterData.characterHeight}");
            sb.AppendLine($"        * Appearance: {characterData.dataDescription}");
            sb.AppendLine($"        * Description: {characterData.dataDescription}");
            sb.AppendLine($"        * Likes: {characterData.characterLikes}");
            sb.AppendLine($"        * Dislikes: {characterData.characterDislikes}");
            sb.AppendLine($"        * Special Traits: {characterData.characterSpecialTraits}");
            sb.AppendLine($"        * Goals: {characterData.characterGoals}");

            return sb.ToString();
        }

        static string BuildPrompt_Narration(string input, StoryContext storyContext)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"You are the narrator. The world is {storyContext.Tone}.");

            sb.AppendLine("You are not an AI assistant, chatbot, or language model.");
            sb.AppendLine("You are a neutral third - person narrator describing what the player sees.");
            sb.AppendLine("Never refer to yourself or your role as an assistant or AI.");

            sb.AppendLine();


            sb.AppendLine($"The {storyContext.CurrentFrame.PlayerData.dataName} is currently in: {storyContext.CurrentFrame.LocationData.dataDescription}");
            sb.AppendLine();

            //sb.AppendLine(BuildRecentMemory(context));
            //sb.AppendLine(BuildCurrentFacts(context));
            sb.AppendLine(BuildCharacterDescriptions(storyContext));
            //sb.AppendLine();

            sb.AppendLine($"Narration Guidelines:");
            sb.AppendLine($"    * There are no AI companions, avatars, or artificial entities present in this world unless explicitly described.");
            sb.AppendLine($"    * Do NOT mention or reference yourself, the LLM, the model, or any abstract narrator entity outside the story.");
            sb.AppendLine($"    * You are simply the narrator, describing what the player sees — do not insert yourself or commentary.");

            sb.AppendLine();

            sb.AppendLine($"Output Guidelines:");
            sb.AppendLine("    * ONLY respond to content between the <INPUT> tags.");
            sb.AppendLine($"    * Describe what {storyContext.CurrentFrame.PlayerData.dataName} sees in 1–2 short cinematic sentences, using second-person perspective.");
            sb.AppendLine("    * Begin the description with 'You...' or use 'you see...' where appropriate.");
            sb.AppendLine("    * Mention the environment and any characters present by name.");
            sb.AppendLine("    * Use the character's name when describing them (e.g., 'Zara stands near the flowers').");
            sb.AppendLine("    * Only describe outward appearances, posture, and actions. Do NOT describe internal thoughts or emotions.");
            sb.AppendLine("    * Avoid quoting speech or assuming relationships.");
            sb.AppendLine("    * Maintain an immersive, neutral tone.");
            sb.AppendLine("    * Do NOT speak from the perspective of any character. Avoid inner thoughts or dialogue.");
            sb.AppendLine();

            sb.AppendLine("Output Format Guidelines:");
            sb.AppendLine("    * ALWAYS include exactly one description of the scene without any characters and prefix it with <LOCATION>");
            sb.AppendLine("    * ALWAYS include character descriptions prefixed with <CHARACTER name='character_name'> for each character in the scene.");
            //sb.AppendLine("    * Do NOT include any dialogue or thoughts.");

            sb.AppendLine();

            sb.AppendLine("Example Output:");
            sb.AppendLine("<LOCATION>You find yourself in a sunlit meadow, the grass swaying gently in the breeze under a clear blue sky.</LOCATION>");
            sb.AppendLine("<CHARACTER name='Zara'>Zara stands near a cluster of wildflowers, her auburn hair catching the sunlight as she gazes into the distance.</CHARACTER>");
            sb.AppendLine();

            //sb.AppendLine("After the description, include a single line like this:");
            //sb.AppendLine("<thought>[a poetic narrator-style summary of what’s happening or what the moment means]</thought>");
            //sb.AppendLine("This is not a character thought. It is a narrative reflection, as if closing the scene.");
            //sb.AppendLine("Examples:");
            //sb.AppendLine("<thought>You and Zara talk long into the night.</thought>");
            //sb.AppendLine("<thought>You were spoiling for a fight. Now you've got one.</thought>");
            //sb.AppendLine("<thought>You drift off to sleep hoping the nightmares won't find you.</thought>");
            //sb.AppendLine();

            sb.AppendLine("The text between <INPUT> and </INPUT> represents Raven’s latest action or observation.");
            sb.AppendLine("Respond as the narrator describing Raven’s point of view.");
            sb.AppendLine("When you finish writing the final </CHARACTER> tag, STOP IMMEDIATELY.");
            sb.AppendLine("Do not add anything after </CHARACTER>.");
            sb.AppendLine();
            sb.AppendLine("<INPUT>");
            sb.AppendLine(input);
            sb.AppendLine("</INPUT>");
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
