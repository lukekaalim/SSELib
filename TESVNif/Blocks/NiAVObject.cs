using System;
using System.Linq;
using System.Collections.Generic;
using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class NiAVObject : NiObjectNET
    {
        public Vector3 Translate { get; set; }
        public Matrix3x3 Rotation { get; set; }
        public float Scale { get; set; }

        public NiAVObject(NIFFile file, BlockStructure.NiObjectData data) : base(file, data)
        {
            Translate = new Vector3(data.GetCompound("Translation"));
            Rotation = new Matrix3x3(data.GetCompound("Rotation"));
            Scale = data.GetBasic<float>("Scale");
        }
    }
}
