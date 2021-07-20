using System;
using System.Linq;
using System.Collections.Generic;
using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class NiNode : NiAVObject
    {
        List<int> childReferences;

        public List<NiObject> Children => childReferences
            .Select(r => File.Objects[r])
            .ToList();

        public NiNode(NIFReader.NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
            childReferences = data.GetBasicList<int>("Children");
        }
    }
}
