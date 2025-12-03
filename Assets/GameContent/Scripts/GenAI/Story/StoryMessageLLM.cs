using UnityEngine;
using VoidAI.GenAI.Text;

namespace VoidAI.GenAI.Story
{
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