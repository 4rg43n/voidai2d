using System;
using System.Data;
using TreeEditor;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEditor.PackageManager.UI;
using UnityEditor.Sprites;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LookDev;
using UnityEngine.TextCore.Text;
using UnityEngine.UIElements;
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

        static string BuildCharacterNameList(StoryContext storyContext)
        {
            var sb = new System.Text.StringBuilder();

            CharacterData[] characters = new CharacterData[]
            {
                storyContext.CurrentFrame.CharacterData
            };

            for(int i = 0;i < characters.Length; i++)
            {
                var c = characters[i];
                sb.Append(c.dataName);
                if (i < characters.Length - 1)
                    sb.Append(", ");
            }


            return sb.ToString();
        }

        static string BuildCharacterDescriptionList(StoryContext storyContext)
        {
            var sb = new System.Text.StringBuilder();

            CharacterData[] characters = new CharacterData[] 
            { 
                storyContext.CurrentFrame.CharacterData 
            };

            foreach (var c in characters)
            {
                //sb.AppendLine($"- {c.dataName}");
                sb.AppendLine(BuildCharacterDescription(c, storyContext));
            }

            return sb.ToString();
        }

        static string BuildCharacterDescription(CharacterData characterData, StoryContext storyContext)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine($"    - {characterData.dataName}");
            sb.AppendLine($"        * Age: {characterData.characterAge}");
            sb.AppendLine($"        * Height: {characterData.characterHeight}");
            sb.AppendLine($"        * Appearance: {characterData.characterAppearance}");
            sb.AppendLine($"        * Relationship to {storyContext.CurrentFrame.PlayerData.dataName}: {characterData.dataDescription}");
            sb.AppendLine($"        * Likes: {characterData.characterLikes}");
            sb.AppendLine($"        * Dislikes: {characterData.characterDislikes}");
            sb.AppendLine($"        * Special Traits: {characterData.characterSpecialTraits}");
            sb.AppendLine($"        * Goals: {characterData.characterGoals}");

            return sb.ToString();
        }

        static string BuildPrompt_Narration(string input, StoryContext storyContext)
        {
            var player = storyContext.CurrentFrame.PlayerData.dataName;
            var location = storyContext.CurrentFrame.LocationData.dataDescription;
            var sb = new System.Text.StringBuilder();

            // Identity & scope
            sb.AppendLine($"You are the narrator. The world is {storyContext.Tone}.");
            sb.AppendLine("You are not an AI assistant, chatbot, or language model.");
            sb.AppendLine("You are a neutral third-person narrator describing what the player sees.");
            sb.AppendLine("Never refer to yourself or your role as an assistant or AI.");
            sb.AppendLine();

            // Scene state
            sb.AppendLine($"The {player} is currently in: {location}");
            sb.AppendLine();

            // Characters (verbatim, so the model knows exactly who to output)
            sb.AppendLine("Characters Present (output one <CHARACTER> block for each, in this order):");
            sb.AppendLine(BuildCharacterDescriptionList(storyContext));
            sb.AppendLine();

            // Narration rules
            sb.AppendLine("Narration Guidelines:");
            sb.AppendLine("  * ONLY respond to content between the <INPUT> tags.");
            sb.AppendLine("  * Maintain an immersive, neutral tone.");
            sb.AppendLine("  * Do NOT include dialogue, inner thoughts, or mind-reading.");
            sb.AppendLine("  * Do NOT mention or reference the LLM, model, assistant, or yourself.");
            sb.AppendLine();

            // Output structure
            sb.AppendLine("Output Structure (exactly this, nothing else):");
            sb.AppendLine("  <LOCATION>…</LOCATION>");
            sb.AppendLine("  <CHARACTER name='…'>…</CHARACTER>  (one per character listed above, in order)");
            sb.AppendLine("  <END>");
            sb.AppendLine();

            // LOCATION rules with labeled lines
            sb.AppendLine("LOCATION Rules:");
            sb.AppendLine("  * Inside <LOCATION>…</LOCATION> output EXACTLY these 4 lines, one per line:");
            sb.AppendLine("    [1] <one sentence in 2nd person describing the environment>");
            sb.AppendLine("    [2] <one sentence in 2nd person describing the environment>");
            sb.AppendLine("    [3] <one sentence in 2nd person describing the environment>");
            sb.AppendLine("    [4] <one sentence in 2nd person describing the environment>");
            sb.AppendLine("  * Do NOT mention or refer to any characters in LOCATION (no names, titles, or pronouns).");
            sb.AppendLine("  * No blank lines inside <LOCATION>. No extra lines beyond [1]-[4].");
            sb.AppendLine();

            // Forbidden character references inside LOCATION (hard ban)
            string characterNames = BuildCharacterNameList(storyContext);
            sb.AppendLine("Forbidden in LOCATION:");
            if (!string.IsNullOrWhiteSpace(characterNames))
                sb.AppendLine($"  * Any of these names or references: {characterNames}"); // append others if present
            sb.AppendLine("  * Any third-person pronoun referring to characters (he, she, they, her, him, them).");
            sb.AppendLine();

            // CHARACTER rules (single source of truth)
            // CHARACTER rules (use [1]/[2] consistently)
            sb.AppendLine("CHARACTER Rules (very important):");
            sb.AppendLine("  * For EACH character, inside <CHARACTER name='...'>…</CHARACTER> output EXACTLY these 2 lines:");
            sb.AppendLine("    [1] One sentence describing the character’s physical snapshot — appearance, posture, or clothing. No inner thoughts.");
            sb.AppendLine($"    [2] One short, observable behavior directed TOWARD {player}, starting with the character’s name.");
            sb.AppendLine("  * Each line must end with a period. No blank lines. No extra lines beyond [1] and [2].");
            sb.AppendLine("  * Do NOT use placeholders or ellipses: '...', '…', 'N/A'.");
            sb.AppendLine("  * Use strictly observable cues (distance, gaze, body tension).");
            sb.AppendLine("  * Do NOT use mind-reading words: feels, thinks, wants, hopes, longs, desires, yearns, loves, hates.");
            sb.AppendLine("  * No quoted dialogue.");
            sb.AppendLine();

            // --- Structure example, no parentheses ---
            sb.AppendLine("Example Output (STRUCTURE ONLY — do not copy wording):");
            sb.AppendLine("<LOCATION>");
            sb.AppendLine("[1] {sentence}");
            sb.AppendLine("[2] {sentence}");
            sb.AppendLine("[3] {sentence}");
            sb.AppendLine("[4] {sentence}");
            sb.AppendLine("</LOCATION>");
            sb.AppendLine("<CHARACTER name='{Name}'>");
            sb.AppendLine("[1] {One sentence describing physical appearance, posture, or clothing.}");
            sb.AppendLine($"[2] {{One short, observable behavior toward {player}, starting with the character’s name.}}");
            sb.AppendLine("</CHARACTER>");
            sb.AppendLine("<END>");
            sb.AppendLine();

            // Originality + format constraints
            sb.AppendLine("Originality Rules:");
            sb.AppendLine("  * Do NOT reuse exact phrases from examples; examples indicate structure only.");
            sb.AppendLine("  * Vary sentence openings; avoid starting two sentences with the same two-word phrase.");
            sb.AppendLine("  * Include at least one sensory detail (sound, motion, texture, temperature, light).");
            sb.AppendLine();

            sb.AppendLine("Hard Format Constraints:");
            sb.AppendLine("  * The FIRST non-whitespace characters MUST be exactly \"<LOCATION>\".");
            sb.AppendLine("  * Tag order MUST be: <LOCATION>…</LOCATION> then <CHARACTER…>…</CHARACTER> (one per listed character), then <END>.");
            sb.AppendLine("  * Do NOT write any text before <LOCATION> or after <END>.");
            sb.AppendLine("  * Placeholders are forbidden anywhere: '...', '…', 'N/A'.");
            sb.AppendLine();

            // Response bounds & input
            sb.AppendLine("Response Bounds:");
            sb.AppendLine($"  * The text between <INPUT> and </INPUT> is {player}’s latest action or observation.");
            sb.AppendLine("  * After writing <END>, STOP. Do not add anything after <END>.");
            sb.AppendLine();
            sb.AppendLine("<INPUT>");
            sb.AppendLine(input);
            sb.AppendLine("</INPUT>");
            sb.AppendLine();

            return sb.ToString();
        }

        //static string BuildPrompt_Narration(string input, StoryContext storyContext)
        //{
        //    var sb = new System.Text.StringBuilder();
        //    sb.AppendLine($"You are the narrator. The world is {storyContext.Tone}.");

        //    sb.AppendLine("You are not an AI assistant, chatbot, or language model.");
        //    sb.AppendLine("You are a neutral third - person narrator describing what the player sees.");
        //    sb.AppendLine("Never refer to yourself or your role as an assistant or AI.");

        //    sb.AppendLine();

        //    sb.AppendLine($"The {storyContext.CurrentFrame.PlayerData.dataName} is currently in: {storyContext.CurrentFrame.LocationData.dataDescription}");
        //    sb.AppendLine();

        //    //sb.AppendLine(BuildRecentMemory(context));
        //    //sb.AppendLine(BuildCurrentFacts(context));
        //    sb.AppendLine(BuildCharacterDescriptions(storyContext));

        //    sb.AppendLine($"Narration Guidelines:");
        //    sb.AppendLine("    * ONLY respond to content between the <INPUT> tags.");
        //    sb.AppendLine($"    * There are no AI companions, avatars, or artificial entities present in this world unless explicitly described.");
        //    sb.AppendLine($"    * Do NOT mention or reference yourself, the LLM, the model, or any abstract narrator entity outside the story.");
        //    sb.AppendLine($"    * You are simply the narrator, describing what the player sees — do not insert yourself or commentary.");
        //    sb.AppendLine("    * Maintain an immersive, neutral tone.");

        //    sb.AppendLine();

        //    sb.AppendLine("Output Format Guidelines:");
        //    sb.AppendLine("    * ALWAYS include exactly one description of the scene without any characters and tag it with <LOCATION>");
        //    sb.AppendLine("    * ALWAYS include character descriptions tagged with <CHARACTER name='character_name'> for each character in the scene.");
        //    sb.AppendLine("    * ALWAYS end the output with a single <END> tag.");

        //    sb.AppendLine();

        //    sb.AppendLine("Location Tag Rules (very important):");
        //    sb.AppendLine($"    * Describe what {storyContext.CurrentFrame.PlayerData.dataName} sees in 1–2 short cinematic sentences, using second-person perspective.");
        //    sb.AppendLine("    * Begin the description with 'You...' or use 'you see...' where appropriate.");
        //    sb.AppendLine("    * Mention the environment and any characters present by name.");
        //    sb.AppendLine("    * For EACH character in the scene, output EXACTLY TWO sentences inside <CHARACTER name='...'>…</CHARACTER>:");
        //    sb.AppendLine("      1) Physical snapshot: appearance / posture / clothing (no thoughts).");
        //    sb.AppendLine($"      2) One short observable behavior TOWARD {storyContext.CurrentFrame.PlayerData.dataName},");
        //    sb.AppendLine("         starting with the character’s name (e.g., 'Rin stands apart from Raven, keeping her distance.').");
        //    sb.AppendLine("    * Use strictly observable cues for attitude (e.g., keeps distance, gaze lingers, jaw tightens, fists clenched).");
        //    sb.AppendLine("    * Do NOT use mind-reading words (feels, thinks, wants, hopes, longs, desires, yearns).");
        //    sb.AppendLine();

        //    sb.AppendLine("Character Tag Rules (very important):");
        //    sb.AppendLine("    * Use the character's name when describing them (e.g., 'Zara stands near the flowers').");
        //    sb.AppendLine("    * For EACH character in the scene, output EXACTLY TWO sentences inside <CHARACTER name='...'>…</CHARACTER>:");
        //    sb.AppendLine("    * Avoid quoting speech or assuming relationships.");
        //    sb.AppendLine("    * Do NOT speak from the perspective of any character. Avoid inner thoughts or dialogue.");
        //    sb.AppendLine("    * Only describe outward appearances, posture, and actions. Do NOT describe internal thoughts or emotions.");
        //    sb.AppendLine("      1) Physical snapshot: appearance / posture / clothing (no thoughts).");
        //    sb.AppendLine($"      2) One short observable behavior TOWARD {storyContext.CurrentFrame.PlayerData.dataName},");
        //    sb.AppendLine("         starting with the character’s name (e.g., 'Rin stands apart from Raven, keeping her distance.').");
        //    sb.AppendLine("    * Use strictly observable cues for attitude (e.g., keeps distance, gaze lingers, jaw tightens, fists clenched).");
        //    sb.AppendLine("    * Do NOT use mind-reading words (feels, thinks, wants, hopes, longs, desires, yearns).");

        //    sb.AppendLine();

        //    sb.AppendLine("Example Output:");
        //    sb.AppendLine("<LOCATION>You find yourself in a sunlit meadow, the grass swaying gently in the breeze under a clear blue sky.</LOCATION>");
        //    sb.AppendLine("<CHARACTER name='Zara'>Zara stands near a cluster of wildflowers, her auburn hair catching the sunlight as she gazes into the distance.</CHARACTER>");
        //    sb.AppendLine("<END>");
        //    sb.AppendLine();

        //    sb.AppendLine("Response Guidelines:");
        //    sb.AppendLine($"    * The text between <INPUT> and </INPUT> represents {storyContext.CurrentFrame.PlayerData.dataName}’s latest action or observation.");
        //    sb.AppendLine($"    * Respond as the narrator describing {storyContext.CurrentFrame.PlayerData.dataName}’s point of view.");
        //    sb.AppendLine("    * When you finish writing the final <END> tag, STOP IMMEDIATELY.");
        //    sb.AppendLine("    * Do not add anything after <END>.");
        //    sb.AppendLine();
        //    sb.AppendLine();
        //    sb.AppendLine("<INPUT>");
        //    sb.AppendLine(input);
        //    sb.AppendLine("</INPUT>");
        //    sb.AppendLine();

        //    return sb.ToString();
        //}
    }
}
