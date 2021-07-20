using System;

namespace SSE.TESVNif.Blocks
{
    public class NiObject
    {
        public NIFReader.NIFFile File { get; set; }

        public NiObject(NIFReader.NIFFile file)
        {
            File = file;
        }
    }
}
