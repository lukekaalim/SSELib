using System;

namespace SSE.TESVNif.Blocks
{
    public class NiObject
    {
        public NIFFile File { get; set; }

        public NiObject(NIFFile file)
        {
            File = file;
        }
    }
}
