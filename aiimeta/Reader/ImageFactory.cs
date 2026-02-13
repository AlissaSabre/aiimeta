using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

using aiimeta.Formats;

namespace aiimeta.Reader
{
    public class ImageFactory : IImageFactory
    {
        private readonly IMetadataReader MetadataReader;

        private readonly IMetadataParser MetadataParser;

        private readonly HttpClient HttpClient;

        public ImageFactory(IMetadataReader reader, IMetadataParser parser, HttpClient http)
        {
            MetadataReader = reader;
            MetadataParser = parser;
            HttpClient = http;
        }

        /// <summary>Property key for the basic image metadata.</summary>
        public string ImageKey { get; set; } = "Image";

        /// <summary>Format string for the basic image metadata.</summary>
        /// <remarks>
        /// This string is fed to <see cref="string.Format(string, object?)"/>
        /// with three more arguments.
        /// <list type="bullet">
        /// <item><c>{0}</c> is a <see cref="string"/> value representing an image file format,</item>
        /// <item><c>{1}</c> is an <see cref="int"/> value for the width, and</item>
        /// <item><c>{2}</c> is an <see cref="int"/> value for the height.</item>
        /// </list>
        /// </remarks>
        public string ImageFormat { get; set; } = "{0}, {1} × {2}";

        /// <summary>Maximum width of the preview image.</summary>
        public double MaxPreviewWidth { get; set; } = int.MaxValue;

        /// <summary>Maximum height of the preview image.</summary>
        public double MaxPreviewHeight { get; set; } = int.MaxValue;

        /// <summary>Creates an image object for an OS file.</summary>
        /// <param name="path">Full path name of the OS file.</param>
        public IImageObject Create(string path)
        {
            var image = Image.Load(path);
            var name = Path.GetFileName(path);
            return new ImageObject(this, image, name, path);
        }

        /// <summary>Creates an image object for a URI.</summary>
        /// <param name="uri">URI of scheme <c>http:</c>, <c>https:</c>, or <c>file:</c>.</param>
        public IImageObject Create(Uri uri)
        {
            if (uri.IsFile)
            {
                // Just in case.
                return Create(uri.LocalPath);
            }

            var stream = HttpClient.GetStreamAsync(uri).Result;
            var image = Image.Load(stream);
            var name = Path.GetFileName(uri.LocalPath);
            return new ImageObject(this, image, name, uri.ToString());
        }

        /// <summary>Creates an image object fot a stream.</summary>
        /// <param name="stream"></param>
        /// <remarks>
        /// This method reads the entire contents of the <paramref name="stream"/> before return.
        /// It is a responsibility of the caller to invoke its <see cref="Stream.Dispose()"/>.
        /// </remarks>
        public IImageObject Create(Stream stream, string name, string full_name)
        {
            var image = Image.Load(stream);
            return new ImageObject(this, image, name, full_name);
        }

        protected Metadata GetMetadata(Image image)
        {
            return MetadataReader.Read(image);
        }

        protected ParsedMetadata GetParsedMetadata(IMetadata metadata)
        {
            var parsed = new ParsedMetadata();
            parsed.Add(ImageKey, string.Format(ImageFormat, 
                metadata.Format, metadata.Width, metadata.Height));
            MetadataParser.Parse(metadata, parsed);
            return parsed;
        }

        protected MemoryStream GetPreviewStream(Image image)
        {
            // Resize the image if it is too large.
            var i = image;
            if (image.Width > MaxPreviewWidth || image.Height > MaxPreviewHeight)
            {
                var wratio = MaxPreviewWidth / image.Width;
                var hratio = MaxPreviewHeight / image.Height;
                var (w, h) = wratio < hratio
                    ? ((int)(image.Width * wratio), 0)
                    : (0, (int)(image.Height * hratio));
                i = image.Clone(x => x.Resize(w, h));
            }

            // Create and return a PNG image stream.
            var stream = new MemoryStream();
            i.SaveAsPng(stream);
            stream.Position = 0;
            return stream;
        }

        protected class ImageObject : IImageObject
        {
            private readonly ImageFactory Factory;

            private readonly Image Image;

            public string Name { get; }

            public string FullName { get; }

            private IMetadata? _Metadata = null;

            public IMetadata Metadata => _Metadata ?? (_Metadata = Factory.GetMetadata(Image));

            private IParsedMetadata? _ParsedMetadata = null;

            public IParsedMetadata ParsedMetadata => _ParsedMetadata ?? (_ParsedMetadata = Factory.GetParsedMetadata(Metadata));

            public ImageObject(ImageFactory factory, Image image, string name, string full_name)
            {
                Factory = factory;
                Image = image;
                Name = name;
                FullName = full_name;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Image.Dispose();
                }
            }

            public void Dispose()
            {
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }

            public MemoryStream GetPreviewStream() => Factory.GetPreviewStream(Image);
        }
    }
}
