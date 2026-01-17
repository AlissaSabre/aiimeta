using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace aiimeta.Formats
{
    public class A1111Parser : IMetadataParser
    {
        private const string NegativePromptMarker = "Negative prompt:";

        private static readonly Regex NegativePromptFinder =
            new Regex("^" + NegativePromptMarker,
                RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Singleline);

        /// <summary>Matches the beginning of the non-prompt parameter block.</summary>
        /// <remarks>
        /// This regex will match multiple parts of the raw parameter string, 
        /// and the last match is the real beginning.
        /// </remarks>
        private static readonly Regex ParameterBlockFinder =
            new Regex("^[A-Z][A-Za-z0-9_ /-]*:",
                RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Singleline);

        /// <summary>Matches a single parameter key in the non-prompt parameter block.</summary>
        /// <remarks>
        /// I assume (although insecure...) that
        /// <list type="bullet">
        /// <item>A pair consists of a key followed by a colon followed by a value.</item>
        /// <item>A key is a series of alphanumeric words separated by underbars, spaces, slants, and hyphens.
        ///       In addition, a key always begins with an uppercase letter.</item>
        /// <item>A value is either
        ///       a single string enclosed in a pair of double quotes, 
        ///       a JSON string, or
        ///       a free text containing any characters except for a comma.</item>
        /// <item>Pairs are separated by a comma.</item>
        /// <item>Redundant spaces can be added anywere.</item>
        /// </list>
        /// This regex matches the key, the following colon, and any following spaces.
        /// &lt;key&gt; matches the key only.
        /// </remarks>
        private static readonly Regex ParameterKey =
            new Regex("(^|(?<=, *))(?<key>[A-Z][A-Za-z0-9_ /-]*): *", 
                RegexOptions.ExplicitCapture | RegexOptions.Multiline | RegexOptions.Singleline);

        public float Priority => 0.0f;

        public void Parse(IMetadata metadata, IParsedMetadata parsed)
        {
            var parameters = metadata.Parameters;
            if (parameters is null) return;

            // The last line _optionally_ contains the non-prompt parameters.
            var match2 = ParameterBlockFinder.Matches(parameters).LastOrDefault(Match.Empty);
            var index2 = match2.Success ? match2.Index : parameters.Length;
            if (match2.Value.StartsWith(NegativePromptMarker))
            {
                // This happens if the negative prompt is a single line (usual)
                // and no non-prompt parameters are included (rare).
                index2 = parameters.Length;
            }

            // "Negative prompt:" divides the positive and negative prompt texts.
            // It is always at the beginning of a new line.
            // I'm not sure, however, what happens if more than one such line exists...
            // The following code takes the first one, BTW.
            var match1 = NegativePromptFinder.Match(parameters, 0, index2);
            var index1 = match1.Success ? match1.Index : index2;

            // Extract the positive and negative prompt texts.
            parsed.PositivePromptText = parameters.Substring(0, index1).Trim();
            parsed.NegativePromptText = index2 > index1
                ? parameters.Substring(
                    index1 + NegativePromptMarker.Length,
                    index2 - index1 - NegativePromptMarker.Length).Trim()
                : null;

            // Analyze the parameter block.
            int end = parameters.Length;
            int index = index2;
            while (index < end)
            {
                // Find the key. This is easy.
                var match = ParameterKey.Match(parameters, index);
                if (!match.Success) break;

                // Calculate the beginning of the value. This is easy, too.
                index = match.Index + match.Length;

                // Check the _type_ of the value and handle appropriately.
                int end_of_value = -1;
                if (index < end)
                {
                    switch (parameters[index])
                    {
                        case '"':
                            // This is a double-quote enclosed text.
                            // Secretly increment the index (to skip the double quote)
                            // and find the end of the quotation.
                            end_of_value = parameters.IndexOf('"', ++index);
                            break;

                        case '{':
                        case '[':
                            // This is a JSON data.
                            // Find the end of the JSON using the parser.
                            // (We discard the resulting JSON object, BTW.)
                            try
                            {
                                (_, end_of_value) = JsonHelper.ParseRaw(parameters, index);
                            }
                            catch (JsonException)
                            {
                                // If the value string is not a valid JSON,
                                // e.g., it is a filename beginning with '[',
                                // ParseRaw throws JsonException.
                                // If it happens, handle it as a free-form text.
                                goto default;
                            }
                            break;

                        default:
                            // This is a free-form text.
                            // The value ends at the _next_ comma.
                            end_of_value = parameters.IndexOf(',', index);
                            break;
                    }
                }

                // Get the value and record the properties.
                if (end_of_value < 0) end_of_value = end;
                var value = parameters.Substring(index, end_of_value - index);
                parsed.Add(match.Groups["key"].Value, value);
                index = end_of_value;

                // If index points to a double quote now,
                // it means we have just handled a quoted string.
                // Skip it.
                if (index < end && parameters[index] == '"') ++index;
            }
        }
    }

    public class A1111ParserFactory : IMetadataParserFactory
    {
        public IMetadataParser GetInstance() => new A1111Parser();
    }
}
