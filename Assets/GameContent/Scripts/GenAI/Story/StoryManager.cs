using System.Collections.Generic;
using UnityEngine;
using VoidAI.GenAI.Agent;
using VoidAI.GenAI.Text;

namespace VoidAI.GenAI.Story
{
    public class StoryManager : MonoBehaviour
    {
        public bool overrideLoad = false;

        public StoryContext storyContext;
        public string testScenarioName = "Test/rins_confession";
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
            ChatPanelUI.Singleton.OnSubmit += (input) => { HandleInput(input); };
            LoadStoryContext();
        }

        void LoadStoryContext()
        {
            storyContext = DataUtils.LoadStoryFromResources(testScenarioName, testPlayerName);
            Debug.Log($"Loaded test story title={storyContext.Title}");

            StoryContext loadedStory = overrideLoad ? null : LoadSaveStoryUtils.LoadStory(storyContext);
            if (loadedStory == null)
            {
                // display the first message
                MessageLLM firstMessageLLM = TextGenBridge.CreateMessageLLM(
                    storyContext.FirstMessage,
                    storyContext.narrator.dataName,
                    "",
                    "",
                    "");
                HandleLLMOutput(firstMessageLLM);

                Debug.Log("Loaded new story. Displaying first message.");
            }
            else
            {
                // restore the loaded story
                storyContext = loadedStory;
                foreach (var frame in storyContext.storyFrames)
                {
                    if (!string.IsNullOrEmpty(frame.StoryResponse.messageData.playerInput))
                        ChatPanelUI.Singleton.AddPlayerMessage(frame.StoryResponse.messageData.playerInput);
                    AddCharacterMessage(frame.StoryResponse.formattedResponse, false);
                }

                Debug.Log("Loaded story from disk and restored messages.");
            }
        }

        public void HandleInput(string input)
        {
            PromptType promptType = PromptType.Character;

            if (input.Trim().ToLower() == "observe" || input.Trim().ToLower() == "look")
                promptType = PromptType.Narration;

            Debug.Log($"Sending to LLM={input}");
            TextGenBridge textBridge = TextGenBridge.Singleton;
            string prompt = PromptUtils.BuildPrompt(input, promptType, storyContext);

            textBridge.SendToLLM(
                input,
                prompt,
                storyContext.CurrentFrame.PlayerData.dataName,
                LLM_Model_Defs.CHARACTER_LLM,
                (resp) => { HandleLLMOutput(resp); });
        }

        void HandleLLMOutput(MessageLLM messageLLM)
        {
            bool isSpecialOutput = string.IsNullOrEmpty(messageLLM.prompt);

            // fill out the message data
            messageLLM.speakerName = storyContext.narrator.dataName;
            StoryMessageLLM storyMessageLLM = new StoryMessageLLM()
            {
                messageData = messageLLM,
                agentRole = storyContext.narrator.agentRole,
                promptType = "narration"
            };

            Debug.Log($"LLM: {messageLLM.response}");

            // process memories and facts

            if (!isSpecialOutput)
                storyContext.AddNewFrame();
            // TODO: process memories and facts here
            storyContext.CurrentFrame.StoryResponse = storyMessageLLM;

            // format for printing
            if (!isSpecialOutput)
            {
                storyContext.CurrentFrame.StoryResponse.ParseResponse();
                storyContext.CurrentFrame.StoryResponse.CreateFormattedResponse();
            }
            else
                storyContext.CurrentFrame.StoryResponse.formattedResponse = messageLLM.response;

            AddCharacterMessage(storyContext.CurrentFrame.StoryResponse.formattedResponse, true);

            storyContext.CurrentFrame.StoryResponse.LogDetails();
        }

        public void AddCharacterMessage(string msg, bool save)
        {
            ChatPanelUI.Singleton.AddCharacterMessage(msg);
            if (save)
            {
                Debug.Log($"Story saved to {LoadSaveStoryUtils.SaveStory(storyContext)}");
            }
        }
    }

    [System.Serializable]
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

        public StoryMessageLLM Clone()
        {
            return new StoryMessageLLM()
            {
                messageData = this.messageData,
                agentRole = this.agentRole,
                promptType = this.promptType,
                formattedResponse = this.formattedResponse
            };
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



