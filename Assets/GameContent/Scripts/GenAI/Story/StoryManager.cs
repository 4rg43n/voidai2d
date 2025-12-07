using UnityEngine;
using VoidAI.GenAI.Agent;
using VoidAI.GenAI.Text;

namespace VoidAI.GenAI.Story
{
    public class StoryManager : MonoBehaviour
    {
        public static StoryManager Singleton { get; private set; }

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

        private void Awake()
        {
            Singleton = this;
        }

        private void Start()
        {
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
                    storyContext.NarratorData.dataName,
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
                        AddCharacterMessage(frame.StoryResponse.messageData.playerInput, frame.Id, true, false);
                    AddCharacterMessage(frame.StoryResponse.formattedResponse, frame.Id, false, false);
                }

                Debug.Log("Loaded story from disk and restored messages.");
            }
        }

        public void RerollResponse()
        {
            string playerInput = storyContext.CurrentFrame.StoryResponse.messageData.playerInput;
            HandleInput(playerInput);

            DeleteFrame(storyContext.CurrentFrame.Id);
            SubmitPlayerInput(playerInput);
        }

        public void DeleteFrame(string frameId)
        {
            storyContext.DeleteFrameById(frameId);
            ChatPanelUI.Singleton.UpdateView();
            SaveStory();
        }

        public void SubmitPlayerInput(string input)
        {
            AddCharacterMessage(input, storyContext.CurrentFrame.Id, true, false);
            HandleInput(input);
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
            messageLLM.speakerName = storyContext.NarratorData.dataName;
            StoryMessageLLM storyMessageLLM = new StoryMessageLLM()
            {
                messageData = messageLLM,
                agentRole = storyContext.NarratorData.agentRole,
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

            AddCharacterMessage(
                storyContext.CurrentFrame.StoryResponse.formattedResponse,
                storyContext.CurrentFrame.Id,
                false,
                true);

            storyContext.CurrentFrame.StoryResponse.LogDetails();
        }

        public void AddCharacterMessage(string msg, string frameId, bool isPlayer, bool save)
        {
            if (isPlayer)
                ChatPanelUI.Singleton.AddPlayerMessage(frameId + ":" + msg, frameId);
            else
                ChatPanelUI.Singleton.AddCharacterMessage(frameId + ":" + msg, frameId);

            if (save)
            {
                SaveStory();
            }
        }

        void SaveStory()
        {
            Debug.Log($"Story saved to {LoadSaveStoryUtils.SaveStory(storyContext)}");
        }
    }

}



