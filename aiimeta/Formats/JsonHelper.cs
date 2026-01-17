using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace aiimeta.Formats
{
    /// <summary>Static helper methods for use with Newtonsoft.JSON.</summary>
    public static class JsonHelper
    {
        /// <summary>Parse the JSON data at the middle of a string and returns the end index of JSON data.</summary>
        /// <param name="input">String containing the JSON data.</param>
        /// <param name="start_index">Index in <paramref name="input"/> of the beginning of the JSON data.</param>
        /// <returns>
        /// Tuple of the following:
        /// <list type="number">
        /// <item>JToken value representing the parsed JSON data.</item>
        /// <item>Index in <paramref name="input"/> to point to the next character after the JSON.</item>
        /// </list>
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="start_index"/> is less than 0, 
        /// or it is equal to or greater than the length of <paramref name="input"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="input"/> contains a newline character in the middle of JSON data.
        /// </exception>
        /// <exception cref="JsonException">
        /// Substring in <paramref name="input"/> at <paramref name="start_index"/> 
        /// is not a valid JSON data.
        /// </exception>
        /// <remarks>This only works for a single-line JSON, i.e., no '\n' in the middle.</remarks>
        public static (JToken, int) ParseRaw(string input, int start_index)
        {
            if (start_index < 0 || start_index >= input.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start_index));
            }
            using var reader = new JsonTextReader(new StringReader(input.Substring(start_index)))
            {
                SupportMultipleContent = true
            };
            var token = JToken.ReadFrom(reader);
            if (reader.LineNumber > 1)
            {
                // This means the JSON reader has skipped one or more newlines during the processing.
                // (LineNumber is base 1.)
                // We can't calculate the end_index if it happens.
                throw new ArgumentException("Can't handle JSON data containing newlines.", nameof(input));
            }
            // LinePosition is base 1,
            // and it points to the last character in JSON data
            // rather than the next character after the JSON data.
            var end_index = start_index + reader.LinePosition;
            return (token, end_index);
        }

        /// <summary>Parse a JSON string as a JSON object without throwing exception.</summary>
        /// <param name="input">Input string.</param>
        /// <returns>JObject if successfully parsed. Null otherwise.</returns>
        public static JObject? ParseAsJObject(this string input)
        {
            if (input is null) return null;
            try
            {
                return JObject.Parse(input);
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>Returns the corresponding .NET string if a token is a JOSN string.</summary>
        /// <param name="token">Any JSON token.</param>
        /// <returns>.NET string, or null if the <paramref name="token"/> is not a JSON string.</returns>
        public static string? AsString(this JToken token)
            => token?.Type == JTokenType.String ? (string?)token : null;

        /// <summary>Gets an element from a JArray without throwing Exception.</summary>
        /// <param name="token">Any JSON token.</param>
        /// <param name="index">Array index.</param>
        /// <returns>
        /// Array element at <paramref name="index"/> 
        /// if <paramref name="token"/> is a JSON array and <paramref name="index"/> is valid.
        /// Null if <paramref name="token"/> is not a JSON array or <paramref name="index"/> is out of range.
        /// </returns>
        public static JToken? At(this JToken token, int index)
        {
            if (token?.Type != JTokenType.Array) return null;
            var array = (JArray)token;
            if (index < 0 || index >= array.Count) return null;
            return array[index];
        }
    }
}
