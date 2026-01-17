using aiimeta.Reader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace aiimeta.Formats
{
    public class ComfyParser : IMetadataParser
    {
        public float Priority => 10.0f;

        /// <summary>List class names of known non-output nodes.</summary>
        private static string[] NonOutputNodes =
        {
            // TODO
        };

        /// <summary>Represents a Comfy node in the prompt JSON.</summary>
        private class Node
        {
            /// <summary>Raw JSON object of this Node.</summary>
            /// <remarks>This is for debugging.</remarks>
            public readonly JObject Json;

            public readonly string Id;
            public readonly string ClassType;
            public readonly string Title;
            public readonly Dictionary<string, string> Parameters;
            public readonly Dictionary<string, Node> Links;

            private Node(JProperty property)
            {
                var json = (JObject)property.Value;
                Json = json;

                Id = property.Name;
                ClassType = (string?)json["class_type"] ?? throw new JsonException($"{GetType()}: Missing class_type.");
                Title = (string?)json["_meta"]?["title"] ?? ClassType;
                
                // Extract the input names and vlaues that are specified on the node.
                var parameters = new Dictionary<string, string>();
                if (json["inputs"] is JObject inputs)
                {
                    foreach (var p in inputs.Properties())
                    {
                        switch (p.Value.Type)
                        {
                            case JTokenType.String:
                            case JTokenType.Boolean:
                            case JTokenType.Float:
                            case JTokenType.Integer:
                                parameters.Add(p.Name, p.Value.ToString());
                                break;
                            case JTokenType.Array:
                                // Will be patched later.
                                break;
                            case JTokenType.Object:
                            case JTokenType.Null:
                                // Malformed input; though we have no good way to indicate it.
                                // Just ignore it for the moment.
                                break;
                            default:
                                // Other JTokenType should never occur here.
                                throw new ApplicationException("Internal error.");
                        }
                    }
                }
                Parameters = parameters;

                // Information on the input pins are patched later.
                Links = new Dictionary<string, Node>();
            }

            /// <summary>Patches the Links dictionary of all nodes.</summary>
            /// <param name="nodes">Dictionary of all Node objects in use.</param>
            private static void PatchLinks(IDictionary<string, Node> nodes)
            {
                foreach (var node in nodes.Values)
                {
                    if (node.Json["inputs"] is JObject inputs)
                    {
                        foreach (var p in inputs.Properties())
                        {
                            if (p.Value is JArray a
                                && a.Count > 0
                                && nodes.TryGetValue((string)a[0]!, out var n))
                            {
                                node.Links.Add(p.Name, n);
                            }
                        }
                    }
                }
            }

            /// <summary>Parse a Comfy prompt string into a Python-like dictionary.</summary>
            /// <param name="prompt">Comfy prompt string.</param>
            /// <returns>Dictionary of node ID to node mapping, or null if the parsing failed.</returns>
            public static IDictionary<string, Node>? Parse(string? prompt)
            {
                // If the whole prompt is not a JSON object, the parsing fails.
                if (string.IsNullOrWhiteSpace(prompt)) return null;
                JObject json;
                try
                {
                    json = JObject.Parse(prompt);
                }
                catch (JsonException)
                {
                    return null;
                }

                // Try to parse each property as a Comfy prompt node.
                var nodes = new Dictionary<string, Node>(json.Count);
                foreach (var property in json.Properties())
                {
                    // If a parse error occured for a node, just ignore it, and keep going.
                    try
                    {
                        nodes[property.Name] = new Node(property);
                    }
                    catch (JsonException) { }
                    catch (InvalidCastException) { }
                }

                // If we got no nodes at all, report that the parsing failed,
                // because such an empty dictionary gives no metadata anyway.
                if (nodes.Count == 0) return null;

                // Patch the links and return the dictionary.
                PatchLinks(nodes);
                return nodes;
            }
        }

        private class NodePool
        {
            private readonly Queue<Node> Nodes = new Queue<Node>();
            private readonly HashSet<string> Seen = new HashSet<string>();

            public bool IsEmpty => Nodes.Count == 0;

            public Node Get()
            {
                return Nodes.Dequeue();
            }

            public void Add(Node node)
            {
                if (Seen.Add(node.Id))
                {
                    Nodes.Enqueue(node);
                }
            }

            public void Ignore(Node node)
            {
                Seen.Add(node.Id);

                // Ignore may be called _after_ the node in question is added.
                // We need to remove it.
                // It takes some overhead.
                // We might need to redesign the overall data structure of this class. FIXME.
                var id = node.Id;
                var count = Nodes.Count;
                for (int i = 0; i < count; i++)
                {
                    var n = Nodes.Dequeue();
                    if (n.Id != id) Nodes.Enqueue(n);
                }
            }
        }

        private class NodeTypeData
        {
            private class NodeTypeInfo
            {
                /// <summary>Mapping from an input name to its Comfy data type name.</summary>
                [JsonProperty("input")]
                public IDictionary<string, string> Input = new Dictionary<string, string>();

                /// <summary>Whether this is an output node.</summary>
                /// <remarks>We assume an unknown node is not an output node.</remarks>
                [JsonProperty("output_node")]
                public bool IsOutputNode = false;

                public NodeTypeInfo DeepClone()
                {
                    return new NodeTypeInfo
                    {
                        Input = Input.ToDictionary(),
                        IsOutputNode = IsOutputNode,
                    };
                }
            }

            private const string MasterDictionaryName = "ComfyNodeTypeData.json";

            private static IDictionary<string, NodeTypeInfo>? MasterDictionary = null;

            public static NodeTypeData Create(string? workflow)
            {
                // Read the master dictionary if not yet.
                if (MasterDictionary is null)
                {
                    var path = Path.Combine(
                        Path.GetDirectoryName(typeof(ComfyParser).Assembly.Location)!,
                        MasterDictionaryName);
                    using var reader = new StreamReader(path, new UTF8Encoding(false));
                    using var json = new JsonTextReader(reader);
                    MasterDictionary = new JsonSerializer().Deserialize<IDictionary<string, NodeTypeInfo>>(json)!;
                }

                // Try to parse the workflow, and return the result merged with the master dictionary.
                // If we have no usable workflow, just use the master dictionary only.
                if (string.IsNullOrWhiteSpace(workflow))
                {
                    return new NodeTypeData();
                }
                try
                {
                    return new NodeTypeData(workflow);
                }
                catch (JsonException)
                {
                    return new NodeTypeData();
                }
            }

            private readonly IDictionary<string, NodeTypeInfo> Dict;

            private NodeTypeData()
            {
                Dict = MasterDictionary!;
            }

            private NodeTypeData(string workflow)
            {
                // Each NodeTypeData instance holds a complete copy of the master dictionary.
                // Is it the right thing to do? FIXME.
                var dict = MasterDictionary!
                    .Select(p => KeyValuePair.Create(p.Key, p.Value.DeepClone()))
                    .ToDictionary();

                var json = JObject.Parse(workflow);
                if (json["nodes"] is JArray nodes)
                {
                    foreach (var node in nodes.OfType<JObject>())
                    {
                        var class_type = (string?)node["type"];
                        if (class_type is null) continue;

                        var inputs = node["inputs"] as JArray;
                        if (inputs is null) continue;

                        if (!dict.TryGetValue(class_type, out var info))
                        {
                            info = new NodeTypeInfo();
                            dict.Add(class_type, info);
                        }

                        foreach (var input in inputs.OfType<JObject>())
                        {
                            var name = (string?)input["name"];
                            if (name is null) continue;

                            var type = (string?)input["type"];
                            if (type is null) continue;

                            info.Input[name] = type;
                        }
                    }
                }
                Dict = dict;
            }

            /// <summary>Gets the data type of an input pin of a node.</summary>
            /// <param name="node"><see cref="Node"/> as read from prompt.</param>
            /// <param name="name">Name of an input pin, such as "positive".</param>
            /// <returns>data type name, such as "CONDITIONING", or null if it is unknown.</returns>
            public string? GetInputDataType(Node node, string name)
            {
                string? type = null;
                _ = Dict.TryGetValue(node.ClassType, out var info) &&
                    info.Input.TryGetValue(name, out type);
                return type;
            }
        }

        private IEnumerable<string> FindPromptText(IDictionary<string, Node> nodes, NodeTypeData node_data, string direction)
        {
            // Find nodes that takes a conditioning of the specified direction as input,
            // and put their linked nodes to the pool for further investigation.
            var pool = new NodePool();
            foreach (var node in nodes.Values)
            {
                if (node_data.GetInputDataType(node, direction) == "CONDITIONING"
                    && node.Links.TryGetValue(direction, out var linked)
                    && linked is not null)
                {
                    pool.Ignore(node);
                    pool.Add(linked);
                }
            }

            // Trace the network to find the text encode node.
            var list = new List<string>();
            while (!pool.IsEmpty)
            {
                var node = pool.Get();
                
                // Does this node look like a "CLIP Text Encode (Prompt)" node?
                if (node.Links.ContainsKey("clip") &&
                    node.Parameters.TryGetValue("text", out var text))
                {
                    // Yes. Collect the text.
                    list.Add(text);
                    continue;
                }

                // If the text is provided by another node, it is not considered.
                // FIXME.

                // "Conditioning Zero Out" is an exception
                // to the conditioning tracking criteria below.
                // It is a special conditioning-modification node that
                // takes a CONDITIONING input to know the shapes of tensors,
                // and its output value has nothing to do with the value of the
                // input conditioning (ultimately created by a positive or negative
                // prompt text).
                // Its common use is to take a positive prompt conditioning and
                // creates a zero negative prompt.
                // We should not trace the conditioning network beyond this point.
                if (node.ClassType == "ConditioningZeroOut") continue;

                // Does this node has an input pin of type CONDITIONING?
                // If so, this node is likely a conditioning modification node,
                // such as "Conditioning (Set Mask)". 
                // A node holding a prompt text will be somewhere before this one.
                // Add the linked nodes to the pool for further investigation.
                // Note that some conditioning modification nodes, e.g.
                // "Apply ControlNet", have two CONDITIONING inputs, one for
                // postive and another for negative.
                // Such inputs are usually labeled "positive" and "negative",
                // so
                foreach (var name in node.Links.Keys)
                {
                    if (node_data.GetInputDataType(node, name) == "CONDITIONING")
                    {
                        pool.Add(node.Links[name]);
                    }
                }
            }

            return list;
        }

        private static void AddTextProperties(string name, IEnumerable<string> texts, IParsedMetadata parsed)
        {
            int count = 0;
            foreach (var item in texts)
            {
                parsed.Add($"{name} ({count++})", item);
            }
        }

        private static readonly string PromptTextJoiner =
            Environment.NewLine + "---" + Environment.NewLine;

        public void Parse(IMetadata metadata, IParsedMetadata parsed)
        {
            // Parse the prompt JSON string to get the node dictionary.
            // It is usually detected in MetadataReader and recorded in
            // IMetadata.ComfyPrompt, but it may be in IMetadata.Parameters, too.
            var nodes = Node.Parse(metadata.ComfyPrompt) ?? Node.Parse(metadata.Parameters);
            if (nodes == null) return;

            // We need the type info included in the workflow to extract prompt texts.
            var node_data = NodeTypeData.Create(metadata.ComfyWorkflow);
            
            // Find the positive and negative prompt texts in the network,
            // and add them to the parsed metadata.
            var positives = FindPromptText(nodes, node_data, "positive");
            AddTextProperties("Positive", positives, parsed);
            parsed.PositivePromptText ??= string.Join(PromptTextJoiner, positives.Distinct());
            
            var negatives = FindPromptText(nodes, node_data, "negative");
            AddTextProperties("Negative", negatives, parsed);
            parsed.NegativePromptText ??= string.Join(PromptTextJoiner, negatives.Distinct());

            // Add the node info to the parsed metadata.
            foreach (var node in nodes.Values)
            {
                var label = $"Node ({node.Id})";
                var value = node.Title + ": " +
                    string.Join(", ", node.Parameters.Select(pair => $"{pair.Key}=\"{pair.Value}\""));
                parsed.Add(label, value);
            }
        }
    }

    public class ComfyParserFactory : IMetadataParserFactory
    {
        public IMetadataParser GetInstance() => new ComfyParser();
    }
}
