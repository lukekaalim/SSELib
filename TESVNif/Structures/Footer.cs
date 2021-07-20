using System;
using System.Collections.Generic;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif.Structures
{
    public class Footer
    {
        public List<int> Roots { get; set; }

        public Footer(CompoundData data)
        {
            Roots = data.GetBasicList<int>("Roots");
        }
    }
}
