using System;
using UnityEngine;

namespace VoidAI.GenAI.Agent
{
    [Serializable]
    public class CharacterFact
    {
        public string Id;        // Unique identifier
        public string Type;      // e.g. "Clothing", "Location", "Mood"
        public string Value;     // e.g. "White sundress"
        public bool IsDefault;   // For reversion later

        public override string ToString()
        {
            return $"Fact {Type} - {Value}";
        }

        public static CharacterFact Clone(CharacterFact fact)
        {
            return new CharacterFact()
            {
                Id = fact.Id,
                Type = fact.Type,
                Value = fact.Value,
                IsDefault = fact.IsDefault
            };
        }
    }
}

