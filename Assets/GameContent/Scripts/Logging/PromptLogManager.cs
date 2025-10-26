using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PromptLogManager : MonoBehaviour
{
    public static PromptLogManager Instance { get; private set; }

    private List<PromptLogEntry> logEntries = new();
    private string logFilePath;
    private string backupFilePath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Set up paths

        string logDir = Path.Combine(Application.persistentDataPath, "logs");
        Directory.CreateDirectory(logDir);

        logFilePath = Path.Combine(logDir, "LLM-prompt.log");
        backupFilePath = Path.Combine(logDir, "LLM-prompt-bak.log");

        RotateLogs();
    }

    private void RotateLogs()
    {
        try
        {
            if (File.Exists(backupFilePath))
                File.Delete(backupFilePath);

            if (File.Exists(logFilePath))
                File.Move(logFilePath, backupFilePath);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PromptLogManager] Log rotation failed: {ex.Message}");
        }
    }

    public void LogPrompt(
        string agentName, 
        string agentType, 
        string promptType, 
        string modelName, 
        string prompt, 
        string originalResponse,
        string response)
    {
        var entry = new PromptLogEntry(agentName, agentType, modelName, prompt, originalResponse, response);
        logEntries.Add(entry);

        string line = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] {agentType} '{agentName}' PROMPT TYPE:{promptType}(model: {modelName})\nPROMPT: {prompt}\nRAW RESPONSE: {originalResponse}\n\nRESPONSE: {response}\n";
        AppendToFile(line);
        Debug.Log($"[LLM LOG] [{agentName}] {prompt} => {response}");
    }

    private void AppendToFile(string line)
    {
        try
        {
            File.AppendAllText(logFilePath, line + "\n");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PromptLogManager] Failed to write to log: {ex.Message}");
        }
    }

    public List<PromptLogEntry> GetAllLogs()
    {
        return new List<PromptLogEntry>(logEntries);
    }

    public void ClearLogs()
    {
        logEntries.Clear();
    }
}
