using System;
using System.Collections.Generic;

namespace VoidAI.GenAI.Agent
{
    public class LocationData : BaseData
    {
        public string locationDescription = "A serene park with lush greenery and a gentle breeze.";

        public override BaseData Clone()
        {
            return new LocationData
            {
                dataId = this.dataId,
                dataName = this.dataName,
                locationDescription = this.locationDescription
            };
        }

        public override void LoadFromResourcePath(List<string[]> parsedData, string userName)
        {
            base.LoadFromResourcePath(parsedData, userName);

            foreach (var entry in parsedData)
            {
                switch (entry[0])
                {
                    case "locationDescription":
                        locationDescription = entry.Length > 1 ? ReplaceAll(entry[1], user_descriptor, userName) : locationDescription;
                        break;
                }
            }
        }
    }
}

