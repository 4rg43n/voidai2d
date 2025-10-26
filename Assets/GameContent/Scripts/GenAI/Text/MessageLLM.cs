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
            originalResponse = resp;
            response = RemoveLineMarkersAndNormalizeWhitespace(resp);
        }

        public void ParseResponseTags()
        {
            string cleanedResponse = RemoveLineMarkersAndNormalizeWhitespace(response);
            parsedTags = ParseTags(cleanedResponse);

            if (parsedTags == null || parsedTags.Count == 0)
            {
                parsedTags = new List<MessageLLMTag>();
                parsedTags.Add(new MessageLLMTag() { tag = "LOCATION", value = cleanedResponse });
                Debug.LogWarning("Response didn't contain proper tags, making everything LOCATION.");
            }
        }

        /// <summary>
        /// Removes [N] markers like [1], [2], etc., and collapses any runs of
        /// two or more whitespace characters into a single space.
        /// </summary>
        public static string RemoveLineMarkersAndNormalizeWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Remove [N] markers (e.g., [1], [12])
            string result = Regex.Replace(input, @"\[\d+\]\s*", string.Empty);

            // Replace multiple spaces, tabs, or newlines with a single space
            result = Regex.Replace(result, @"\s{2,}", " ");

            // Trim leading/trailing whitespace
            return result.Trim();
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

