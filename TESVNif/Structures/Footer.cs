using System;
using System.Collections.Generic;

namespace SSE.TESVNif.Structures
{
    public class Footer
    {
        public List<int> Roots { get; set; }

        public Footer(BlockStructure.CompoundData data)
        {
            Roots = data.GetBasicList<int>("Roots");
        }
    }
}
