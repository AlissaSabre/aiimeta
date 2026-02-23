using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using SixLabors.ImageSharp;

namespace aiimeta.Reader
{
    /// <summary>Reads raw image metadata out of the ImageSharp.Image.</summary>
    public interface IMetadataReader
    {
        /// <summary>Reads raw image metadata out of the ImageSharp.Image.</summary>
        Metadata Read(Image image);
    }
}
