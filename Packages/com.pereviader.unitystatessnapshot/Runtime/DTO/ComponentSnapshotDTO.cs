using System;
using System.Collections.Generic;

namespace PereViader.UnityStateSnapshot
{
    [Serializable]
    public class ComponentStateEntry
    {
        public string Key;
        public string Value;

        public ComponentStateEntry() { }

        public ComponentStateEntry(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }

    [Serializable]
    public class ComponentSnapshotDTO
    {
        public string TypeName;
        public bool Enabled = true;
        public List<ComponentStateEntry> StateEntries = new List<ComponentStateEntry>();
    }
}
