using System;
using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class NiObjectNET : NiObject
    {
        ReferenceString NameIndex;

        public string Name => NameIndex.Resolve(File.Header);

        public NiObjectNET(NIFFile file, BlockStructure.BlockData data) : base(file)
        {
            NameIndex = new ReferenceString(data.GetCompound("Name"));
        }
    }
}
