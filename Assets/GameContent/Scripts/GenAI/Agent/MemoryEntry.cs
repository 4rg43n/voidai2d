using System;
using UnityEngine;

namespace VoidAI.GenAI.Agent
{
    [Serializable]
    public class MemoryEntry
    {
        public string Id;
        public string Full;
        public string Summary;

        public MemoryEntry()
        {
            Id = Guid.NewGuid().ToString();
        }

        public override string ToString()
        {
            return $"Memory {Id} - {Summary}";
        }

        public static MemoryEntry Clone(MemoryEntry entry)
        {
            return new MemoryEntry()
            {
                Id = entry.Id,
                Full = entry.Full,
                Summary = entry.Summary
            };
        }
    }
}


