using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using aiimeta.Formats;

namespace aiimeta.Reader
{

    public class AggregateMetadataParser : IMetadataParser
    {
        /// <summary>Throws NotImplementedException if got.</summary>
        public float Priority => throw new NotImplementedException();

        private readonly List<IMetadataParser> Parsers = new List<IMetadataParser>();

        public AggregateMetadataParser()
        {
            // Search the assembly for factories to create parser instances.
            foreach (var t in GetType().Assembly.GetTypes())
            {
                if (t.IsClass &&
                    typeof(IMetadataParserFactory).IsAssignableFrom(t))
                {
                    try
                    {
                        if (Activator.CreateInstance(t) is IMetadataParserFactory factory)
                        {
                            Parsers.Add(factory.GetInstance());
                        }
                    }
                    catch (Exception) { }
                }
            }

            // Sort the parsers by their priority,
            // so that our Parse invokes lower priority parsers first.
            Parsers.Sort((x, y) => Comparer<float>.Default.Compare(x.Priority, y.Priority));
        }

        public void Parse(IMetadata metadata, IParsedMetadata parsed)
        {
            foreach (var parser in Parsers)
            {
                parser.Parse(metadata, parsed);
            }
        }
    }
}
