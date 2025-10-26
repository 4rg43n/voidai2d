using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VoidAI.GenAI.Text
{

    public class MessageLLM
    {
        public string speakerName;
        public string modelName;
        public string prompt;

        public string originalResponse;
        public string response;

        public List<MessageLLMTag> parsedTags = new List<MessageLLMTag>();

        public MessageLLM(string resp)
        {
            response = resp;
        }

        public void ParseResponseTags()
        {
            parsedTags = ParseTags(response);

            if (parsedTags == null || parsedTags.Count == 0)
            {
                parsedTags = new List<MessageLLMTag>();
                parsedTags.Add(new MessageLLMTag() { tag = "LOCATION", value = response });
                Debug.LogWarning("Response didn't contain proper tags, making everything LOCATION.");
            }
        }


        /// <summary>
        /// Parses a string containing XML-like tags into a list of MessageLLMTag objects.
        /// Example:
        /// <LOCATION>You stand...</LOCATION>
        /// <CHARACTER name='Rin'>Rin is petite...</CHARACTER>
        /// </summary>
        public static List<MessageLLMTag> ParseTags(string input)
        {
            var results = new List<MessageLLMTag>();
            if (string.IsNullOrWhiteSpace(input))
                return results;

            // Matches things like:
            // <TAG attr1='val1' attr2="val2"> ... </TAG>
            var tagPattern = new Regex(
                @"<(?<tag>\w+)(?<attrs>[^>]*)>(?<value>.*?)</\k<tag>>",
                RegexOptions.Singleline | RegexOptions.IgnoreCase);

            foreach (Match match in tagPattern.Matches(input))
            {
                var tag = new MessageLLMTag
                {
                    tag = match.Groups["tag"].Value.Trim().ToUpperInvariant(),
                    value = match.Groups["value"].Value.Trim()
                };

                string attrs = match.Groups["attrs"].Value;
                if (!string.IsNullOrWhiteSpace(attrs))
                {
                    // Match name='value' or name="value"
                    var attrPattern = new Regex(@"(\w+)\s*=\s*['""]([^'""]+)['""]");
                    foreach (Match a in attrPattern.Matches(attrs))
                    {
                        tag.attributes.Add(new string[] { a.Groups[1].Value, a.Groups[2].Value });
                    }
                }

                results.Add(tag);
            }

            return results;
        }
    }

    public class MessageLLMTag
    {
        public string tag;
        public List<string[]> attributes = new List<string[]>();
        public string value;
    }
}

