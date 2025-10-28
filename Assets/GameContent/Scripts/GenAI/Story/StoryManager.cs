using UnityEngine;
using VoidAI.GenAI.Agent;
using VoidAI.GenAI.Text;

namespace VoidAI.GenAI.Story
{
    public class StoryManager : MonoBehaviour
    {
        public StoryContext storyContext;
        public string locationPathName = "Test/location_white_void";
        public string characterPathName = "Test/character_rin";
        public string testPlayerName = "Raven";

        public PromptType testPromptType = PromptType.Narration;

        public Color locationTagColor = Color.black;
        public Color characterTagColor = Color.blue;
        public Color thoughtTagColor = Color.gray;
        public Color actionTagColor = Color.blue;
        public Color dialogueTagColor = Color.green;

        private void Start()
        {
            storyContext = LoadStoryFromResources(new string[] { locationPathName, characterPathName });
            ChatPanelUI.Singleton.OnSubmit += (input) => { HandleInput(input); };
        }

        //private void Update()
        //{
        //    if (Input.GetKeyDown(KeyCode.T))
        //    {
        //        Debug.Log("Starting LLM cal==========>");
        //        HandleInput();
        //    }
        //}

        public void HandleInput(string input)
        {
            Debug.Log($"Sending to LLM={input}");
            TextGenBridge textBridge = TextGenBridge.Singleton;
            string prompt = PromptUtils.BuildPrompt(input, testPromptType, storyContext);

            textBridge.SendToLLM(
                prompt,
                storyContext.CurrentFrame.PlayerData.dataName,
                LLM_Model_Defs.CHARACTER_LLM,
                (resp) => { HandleLLMOutput(resp); });
        }

        void HandleLLMOutput(MessageLLM messageLLM)
        {
            messageLLM.speakerName = storyContext.narrator.dataName;
            StoryMessageLLM storyMessageLLM = new StoryMessageLLM()
            {
                messageData = messageLLM,
                agentRole = storyContext.narrator.agentRole,
                promptType = "narration"
            };

            Debug.Log($"LLM: {messageLLM.response}");

            storyMessageLLM.ParseResponse();
            storyMessageLLM.CreateFormattedResponse();
            ChatPanelUI.Singleton.AddCharacterMessage(storyMessageLLM.formattedResponse);

            storyMessageLLM.LogDetails();
        }

        StoryContext LoadStoryFromResources(string[] resourcePathNames)
        {
            StoryContext newStoryContext = new StoryContext();

            newStoryContext.CurrentFrame.PlayerData = new PlayerData();
            newStoryContext.CurrentFrame.PlayerData.dataName = testPlayerName;

            newStoryContext.CurrentFrame.LocationData = DataUtils.LoadDataFromResources<LocationData>(resourcePathNames[0], newStoryContext.CurrentFrame.PlayerData.dataName);
            newStoryContext.CurrentFrame.CharacterData = DataUtils.LoadDataFromResources<CharacterData>(resourcePathNames[1], newStoryContext.CurrentFrame.PlayerData.dataName);


            newStoryContext.narrator = new NarratorData() { agentRole = "narrator", dataName = "Narrator" };

            return newStoryContext;
        }
    }

    public class StoryMessageLLM
    {
        public MessageLLM messageData;
        public string agentRole;
        public string promptType;

        public string formattedResponse;

        public void LogDetails()
        {
            PromptLogManager.Instance.LogPrompt(
                agentName: messageData.speakerName,
                agentType: agentRole,
                promptType: promptType,
                modelName: TextGenBridge.Singleton.modelName,
                prompt: messageData.prompt,
                originalResponse: messageData.originalResponse,
                response: messageData.response);
        }

        public void CreateFormattedResponse()
        {
            formattedResponse = "";
            foreach (MessageLLMTag tag in messageData.parsedTags)
            {
                Color tagColor = GetTagColor(tag.tag);
                formattedResponse += $"<color=#{ColorUtility.ToHtmlStringRGB(tagColor)}>{tag.value}</color>\n\n";
            }
        }


        public Color GetTagColor(string tag)
        {
            if (tag == "LOCATION")
                return GameManager.Singleton.StoryManager.locationTagColor;
            else if (tag == "CHARACTER")
                return GameManager.Singleton.StoryManager.characterTagColor;
            else if (tag == "THOUGHT")
                return GameManager.Singleton.StoryManager.thoughtTagColor;
            else if (tag == "ACTION")
                return GameManager.Singleton.StoryManager.actionTagColor;
            else if (tag == "DIALOGUE")
                return GameManager.Singleton.StoryManager.dialogueTagColor;
            else
                return Color.black;
        }

        public void ParseResponse()
        {
            messageData.ParseResponseTags();
        }
    }
}



