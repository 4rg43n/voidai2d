using System;
using System.Collections.Generic;
using UnityEngine;

namespace VoidAI.GenAI.Agent
{
    [Serializable]
    public abstract class BaseData
    {
        public string dataId = "";
        public string dataName = "New Data";
        public string dataDescription = "Description";

        public BaseData()
        {
            dataId = Guid.NewGuid().ToString();
        }

        public virtual void LoadFromResourcePath(List<string[]> parsedData)
        {
            // Override in derived classes to load data from resources
            foreach(var entry in parsedData)
            {
                switch(entry[0])
                {
                    case "dataName":
                        dataName = entry.Length > 1 ? entry[1] : dataName;
                        break;
                    case "dataDescription":
                        dataDescription = entry.Length > 1 ? entry[1] : dataDescription;
                        break;
                    default:
                        Debug.LogWarning($"BaseData: Unknown data field '{entry[0]}'");
                        break;
                }
            }
        }
    }
}


