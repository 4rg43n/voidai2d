using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using VoidAI.GenAI.Story;
using static UnityEngine.Rendering.STP;

public static class LoadSaveStoryUtils
{
    public static string ES3_SavePath = "saves/";

    public static string SaveStory(StoryContext storyContext)
    {
        string filename = GetBaseSaveFileName(storyContext);

        ES3.Save<StoryContext>("storyContext", storyContext, filename);

        return filename;
    }

    public static StoryContext LoadStory(StoryContext storyContext)
    {
        string filename = GetBaseSaveFileName(storyContext);
        if (ES3.FileExists(filename))
        {
            StoryContext loadedStory = ES3.Load<StoryContext>("storyContext", filename);
            return loadedStory;
        }
        else
        {
            Debug.LogWarning($"No save file found for story '{storyContext.Title}' with player '{storyContext.PlayerData.dataName}'");
            return null;
        }
    }

    static string GetBaseSaveFileName(StoryContext storyContext)
    {
        string storyTitleSafe = ToSafeFilename(storyContext.Title);
        string baseFileName = $"{ES3_SavePath}{storyTitleSafe}_Save_{storyContext.PlayerData.dataName}.es3";
        return baseFileName;
    }

    static string ToSafeFilename(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "%";

        List<char> invalid = new List<char>(Path.GetInvalidFileNameChars());
        var sb = new StringBuilder(input.Length);

        foreach (char c in input)
        {
            if (invalid.Contains(c))
                sb.Append('%');
            else
                sb.Append(c);
        }

        string result = sb.ToString().Trim();

        // Optional: collapse consecutive '%' into a single '%'
        // result = Regex.Replace(result, "%{2,}", "%");

        if (result.Length == 0)
            return "%";

        return result;
    }
}


