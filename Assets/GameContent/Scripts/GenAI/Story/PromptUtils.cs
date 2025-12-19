using UnityEngine;
using VoidAI.GenAI.Agent;

namespace VoidAI.GenAI.Story
{
    public enum PromptType
    {
        Narration,
        Character,
    }

    public static class PromptUtils
    {
        public static string BuildPrompt(string input, PromptType promptType, StoryContext storyContext)
        {
            switch (promptType)
            {
                case PromptType.Narration:
                    return BuildPrompt_Narration(input, storyContext);
                case PromptType.Character:
                    return BuildPrompt_Character(input, storyContext);
                //case PromptType.CharacterThought:
                //    return BuildPrompt_CharacterThought(input);
                //case PromptType.WorldEvent:
                //    return BuildPrompt_WorldEvent(input);
                default:
                    Debug.LogError($"Unsupported PromptType: {promptType}");
                    return "...";
            }
        }

        static string BuildPrompt_Character(string input, StoryContext storyContext)
        {
            var player = storyContext.CurrentFrame.PlayerData.dataName;
            var character = storyContext.CurrentFrame.CharacterData[0];
            var location = storyContext.CurrentFrame.LocationData.locationDescription;

            var sb = new System.Text.StringBuilder();

            // Identity & context
            sb.AppendLine($"You are {character.dataName}.");
            sb.AppendLine($"{character.dataName} is {character.characterAge} years old, {character.characterHeight} tall.");
            sb.AppendLine($"{character.dataName}’s appearance: {character.characterAppearance}");
            sb.AppendLine($"{character.dataName}’s relationship to {player}: {character.characterRelationship}");
            sb.AppendLine($"{character.dataName}’s personality: {character.characterPersonality}");
            sb.AppendLine();
            sb.AppendLine($"The world is {storyContext.Tone}.");
            sb.AppendLine($"You are currently in: {location}");
            sb.AppendLine();

            sb.AppendLine($"The player is {player}.");
            sb.AppendLine($"    - Age: {storyContext.PlayerData.dataName}.");
            sb.AppendLine($"    - Gender: {storyContext.PlayerData.playerGender}.");
            sb.AppendLine($"    - Height: {storyContext.PlayerData.playerHeight}.");
            sb.AppendLine($"    - Appearance: {storyContext.PlayerData.playerAppearance}.");
            sb.AppendLine();

            // Scenario information
            sb.AppendLine("Scenario Information:");
            sb.AppendLine($"    {storyContext.Scenario}");
            sb.AppendLine();

            // Memories
            string memories = BuildMemories(storyContext);
            if (!string.IsNullOrWhiteSpace(memories))
            {
                sb.AppendLine(memories);
            }

            // Character constraints
            sb.AppendLine("Character Guidelines:");
            sb.AppendLine("  * Write only what the player can observe or hear from you.");
            sb.AppendLine("  * Maintain natural tone, consistent with your personality.");
            sb.AppendLine("  * Do NOT include the narrator, system messages, or meta commentary.");
            sb.AppendLine("  * Do NOT reference the LLM, model, or assistant.");
            sb.AppendLine("  * Do NOT repeat the player's input unless it is being directly echoed as part of natural dialogue.");
            sb.AppendLine();

            // Output structure
            sb.AppendLine("Output Structure (exactly this, nothing else):");
            sb.AppendLine("  <THOUGHT>…</THOUGHT>   (your private, unspoken thoughts — keep brief, consistent with personality)");
            sb.AppendLine("  <ACTION>…</ACTION>     (a visible action or physical response the player can observe)");
            sb.AppendLine("  <DIALOGUE>…</DIALOGUE> (spoken line, if you choose to speak — or leave empty if silent)");
            sb.AppendLine("  <END>");
            sb.AppendLine();

            // Tag content rules
            sb.AppendLine("THOUGHT Rules:");
            sb.AppendLine("  * Write 1–2 short sentences capturing your immediate emotional or mental reaction.");
            sb.AppendLine("  * Keep thoughts private — do NOT address the player directly in <THOUGHT>.");
            sb.AppendLine("  * No long inner monologues or exposition.");
            sb.AppendLine();

            sb.AppendLine("ACTION Rules:");
            sb.AppendLine("  * Write 1–2 short sentences describing visible movement, posture, or facial expression.");
            sb.AppendLine("  * Use third person (e.g., 'Rin glances away' or 'Rin’s hand trembles slightly').");
            sb.AppendLine("  * Do NOT include dialogue, thoughts, or interpretation.");
            sb.AppendLine();

            sb.AppendLine("DIALOGUE Rules:");
            sb.AppendLine($"  * If you speak to {player}, write natural, concise dialogue inside <DIALOGUE>…</DIALOGUE>.");
            sb.AppendLine("  * Keep it realistic and in tone with personality.");
            sb.AppendLine("  * Do NOT repeat thoughts or describe actions here.");
            sb.AppendLine("  * May be empty if you stay silent.");
            sb.AppendLine();

            // Example
            sb.AppendLine("Example Output (STRUCTURE ONLY — do not copy wording):");
            sb.AppendLine("<THOUGHT> {private thought, 1–2 sentences} </THOUGHT>");
            sb.AppendLine("<ACTION> {visible physical behavior, 1–2 sentences} </ACTION>");
            sb.AppendLine("<DIALOGUE> {spoken line or empty} </DIALOGUE>");
            sb.AppendLine("<END>");
            sb.AppendLine();

            // Hard format constraints
            sb.AppendLine("Hard Format Constraints:");
            sb.AppendLine("  * The FIRST non-whitespace characters MUST be exactly \"<THOUGHT>\".");
            sb.AppendLine("  * Tag order MUST be <THOUGHT>…</THOUGHT>, then <ACTION>…</ACTION>, then <DIALOGUE>…</DIALOGUE>, then <END>.");
            sb.AppendLine("  * Do NOT add any tags beyond these four.");
            sb.AppendLine("  * Do NOT write anything before <THOUGHT> or after <END>.");
            sb.AppendLine();

            // Response bounds & input
            sb.AppendLine("Response Bounds:");
            sb.AppendLine($"  * The text between <INPUT> and </INPUT> is {player}’s latest action or statement.");
            sb.AppendLine("  * After writing <END>, STOP. Do not add anything after <END>.");
            sb.AppendLine();
            sb.AppendLine("<INPUT>");
            sb.AppendLine(input);
            sb.AppendLine("</INPUT>");
            sb.AppendLine();

            return sb.ToString();
        }

        static string BuildCharacterNameList(StoryContext storyContext)
        {
            var character = storyContext.CurrentFrame.CharacterData[0];

            var sb = new System.Text.StringBuilder();

            CharacterData[] characters = new CharacterData[]
            {
                character
            };

            for (int i = 0; i < characters.Length; i++)
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
            var character = storyContext.CurrentFrame.CharacterData[0];

            var sb = new System.Text.StringBuilder();

            CharacterData[] characters = new CharacterData[]
            {
                character
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
            sb.AppendLine($"        * Relationship to {storyContext.CurrentFrame.PlayerData.dataName}: {characterData.characterRelationship}");
            sb.AppendLine($"        * Personality: {characterData.characterPersonality}");

            sb.AppendLine($"        * Likes: {characterData.characterLikes}");
            sb.AppendLine($"        * Dislikes: {characterData.characterDislikes}");
            sb.AppendLine($"        * Special Traits: {characterData.characterSpecialTraits}");
            sb.AppendLine($"        * Goals: {characterData.characterGoals}");

            return sb.ToString();
        }

        static string BuildMemories(StoryContext storyContext)
        {
            if (storyContext.CurrentFrame.Memories.Count == 0)
                return null;

            var sb = new System.Text.StringBuilder();
            // Memories
            sb.AppendLine("Relevant Memories:");
            foreach (var memory in storyContext.CurrentFrame.Memories)
            {
                sb.AppendLine($"  - {memory.Full}");
            }
            sb.AppendLine();
            return sb.ToString();
        }

        static string BuildPrompt_Narration(string input, StoryContext storyContext)
        {
            var character = storyContext.CurrentFrame.CharacterData[0].dataName;
            var player = storyContext.CurrentFrame.PlayerData.dataName;
            var location = storyContext.CurrentFrame.LocationData.locationDescription;
            var sb = new System.Text.StringBuilder();

            // Identity & scope
            sb.AppendLine($"The world is {storyContext.Tone}.");
            sb.AppendLine($"{character} is the narrator and will write the thoughts, dialogue, " +
                $"and actions of {character} and other characters that may appear in the narrative, " +
                $"except for {player}. {character} AVOIDS writing the thoughts, dialogue, and actions of {player}" +
                $"and focuses on describing the environment and other characters' observable behaviors." +
                $"{character} responses and descriptions are written in third person perspective. " +
                $"{player} descriptions and actions are writen in second person perspective.");
            sb.AppendLine("You are NOT an AI assistant, chatbot, or language model.");

            // guidance


            sb.AppendLine();

            sb.AppendLine($"The player is {player}.");
            sb.AppendLine($"    - Age: {storyContext.PlayerData.dataName}.");
            sb.AppendLine($"    - Gender: {storyContext.PlayerData.playerGender}.");
            sb.AppendLine($"    - Height: {storyContext.PlayerData.playerHeight}.");
            sb.AppendLine($"    - Appearance: {storyContext.PlayerData.playerAppearance}.");
            sb.AppendLine();

            // Scene state
            sb.AppendLine($"The {player} is currently at: {location}");
            sb.AppendLine();

            // Scenario information
            sb.AppendLine("Scenario Information:");
            sb.AppendLine($"    {storyContext.Scenario}");
            sb.AppendLine();

            // Memories
            string memories = BuildMemories(storyContext);
            if (!string.IsNullOrWhiteSpace(memories))
            {
                sb.AppendLine(memories);
            }

            // Characters (verbatim, so the model knows exactly who to output)
            sb.AppendLine("Characters Present (output one <CHARACTER> block for each, in this order):");
            sb.AppendLine(BuildCharacterDescriptionList(storyContext));
            sb.AppendLine();

            // Narration rules
            sb.AppendLine("Narration Guidelines:");
            sb.AppendLine("  * ONLY respond to content between the <INPUT> tags.");
            sb.AppendLine("  * Maintain an immersive, neutral tone.");
            sb.AppendLine("  * Do NOT include inner thoughts, or mind-reading.");
            sb.AppendLine($"  * Only mention the environment, characters or {player}.");
            sb.AppendLine();

            sb.AppendLine();

            sb.AppendLine();

            // Originality + format constraints
            sb.AppendLine("Originality Rules:");
            sb.AppendLine("  * Do NOT reuse exact phrases from examples; examples indicate structure only.");
            sb.AppendLine("  * Avoid repeating phrases that appear in the history or input.");
            sb.AppendLine("  * Vary sentence openings; avoid starting two sentences with the same two-word phrase.");
            sb.AppendLine("  * Include at least one sensory detail (sound, motion, texture, temperature, light).");
            sb.AppendLine();


            // Response bounds & input
            sb.AppendLine("Response Bounds:");
            sb.AppendLine($"  * The text between <INPUT> and </INPUT> is {player}’s latest action or observation.");
            sb.AppendLine();
            sb.AppendLine("<INPUT>");
            sb.AppendLine(input);
            sb.AppendLine("</INPUT>");
            sb.AppendLine();

            return sb.ToString();
        }

        //static string BuildPrompt_Narration(string input, StoryContext storyContext)
        //{
        //    var player = storyContext.CurrentFrame.PlayerData.dataName;
        //    var location = storyContext.CurrentFrame.LocationData.locationDescription;
        //    var sb = new System.Text.StringBuilder();

        //    // Identity & scope
        //    sb.AppendLine($"You are the narrator. The world is {storyContext.Tone}.");
        //    sb.AppendLine("You are not an AI assistant, chatbot, or language model.");
        //    sb.AppendLine("You are a neutral third-person narrator describing what the player sees.");
        //    sb.AppendLine("Never refer to yourself or your role as an assistant or AI.");
        //    sb.AppendLine();

        //    sb.AppendLine($"The player is {player}.");
        //    sb.AppendLine($"    - Age: {storyContext.PlayerData.dataName}.");
        //    sb.AppendLine($"    - Gender: {storyContext.PlayerData.playerGender}.");
        //    sb.AppendLine($"    - Height: {storyContext.PlayerData.playerHeight}.");
        //    sb.AppendLine($"    - Appearance: {storyContext.PlayerData.playerAppearance}.");
        //    sb.AppendLine();

        //    // Scene state
        //    sb.AppendLine($"The {player} is currently in: {location}");
        //    sb.AppendLine();

        //    // Scenario information
        //    sb.AppendLine("Scenario Information:");
        //    sb.AppendLine($"    {storyContext.Scenario}");
        //    sb.AppendLine();

        //    // Memories
        //    string memories = BuildMemories(storyContext);
        //    if (!string.IsNullOrWhiteSpace(memories))
        //    {
        //        sb.AppendLine(memories);
        //    }

        //    // Characters (verbatim, so the model knows exactly who to output)
        //    sb.AppendLine("Characters Present (output one <CHARACTER> block for each, in this order):");
        //    sb.AppendLine(BuildCharacterDescriptionList(storyContext));
        //    sb.AppendLine();

        //    // Narration rules
        //    sb.AppendLine("Narration Guidelines:");
        //    sb.AppendLine("  * ONLY respond to content between the <INPUT> tags.");
        //    sb.AppendLine("  * Maintain an immersive, neutral tone.");
        //    sb.AppendLine("  * Do NOT include dialogue, inner thoughts, or mind-reading.");
        //    sb.AppendLine("  * Do NOT mention or reference the LLM, model, assistant, or yourself.");
        //    sb.AppendLine();

        //    // Output structure
        //    sb.AppendLine("Output Structure (exactly this, nothing else):");
        //    sb.AppendLine("  <LOCATION>…</LOCATION>");
        //    sb.AppendLine("  <CHARACTER name='…'>…</CHARACTER>  (one per character listed above, in order)");
        //    sb.AppendLine("  <END>");
        //    sb.AppendLine();

        //    // LOCATION rules with labeled lines
        //    sb.AppendLine("LOCATION Rules:");
        //    sb.AppendLine("  * Inside <LOCATION>…</LOCATION> output EXACTLY these 4 lines, one per line:");
        //    sb.AppendLine("    [1] <one sentence in 2nd person describing the environment>");
        //    sb.AppendLine("    [2] <one sentence in 2nd person describing the environment>");
        //    sb.AppendLine("    [3] <one sentence in 2nd person describing the environment>");
        //    sb.AppendLine("    [4] <one sentence in 2nd person describing the environment>");
        //    sb.AppendLine("  * Do NOT mention or refer to any characters in LOCATION (no names, titles, or pronouns).");
        //    sb.AppendLine("  * No blank lines inside <LOCATION>. No extra lines beyond [1]-[4].");
        //    sb.AppendLine();

        //    // Forbidden character references inside LOCATION (hard ban)
        //    string characterNames = BuildCharacterNameList(storyContext);
        //    sb.AppendLine("Forbidden in LOCATION:");
        //    if (!string.IsNullOrWhiteSpace(characterNames))
        //        sb.AppendLine($"  * Any of these names or references: {characterNames}"); // append others if present
        //    sb.AppendLine("  * Any third-person pronoun referring to characters (he, she, they, her, him, them).");
        //    sb.AppendLine();

        //    // CHARACTER rules (single source of truth)
        //    // CHARACTER rules (use [1]/[2] consistently)
        //    sb.AppendLine("CHARACTER Rules (very important):");
        //    sb.AppendLine("  * For EACH character, inside <CHARACTER name='...'>…</CHARACTER> output EXACTLY these 2 lines:");
        //    sb.AppendLine("    [1] One sentence describing the character’s physical snapshot — appearance, posture, or clothing. No inner thoughts.");
        //    sb.AppendLine($"    [2] One short, observable behavior directed TOWARD {player}, starting with the character’s name.");
        //    sb.AppendLine("  * Each line must end with a period. No blank lines. No extra lines beyond [1] and [2].");
        //    sb.AppendLine("  * Do NOT use placeholders or ellipses: '...', '…', 'N/A'.");
        //    sb.AppendLine("  * Use strictly observable cues (distance, gaze, body tension).");
        //    sb.AppendLine("  * Do NOT use mind-reading words: feels, thinks, wants, hopes, longs, desires, yearns, loves, hates.");
        //    sb.AppendLine("  * No quoted dialogue.");
        //    sb.AppendLine();

        //    // --- Structure example, no parentheses ---
        //    sb.AppendLine("Example Output (STRUCTURE ONLY — do not copy wording):");
        //    sb.AppendLine("<LOCATION>");
        //    sb.AppendLine("[1] {sentence}");
        //    sb.AppendLine("[2] {sentence}");
        //    sb.AppendLine("[3] {sentence}");
        //    sb.AppendLine("[4] {sentence}");
        //    sb.AppendLine("</LOCATION>");
        //    sb.AppendLine("<CHARACTER name='{Name}'>");
        //    sb.AppendLine("[1] {One sentence describing physical appearance, posture, or clothing.}");
        //    sb.AppendLine($"[2] {{One short, observable behavior toward {player}, starting with the character’s name.}}");
        //    sb.AppendLine("</CHARACTER>");
        //    sb.AppendLine("<END>");
        //    sb.AppendLine();

        //    // Originality + format constraints
        //    sb.AppendLine("Originality Rules:");
        //    sb.AppendLine("  * Do NOT reuse exact phrases from examples; examples indicate structure only.");
        //    sb.AppendLine("  * Vary sentence openings; avoid starting two sentences with the same two-word phrase.");
        //    sb.AppendLine("  * Include at least one sensory detail (sound, motion, texture, temperature, light).");
        //    sb.AppendLine();

        //    sb.AppendLine("Hard Format Constraints:");
        //    sb.AppendLine("  * The FIRST non-whitespace characters MUST be exactly \"<LOCATION>\".");
        //    sb.AppendLine("  * Tag order MUST be: <LOCATION>…</LOCATION> then <CHARACTER…>…</CHARACTER> (one per listed character), then <END>.");
        //    sb.AppendLine("  * Do NOT write any text before <LOCATION> or after <END>.");
        //    sb.AppendLine("  * Placeholders are forbidden anywhere: '...', '…', 'N/A'.");
        //    sb.AppendLine();

        //    // Response bounds & input
        //    sb.AppendLine("Response Bounds:");
        //    sb.AppendLine($"  * The text between <INPUT> and </INPUT> is {player}’s latest action or observation.");
        //    sb.AppendLine("  * After writing <END>, STOP. Do not add anything after <END>.");
        //    sb.AppendLine();
        //    sb.AppendLine("<INPUT>");
        //    sb.AppendLine(input);
        //    sb.AppendLine("</INPUT>");
        //    sb.AppendLine();

        //    return sb.ToString();
        //}
    }
}
