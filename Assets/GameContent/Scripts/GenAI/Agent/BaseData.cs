using System;
using System.Collections.Generic;

namespace VoidAI.GenAI.Agent
{
    public abstract class BaseData
    {
        public static string user_descriptor = "{{user}}";

        public string dataId = "";
        public string dataName = "New Data";

        public BaseData()
        {
            dataId = Guid.NewGuid().ToString();
        }

        public virtual void LoadFromResourcePath(List<string[]> parsedData, string userName)
        {
            // Override in derived classes to load data from resources
            foreach (var entry in parsedData)
            {
                switch (entry[0])
                {
                    case "dataName":
                        dataName = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : dataName;
                        break;
                }
            }
        }

        public abstract BaseData Clone();

        /// <summary>
        /// Replaces all occurrences of a given substring in a string with another substring.
        /// </summary>
        /// <param name="input">The original string.</param>
        /// <param name="toReplace">The substring to replace.</param>
        /// <param name="toReplaceWith">The replacement substring.</param>
        /// <returns>A new string with all instances replaced.</returns>
        public static string ReplaceAll(string input, string toReplace, string toReplaceWith)
        {
            if (input == null) return null;
            if (string.IsNullOrEmpty(toReplace)) return input;

            return input.Replace(toReplace, toReplaceWith);
        }
    }
}


