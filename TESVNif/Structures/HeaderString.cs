using System;

namespace SSE.TESVNif.Structures
{
    public class ReferenceString
    {
        public int Index { get; set; }
        public ReferenceString(BlockStructure.CompoundData data)
        {
            Index = data.GetBasic<int>("Index");
        }

        public string Resolve(Header header)
        {
            return header.Strings[Index];
        }
    }
}
