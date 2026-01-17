using aiimeta.Formats;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aiimeta.Reader
{
    /// <summary>Storage for the generation properties parsed from metadata.</summary>
    public class ParsedMetadata : IParsedMetadata
    {
        private readonly List<KeyValuePair<string, string>> PropertyPool = new();

        private readonly StringComparison Comparison = StringComparison.OrdinalIgnoreCase;

        public string? PositivePromptText { get; set; }
        
        public string? NegativePromptText { get; set; }

        public IEnumerable<KeyValuePair<string, string>> Properties => PropertyPool.AsEnumerable();

        public IEnumerable<string> Get(string key)
        {
            foreach (var pair in PropertyPool)
            {
                var c = string.Compare(pair.Key, key, Comparison);
                if (c == 0) yield return pair.Value;
            }
        }

        public bool Add(string key, string value)
        {
            foreach (var pair in PropertyPool)
            {
                if (string.Compare(pair.Key, key, Comparison) == 0
                    && pair.Value == value) return false;
            }
            PropertyPool.Add(new KeyValuePair<string, string>(key, value));
            return true;
        }

        public bool AddRange(string key, IEnumerable<string> values)
        {
            var added = true;
            foreach (var value in values) added &= Add(key, value);
            return added;
        }

        public bool AddRange(IEnumerable<KeyValuePair<string, string>> pairs)
        {
            var added = true;
            foreach (var pair in pairs) added &= Add(pair.Key, pair.Value);
            return added;
        }

        public bool Remove(string key, string value)
        {
            for (int i = PropertyPool.Count - 1; i >= 0; --i)
            {
                var pair = PropertyPool[i];
                if (string.Compare(pair.Key, key, Comparison) == 0
                    && pair.Value == value)
                {
                    PropertyPool.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        public bool RemoveAll(string key)
        {
            bool result = false;
            for (int i = PropertyPool.Count - 1; i >= 0; --i)
            {
                var pair = PropertyPool[i];
                if (string.Compare(pair.Key, key, Comparison) == 0)
                {
                    PropertyPool.RemoveAt(i);
                    result = true;
                }
            }
            return result;
        }
    }
}
