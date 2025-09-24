using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RX.AI
{
    public class AIBlackboard
    {
        public Dictionary<string, object> dataMap = new Dictionary<string, object>();

        public bool HasKey(string key)
        {
            return dataMap.ContainsKey(key);
        }

        public bool RemoveKey(string key)
        {
            if (dataMap.ContainsKey(key))
            {
                return dataMap.Remove(key);
            }

            return false;
        }

        public bool HasValue<T>(string key)
        {
            if (dataMap.ContainsKey(key) && dataMap[key] is T)
                return true;

            return false;
        }

        public T GetValue<T>(string key)
        {
            if (!HasKey(key) || !HasValue<T>(key))
                return default(T);

            return (T)dataMap[key];
        }

        public void SetValue<T>(string key, T value)
        {

            dataMap[key] = value;
        }

        public List<string> GetSaveData()
        {
            List<string> data = new List<string>();
            foreach (string key in dataMap.Keys)
            {
                object val = dataMap[key];
                if (val is string)
                {
                    data.Add(key);
                    data.Add((string)val);
                }
            }

            return data;
        }

        public void LoadFromData(List<string> data)
        {
            for (int i = 0; i < data.Count; i += 2)
            {
                dataMap[data[i]] = data[i + 1];
            }
        }
    }
}


