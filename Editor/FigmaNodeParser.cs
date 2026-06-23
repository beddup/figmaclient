using System;
using UnityEngine;

namespace FigmaClient
{
    public class FigmaNodeParser
    {
        public T ParseNode<T>(string json) where T : class
        {
            var substr = json.Split(new string[]{"\"document\":"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in substr)
            {
                try
                {
                    if (s.Contains("id"))
                    {
                        return ParseSingleNode<T>(s.Trim());
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"parse node exception {e.Message}");
                }
            }
            return null;
        }

        private T ParseSingleNode<T>(string s)
        {
            var jsonData = FixBraces(s);
            return JsonUtility.FromJson<T>(jsonData);
        }

        private string FixBraces(string s)
        {
            int lastProperPlace = -1;
            int bracesCount = 0;
            for (int i = 1; i < s.Length; i++)
            {
                if (s[i] == '{')
                    bracesCount++;
                if (s[i] == '}')
                    bracesCount--;
                if (bracesCount == 0)
                    lastProperPlace = i;
                if (bracesCount == -1)
                    return s.Substring(0, i + 1);
            }
            return s.Substring(0, lastProperPlace + 1);
        }
    }
}
