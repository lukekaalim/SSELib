using System;
using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks.Besthesda
{
    public class BSInvMarker : NiExtraData
    {
        public float Zoom { get; set; }

        public BSInvMarker(NIFFile file, BlockStructure.NiObjectData data) : base(file, data)
        {
            Zoom = data.GetBasic<float>("Zoom");
        }
    }
}
