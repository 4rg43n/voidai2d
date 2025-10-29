using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace VoidAI.GenAI.Text
{
    public static class TagOutputSanitizer
    {
        public static void TestSanitizer()
        {
            string testStr = "<THOUGHT>Oh no, here we go. I'm so nervous! Please don't reject me.</THOUGHT>\r\n<ACTION>Rin smiles nervously and fidgets with her fingers as she speaks.</ACTION>\r\n<DIALOGUE>Hi Raven. I just wanted to have you over for a visit, enjoy some time together.\r\nDIALOGUE>";
            string[] allTags = ExtractTags(testStr);
            Debug.Log("All tags: " + allTags);
        }

        public static string[] ExtractTags(string str)
        {
            bool insideTag = false;
            string tag = "";
            List<string> allTags = new List<string>();
            for(int i=0;i<str.Length;i++)
            {
                char c = str[i];
                if (c=='<')
                {
                    if (insideTag)
                    {
                        Debug.LogError("Found nested tags in LLM output. Not allowed:" + str);
                        return null;
                    }
                    else
                    {
                        insideTag = true;
                        tag = "";
                        continue;
                    }
                }
                else if (c=='>')
                {
                    if (insideTag)
                    {
                        allTags.Add(tag);
                        insideTag = false;
                        tag = "";
                        continue;
                    }
                    else
                    {
                        Debug.LogError("Found end tag before start of tag. Not allowed:" + str);
                        return null;
                    }
                }

                if (insideTag)
                    tag += c;
            }

            return allTags.ToArray();
        }
    }
}
