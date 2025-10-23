using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoidAI.GenAI.Agent
{

    public static class DataUtils
    {
        public static List<string[]> ParseData(string resourcePath)
        {
            List<string> lines = ReadLinesFromResources(resourcePath);
            List<string[]> parsedData = new List<string[]>();

            foreach (string line in lines)
            {
                string[] entries = line.Split('|');
                if (entries == null || entries.Length < 2)
                {
                    Debug.LogWarning($"Unable to parse line: {line}");
                    continue;
                }

                string entry0 = entries[0].Trim();
                //entry0 = entry0.Substring(0, entry0.Length - 1);
                string entry1 = entries[1].Trim();

                parsedData.Add(new string[] { entry0, entry1 });
            }

            return parsedData;
        }

        /// <summary>
        /// Loads a TextAsset from Resources (use path without "Resources/" and without extension)
        /// and returns all non-empty, trimmed lines that do not start with '#'.

        /// </summary>
        public static List<string> ReadLinesFromResources(string resourcePath)
        {
            var result = new List<string>();
            if (string.IsNullOrEmpty(resourcePath))
                return result;

            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset == null)
            {
                Debug.LogWarning($"DataUtils: TextAsset not found at Resources/{resourcePath}");
                return result;
            }

            var lines = textAsset.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var raw in lines)
            {
                var line = raw?.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.StartsWith("#"))
                    continue;
                result.Add(line);
            }

            return result;
        }
    }
}


