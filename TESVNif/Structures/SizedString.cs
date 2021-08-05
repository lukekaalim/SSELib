using System;
using BlockStructure;

namespace SSE.TESVNif.Structures
{
    public static class CharList
    {
        public static string ReadString(Data data)
        {
            var compound = (CompoundData)data;
            var charArray = compound
                .GetBasicList<char>("Value")
                .ToArray();
            return new string(charArray);
        }
    }
}
