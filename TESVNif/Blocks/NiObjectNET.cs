using System;
using SSE.TESVNif.Structures;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Blocks
{
    public class NiObjectNET : NiObject
    {
        ReferenceString NameIndex;

        public string Name => NameIndex.Resolve(File.Header);

        public NiObjectNET(NIFReader.NIFFile file, BlockStructure.BlockData data) : base(file)
        {
            NameIndex = new ReferenceString(data.GetCompound("Name"));
        }
    }
}
