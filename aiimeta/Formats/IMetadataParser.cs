using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aiimeta.Formats
{

    public interface IMetadataParser
    {
        /// <summary>Priority of this parser.</summary>
        /// <remarks>
        /// Parsers with lower priority parse earlier,
        /// and those with higher priority can override earlier results.
        /// The <i>standard</i> A1111 parser has the priority 0.0.
        /// </remarks>
        public float Priority { get; }

        void Parse(IMetadata metadata, IParsedMetadata parsed);
    }

    public interface IMetadataParserFactory
    {
        IMetadataParser GetInstance();
    }
}
