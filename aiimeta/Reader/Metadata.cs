using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using aiimeta.Formats;

namespace aiimeta.Reader
{
    public class Metadata : IMetadata
    {
        // Image metadata (no AI specific).
        public int Width { get; set; }
        public int Height { get; set; }
        public string Format { get; set; } = string.Empty;

        // Raw AI metadata strings.
        public string? Parameters { get; set; }
        public string? ComfyPrompt { get; set; }
        public string? ComfyWorkflow { get; set; }
    }
}
