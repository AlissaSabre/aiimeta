using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace aiimeta.Formats
{
    /// <summary>Parser for SwarmUI metadata.</summary>
    public class SwarmParser : IMetadataParser
    {
        public float Priority => 3.0f;

        public void Parse(IMetadata metadata, IParsedMetadata parsed)
        {
            // SwarmUI puts its own JSON metadata in PNG Parameters or EXIF User Comment.
            if (string.IsNullOrWhiteSpace(metadata.Parameters)) return;
            var json = JsonHelper.ParseAsJObject(metadata.Parameters);
            if (json is null) return;

            if (json["sui_image_params"] is JObject image_params)
            {
                // If the JSON in Parameters has "sui_image_params",
                // it is very likely that this image was generated on SwrmUI.
                // Overwrite positive/negative prompts set by earlier parsers.
                parsed.PositivePromptText = image_params["prompt"]?.AsString();
                parsed.NegativePromptText = image_params["negativeprompt"]?.AsString();
                foreach (var property in image_params.Properties())
                {
                    parsed.Add(property.Name, property.Value?.ToString(Formatting.None) ?? string.Empty);
                }
            }

            if (json["sui_extra_data"] is JObject extra_data)
            {
                foreach (var property in extra_data.Properties())
                {
                    parsed.Add(property.Name, property.Value?.ToString(Formatting.None) ?? string.Empty);
                }
            }

            if (json["sui_models"] is JArray models)
            {
                for (int i = 0; i < models.Count; i++)
                {
                    parsed.Add($"model ({i})", models[i]?.ToString(Formatting.None) ?? string.Empty);
                }
            }
        }
    }

    public class SwarmParserFactory : IMetadataParserFactory
    {
        public IMetadataParser GetInstance() => new SwarmParser();
    }
}
