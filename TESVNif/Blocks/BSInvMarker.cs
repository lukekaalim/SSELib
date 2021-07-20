using System;
using SSE.TESVNif.Structures;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Blocks
{
    public class BSInvMarker : NiExtraData
    {
        public float Zoom { get; set; }

        public BSInvMarker(NIFReader.NIFFile file, BlockData data) : base(file, data)
        {
            Zoom = data.GetBasic<float>("Zoom");
        }
    }
}
