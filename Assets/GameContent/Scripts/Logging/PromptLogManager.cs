using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class PromptLogManager : MonoBehaviour
{
    public static PromptLogManager Instance { get; private set; }

    [Header("Log Rotation")]
    [Tooltip("How many prompt logs to retain (newest kept).")]
    public int maxLogFiles = 10;

    private List<PromptLogEntry> logEntries = new();

    private string logsDirectory;
    private string currentLogPath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        logsDirectory = Path.Combine(Application.persistentDataPath, "Logs");

        try
        {
            if (!Directory.Exists(logsDirectory))
                Directory.CreateDirectory(logsDirectory);

            // Create a timestamped file for THIS session
            string fileName = $"PromptLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
            currentLogPath = Path.Combine(logsDirectory, fileName);

            File.WriteAllText(currentLogPath,
                $"Prompt Log Started: {DateTime.Now:F}\nUnity Version: {Application.unityVersion}\n---------------------------------------------\n");

            CleanupOldLogs();
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PromptLogManager] Failed to init logging: {ex}");
        }
    }

    /// <summary>
    /// Deletes older log files leaving only the newest N
    /// </summary>
    private void CleanupOldLogs()
    {
        try
        {
            var files = Directory.GetFiles(logsDirectory, "PromptLog_*.log")
                                 .OrderByDescending(File.GetCreationTimeUtc)
                                 .ToList();

            if (files.Count <= maxLogFiles)
                return;

            var toDelete = files.Skip(maxLogFiles);

            foreach (var f in toDelete)
            {
                File.Delete(f);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PromptLogManager] Cleanup failed: {ex}");
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
        var entry = new PromptLogEntry(agentName, agentType, promptType, modelName, prompt, originalResponse, response);
        logEntries.Add(entry);

        string line =
            $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}]\n" +
            $"AGENT: {agentName} ({agentType})\n" +
            $"PROMPT TYPE: {promptType} ({promptType})\n" +
            $"MODEL: {modelName}\n" +
            $"PROMPT:\n{prompt}\n\n" +
            $"RAW RESPONSE:\n{originalResponse}\n\n" +
            $"CLEAN RESPONSE:\n{response}\n" +
            $"---------------------------------------------\n";

        AppendToFile(line);

        Debug.Log($"[LLM LOG] [{agentName}] logged prompt");
    }

    private void AppendToFile(string line)
    {
        try
        {
            File.AppendAllText(currentLogPath, line);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PromptLogManager] Failed to write to log: {ex}");
        }
    }

    public List<PromptLogEntry> GetAllLogs()
    {
        return new List<PromptLogEntry>(logEntries);
    }

    public void ClearLogsInMemory()
    {
        logEntries.Clear();
    }
}
