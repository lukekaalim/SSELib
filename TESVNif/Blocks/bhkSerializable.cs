using System;
namespace SSE.TESVNif.Blocks
{
    public class bhkSerializable : bhkRefObject
    {
        public bhkSerializable(NIFReader.NIFFile file, BlockStructure.BlockData data) : base(file, data)
        {
        }
    }
}
