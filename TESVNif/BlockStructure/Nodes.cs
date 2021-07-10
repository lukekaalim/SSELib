using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;

namespace SSE.TESVNif.BlockStructure
{
    public abstract class Node
    {
        public string Comment { get; set; }
        public XElement SourceElement { get; set; }

        public Node(XElement element)
        {
            SourceElement = element;
        }

        public static Node Parse(XElement element)
        {
            switch (element.Name.LocalName) {
                case "niftoolsxml":
                    return new RootNode(element);
                case "version":
                    return new VersionNode(element);
                case "basic":
                    return new BasicNode(element);
                case "compound":
                    return new CompoundNode(element);
                case "add":
                    return new AddNode(element);
                default:
                    return null;
            }
        }
    }

    public class RootNode : Node
    {
        public string Version { get; set; }
        public List<Node> Nodes { get; set; }

        public RootNode(XElement element) : base(element)
        {
            Version = element.Attribute("version").Value;
            Nodes = element.Descendants()
                .Select(e => Parse(e))
                .Where(n => n != null)
                .ToList();
        }
    }

    public class VersionNode : Node
    {
        public string Num { get; set; }

        public VersionNode(XElement element) : base(element)
        {
            Num = element.Attribute("num").Value;
        }
    }

    public class BasicNode : Node
    {
        public string Name { get; set; }
        public bool Intergral { get; set; }
        public bool Countable { get; set; }
        public int Size { get; set; }

        public BasicNode(XElement element) : base(element)
        {
            Name = element.Attribute("name")?.Value;
            Intergral = element.Attribute("integral")?.Value == "true";
            Countable = element.Attribute("countable")?.Value == "true";
            Size = int.Parse(element.Attribute("size")?.Value ?? "-1");
        }
    }

    //public class EnumNode : Node { }

    //public class OptionNode : Node { }

    //public class BitflagsNode : Node { }

    public class CompoundNode : Node
    {
        public string Name { get; set; }
        public List<AddNode> Adds { get; set; }

        public CompoundNode(XElement element) : base(element)
        {
            Name = element.Attribute("name")?.Value;
            Adds = element.Descendants()
                .Select(e => Parse(e))
                .Select(n => n as AddNode)
                .Where(a => a != null)
                .ToList();
        }
    }

    public class AddNode : Node
    {
        public string Name { get; set; }
        public string Type { get; set; }

        public AddNode(XElement element) : base(element)
        {
            Name = element.Attribute("name")?.Value;
            Type = element.Attribute("type")?.Value;
        }
    }

    //public class NiObjectNode : Node { }
}
