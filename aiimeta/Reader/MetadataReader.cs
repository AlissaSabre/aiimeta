using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace aiimeta.Reader
{
    public class MetadataReader : IMetadataReader
    {
        public Metadata Read(Image image)
        { 
            var metadata = new Metadata();

            // Fill image metadata.
            metadata.Width = image.Width;
            metadata.Height = image.Height;
            metadata.Format = image.Metadata.DecodedImageFormat?.Name ?? "(unknown)";

            // AI generation metadata are either in PNG text chunks or EXIF descriptors.
            var png = image.Metadata.GetPngMetadata().TextData;
            var exif = image.Metadata.ExifProfile;

            // Fetch raw metadata strings.
            metadata.Parameters =
                png?.GetValue("Parameters") ??
                exif?.GetValue(ExifTag.UserComment);
            metadata.ComfyWorkflow =
                png?.GetValue("Workflow") ??
                exif?.GetValue(ExifTag.ImageDescription, "Workflow:") ??
                exif?.GetValue(ExifTag.Make, "Workflow:");
            metadata.ComfyPrompt =
                png?.GetValue("Prompt") ??
                exif?.GetValue(ExifTag.Model, "Prompt:");

            return metadata;
        }
    }

    /// <summary>Set of extension methods for handling metadata.</summary>
    /// <remarks>
    /// This class is only used by <see cref="MetadataReader"/>,
    /// but C# language specification requires it being a public class. :(
    /// </remarks>
    public static class MetadataHelpers
    {
        /// <summary>Gets the value of a PNG textural chunk of the specified keyword.</summary>
        /// <param name="list"><see cref="PngMetadata.TextData"/> value.</param>
        /// <param name="keyword">Keyword of the chunk.</param>
        /// <returns>Textural value of the chunk, or null if no chunk of the specified keyword exists.</returns>
        /// <remarks>If there is more than one text chunk</remarks>
        public static string? GetValue(this IList<PngTextData> list, string keyword)
        {
            return list.TryGetValue(keyword, out var value) ? value : null;
        }

        public static bool TryGetValue(this IList<PngTextData> list, string keyword, out string value)
        {
            foreach (var data in list)
            {
                if (string.Equals(data.Keyword, keyword, StringComparison.InvariantCultureIgnoreCase))
                {
                    value = data.Value;
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }

        public static string? GetValue(this ExifProfile exif, ExifTag<string> tag)
        {
            return exif.TryGetValue(tag, out var value) ? value.Value : null;
        }

        public static string? GetValue(this ExifProfile exif, ExifTag<string> tag, string prefix)
        {
            return exif.TryGetValue(tag, out var value)
                && value.Value?.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase) == true
                ? value.Value.Substring(prefix.Length)
                : null;
        }

        public static string? GetValue(this ExifProfile exif, ExifTag<EncodedString> tag)
        {
            if (!exif.TryGetValue(tag, out var value)) return null;
            var s = value.Value.Text;
            if (string.IsNullOrEmpty(s)) return string.Empty;

            // We sometimes get a byte-swapped Unicode (i.e., UTF-16) data in EncodedString.
            // ImageSharp issue #2906 claims that some earlier versions of the library had a bug
            // on handling the byte order of Unicode string, that the bug was fixed in 20250611,
            // and that the fix was included in the version 3.1.10.
            // However, I still experience the byte swap issue when using ImageSharp 3.1.13.
            // I don't know why.
            // As a workaround, let's try to detect the case and swap bytes if required.
            if (s[0] == 0xFFFE ||
                s.Count(c => (c & 0x00FF) == 0) > s.Count(c => (c & 0xFF00) == 0))
            {
                s = new string(s.Select(c => (char)((c << 8) | (c >> 8))).ToArray());
            }

            // As a bonus, we remove the BOM if any.
            if (s[0] == 0xFEFF) s = s.Substring(1);

            return s;               
        }
    }
}
