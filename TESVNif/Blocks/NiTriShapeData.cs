using System;
using System.Linq;
using System.Collections.Generic;

using SSE.TESVNif.Structures;

namespace SSE.TESVNif.Blocks
{
    public class NiTriShapeData : NiTriBasedGeomData
    {
        public List<Triangle> Triangles { get; set; }
        public List<Vector3> Vertices { get; set; }

        public NiTriShapeData(NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
            Triangles = data.GetCompoundList("Triangles")
                .Select(d => new Triangle(d))
                .ToList();
            Vertices = data.GetCompoundList("Vertices")
                .Select(d => new Vector3(d))
                .ToList();
        }
    }
}
