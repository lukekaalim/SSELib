using System;
using System.Linq;
using System.Collections.Generic;

using SSE.TESVNif.BlockStructure;
using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class NiExtraData : NiObject
    {
        ReferenceString nameRef;

        public string Name => nameRef.Resolve(File.Header);

        public NiExtraData(NIFReader.NIFFile file, BlockData data) : base(file)
        {
            nameRef = new ReferenceString(data.GetCompound("Name"));
        }
    }
}
