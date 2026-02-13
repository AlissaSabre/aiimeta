using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

using aiimeta.Formats;

namespace aiimeta.Reader
{
    /// <summary>Factory to create an image object from an image bytes.</summary>
    public interface IImageFactory
    {
        /// <summary>Creates an image object for an OS file.</summary>
        /// <param name="path">Full path name of the OS file.</param>
        IImageObject Create(string path);

        /// <summary>Creates an image object for a URI.</summary>
        /// <param name="uri">URI of scheme <c>http:</c>, <c>https:</c>, or <c>file:</c>.</param>
        IImageObject Create(Uri uri);

        /// <summary>Creates an image object fot a stream.</summary>
        /// <param name="stream">Stream containing the image data.</param>
        /// <param name="name">Short name of the image, e.g., a title.</param>
        /// <param name="full_name">Full name of the image, e.g., a full path name or an absolute URI.</param>
        /// <remarks>
        /// <para>
        /// This method reads the image content from the current position.
        /// It may or may not read extra bytes after the image data if any.
        /// </para>
        /// <para>
        /// This method may or may not invoke <see cref="Stream.Dispose()"/> of <paramref name="stream"/>.
        /// The caller needs to invoke it after this method returns to make sure the stream is disposed after use.
        /// At the same time, the caller can't assue it is still usable after this method returns.
        /// </para>
        /// </remarks>
        IImageObject Create(Stream stream, string name, string full_name);
    }

    /// <summary>Image object.</summary>
    public interface IImageObject : IDisposable
    {
        /// <summary>Short name of this image, e.g., a title.</summary>
        string Name { get; }

        /// <summary>Full name of this image, e.g., a full path name or an absolute URI.</summary>
        string FullName { get; }

        /// <summary>Raw image metadata.</summary>
        IMetadata Metadata { get; }

        /// <summary>Parsed image generation metadata.</summary>
        IParsedMetadata ParsedMetadata { get; }

        /// <summary>Image data suitable for preview on the screen.</summary>
        /// <returns>Memory stream containing the preview image bytes.</returns>
        /// <remarks>A preview image may have lower quality than the original.</remarks>
        MemoryStream GetPreviewStream();
    }
}
