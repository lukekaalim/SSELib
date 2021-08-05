using System;
using System.Linq;
using System.Collections.Generic;
using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class NiAVObject : NiObjectNET
    {
        public Vector3 Translate { get; set; }

        public NiAVObject(NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
            Translate = new Vector3(data.GetCompound("Translation"));
        }
    }
}
