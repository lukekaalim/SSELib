using System;

namespace SSE.TESVNif.Structures
{
    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3(BlockStructure.CompoundData data)
        {
            X = data.GetBasic<float>("x");
            Y = data.GetBasic<float>("y");
            Z = data.GetBasic<float>("z");
        }
    }

    public class ByteVector3
    {
        public byte X { get; set; }
        public byte Y { get; set; }
        public byte Z { get; set; }

        public ByteVector3(BlockStructure.CompoundData data)
        {
            X = data.GetBasic<byte>("x");
            Y = data.GetBasic<byte>("y");
            Z = data.GetBasic<byte>("z");
        }
    }
}
