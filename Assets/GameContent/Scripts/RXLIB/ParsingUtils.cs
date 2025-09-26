using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.Utils
{
    public static class ParsingUtils
    {
        public static readonly char COMMA_SUB = '$';

        public static string[] ParseFunction(string fstr)
        {
            fstr = ConvertFunctionString(fstr);

            string[] tokens = fstr.Trim().Split("(");

            string fname = tokens[0].Trim();
            string argstr = fstr.Substring(fname.Length);

            argstr=argstr.Substring(1, argstr.Length - 2);

            tokens = argstr.Split(COMMA_SUB);
            List<string> result = new List<string>();
            result.Add(fname);

            if (tokens.Length>0)
            {
                foreach(string tok in tokens)
                {
                    string token = tok.Trim();
                    if (token.StartsWith('"'))
                    {
                        result.Add(token.Substring(1, token.Length - 2));
                    }
                }
            }

            return result.ToArray();
        }

        static string ConvertFunctionString(string fstr)
        {
            bool inParen = false;
            bool inQuote = false;
            string newfstr = "";
            for (int i = 0; i < fstr.Length; i++)
            {
                char c = fstr[i];
                if (c == '(')
                    inParen = true;
                else if (c == ')')
                    inParen = false;
                else if (c == '"')
                    inQuote = !inQuote;
                else if (c == ',' && inParen && !inQuote)
                    c = COMMA_SUB;
                newfstr += c;
            }

            return newfstr;
        }

        public static string ConvertFunctionListString(string fstr)
        {
            bool inParen = false;
            bool inQuote = false;
            string newfstr = "";
            for (int i = 0; i < fstr.Length; i++)
            {
                char c = fstr[i];
                if (c == '(')
                    inParen = true;
                else if (c == ')')
                    inParen = false;
                else if (c == '"')
                    inQuote = !inQuote;
                else if (c == ',' && !inParen && !inQuote)
                    c = COMMA_SUB;
                newfstr += c;
            }

            return newfstr;
        }
    }
}


