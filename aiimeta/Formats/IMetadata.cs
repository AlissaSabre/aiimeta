using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aiimeta.Formats
{
    public interface IMetadata
    {
        // Image metadata (no AI specific).
        int Width { get; }
        int Height { get; }
        string Format { get; }

        // Raw AI metadata strings.
        string? Parameters { get; }
        string? ComfyPrompt { get; }
        string? ComfyWorkflow { get; }

    }
}
