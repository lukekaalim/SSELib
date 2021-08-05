using System;
namespace SSE.TESVNif.Blocks
{
    public class NiGeometry : NiAVObject
    {
        int dataIndex;

        public NiGeometryData Data => File.Objects[dataIndex] as NiGeometryData;

        public NiGeometry(NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
            dataIndex = data.GetBasic<int>("Data");
        }
    }
}
