using System;
using System.Linq;
using System.Collections.Generic;

using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class BSTriShape : NiAVObject
    {
        public List<Triangle> Triangles { get; set; }
        public List<BSVertexData> VertexData { get; set; }

        public BSTriShape(NIFReader.NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
            Triangles = data.GetCompoundList("Triangles")
                .Select(d => new Triangle(d))
                .ToList();
            VertexData = data.GetCompoundList("Vertex Data")
                .Select(d => new BSVertexData(d))
                .ToList();
        }
    }
}
