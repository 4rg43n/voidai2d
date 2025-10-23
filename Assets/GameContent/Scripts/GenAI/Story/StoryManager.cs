using System.Collections.Generic;
using UnityEngine;
using VoidAI.GenAI.Agent;
using VoidAI.GenAI.Text;

namespace VoidAI.GenAI.Story
{
    public class StoryManager : MonoBehaviour
    {
        public StoryContext storyContext;
        public string resourcePathName = "Test/white_void";
        public string testPlayerName = "Raven";

        private void Start()
        {
            storyContext = LoadStoryFromResources(resourcePathName);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("Starting LLM cal==========>");
                HandleInput();
            }
        }

        public void HandleInput()
        {
            TextGenBridge textBridge = TextGenBridge.Singleton;
            string prompt = BuildPrompt("observe");

            textBridge.SendToLLM(prompt, storyContext.PlayerData.dataName, LLM_Model_Defs.CHARACTER_LLM, (resp) => { HandleLLMOutput(resp); });
        }

        void HandleLLMOutput(string response)
        {
            Debug.Log($"LLM: {response}");
        }

        StoryContext LoadStoryFromResources(string resourcePathName)
        {
            StoryContext newStoryContext = new StoryContext();
            List<string[]> parsedData = DataUtils.ParseData(resourcePathName);
            newStoryContext.LocationData = new LocationData();
            newStoryContext.LocationData.LoadFromResourcePath(parsedData);


            newStoryContext.PlayerData = new PlayerData();
            newStoryContext.PlayerData.dataName = testPlayerName;


            return newStoryContext;
        }

        string BuildPrompt(string input)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"You are the narrator. The world is {storyContext.Tone}.");
            sb.AppendLine($"The {storyContext.PlayerData.dataName} is currently in: {storyContext.LocationData.dataDescription}");
            sb.AppendLine();

            //sb.AppendLine(BuildRecentMemory(context));
            //sb.AppendLine(BuildCurrentFacts(context));
            //sb.AppendLine(BuildCharacterDescriptions(context));
            //sb.AppendLine();


            sb.AppendLine($"There are no AI companions, avatars, or artificial entities present in this world unless explicitly described.");
            sb.AppendLine($"Do NOT mention or reference yourself, the LLM, the model, or any abstract narrator entity outside the story.");
            sb.AppendLine($"You are simply the narrator, describing what the player sees — do not insert yourself or commentary.");


            sb.AppendLine($"Describe what {storyContext.PlayerData.dataName} sees in 1–2 short cinematic sentences, using second-person perspective.");
            sb.AppendLine("Begin the description with 'You...' or use 'you see...' where appropriate.");
            sb.AppendLine("Mention the environment and any characters present by name.");
            sb.AppendLine("Use the character's name when describing them (e.g., 'Zara stands near the flowers').");
            sb.AppendLine("Only describe outward appearances, posture, and actions. Do NOT describe internal thoughts or emotions.");
            sb.AppendLine("Avoid quoting speech or assuming relationships.");
            sb.AppendLine("Maintain an immersive, neutral tone.");
            sb.AppendLine("Do NOT speak from the perspective of any character. Avoid inner thoughts or dialogue.");
            sb.AppendLine();

            //sb.AppendLine("After the description, include a single line like this:");
            //sb.AppendLine("<thought>[a poetic narrator-style summary of what’s happening or what the moment means]</thought>");
            //sb.AppendLine("This is not a character thought. It is a narrative reflection, as if closing the scene.");
            //sb.AppendLine("Examples:");
            //sb.AppendLine("<thought>You and Zara talk long into the night.</thought>");
            //sb.AppendLine("<thought>You were spoiling for a fight. Now you've got one.</thought>");
            //sb.AppendLine("<thought>You drift off to sleep hoping the nightmares won't find you.</thought>");
            //sb.AppendLine();

            sb.AppendLine($"{storyContext.PlayerData.dataName} input: {input}");

            return sb.ToString();
        }
    }
}



