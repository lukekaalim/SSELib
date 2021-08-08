using System;
using System.Linq;
using System.Collections.Generic;

using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class NiExtraData : NiObject
    {
        ReferenceString nameRef;

        public string Name => nameRef.Resolve(File.Header);

        public NiExtraData(NIFFile file, BlockStructure.NiObjectData data) : base(file)
        {
            nameRef = new ReferenceString(data.TryGetCompound("Name"));
        }
    }
}
