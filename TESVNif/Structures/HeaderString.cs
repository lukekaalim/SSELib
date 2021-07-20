using System;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Structures
{
    public class ReferenceString
    {
        public int Index { get; set; }
        public ReferenceString(CompoundData data)
        {
            Index = data.GetBasic<int>("Index");
        }

        public string Resolve(Header header)
        {
            return header.Strings[Index];
        }
    }
}
