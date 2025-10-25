using System;

[System.Serializable]
public class PromptLogEntry
{
    public string AgentName;
    public string AgentType;
    public string ModelName;
    public string Prompt;
    public string Response;
    public DateTime Timestamp;

    public PromptLogEntry(string agentName, string agentType, string modelName, string prompt, string response)
    {
        AgentName = agentName;
        AgentType = agentType;
        ModelName = modelName;
        Prompt = prompt;
        Response = response;
        Timestamp = DateTime.Now;
    }
}
