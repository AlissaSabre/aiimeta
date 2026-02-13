using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using SixLabors.ImageSharp;

namespace aiimeta.Reader
{
    public interface IMetadataReader
    {
        Metadata Read(Uri uri);

        Metadata Read(string path);

        Metadata Read(Stream stream);

        Metadata Read(Image image);
    }
}
