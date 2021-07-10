using System;
using System.Xml.Linq;

namespace SSE.TESVNif.BlockStructure
{
    /// <summary>
    /// "Block Structure" is a document that describes a binary type.
    /// I think it used to just be _the_ "nif.xml", but is now
    /// its own independant system.
    /// https://github.com/niftools/nifxml/wiki
    /// </summary>
    public class BlockStructureReader
    {
        public BlockStructureReader(XElement rootElement)
        {
            var node = new RootNode(rootElement);
        }
    }
}
