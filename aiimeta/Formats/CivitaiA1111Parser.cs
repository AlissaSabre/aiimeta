using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace aiimeta.Formats
{
    /// <summary>Parser for the Civitai-flavored A1111 Parameters.</summary>
    public class CivitaiA1111Parser : IMetadataParser
    {
        public float Priority => 6.0f;

        private const string ResourcesKey = "Civitai resources";

        //private const string MetadataKey = "Civitai metadata";

        public void Parse(IMetadata metadata, IParsedMetadata parsed)
        {
            // Assume A1111 parser has already run, and
            // Civitai-specific metadata, if any, have been extracted from Parameters
            // to the parsed metadata.
            foreach (var r in parsed.Get(ResourcesKey).ToList())
            {
                HandleResources(r, parsed);
            }
            //foreach (var m in parsed.Get(MetadataKey).ToList())
            //{
            //    HandleMetadata(m, parsed);
            //}
        }

        private static void HandleResources(string resources, IParsedMetadata parsed)
        {
            try
            {
                var list = new List<KeyValuePair<string, string>>();
                var array = JArray.Parse(resources);
                foreach (var item in array)
                {
                    if (item is JObject resource)
                    {
                        var type = resource["type"]?.ToString();
                        var name = resource["modelName"]?.ToString();
                        var version = resource["modelVersionName"]?.ToString();
                        var weight = resource["weight"]?.ToString();

                        if (type is not null && name is not null)
                        {
                            var value = name;
                            if (version is not null) value += $" ({version})";
                            if (weight is not null)  value += $", weight {weight}";
                            list.Add(new KeyValuePair<string, string>(type, value));
                        }
                    }
                }
                parsed.AddRange(list);
            }
            catch (JsonException) { }
        }

        // I see no useful info for a tool external to Civitai in Civitai metadata.
        // Just stop parsing it.
        //private static bool HandleMetadata(string metadata, IParsedMetadata parsed)
        //{
        //    // We don't touch it for the moment.
        //    return false;
        //}
    }

    public class CivitaiParserFactory : IMetadataParserFactory
    {
        public IMetadataParser GetInstance() => new CivitaiA1111Parser();
    }
}
