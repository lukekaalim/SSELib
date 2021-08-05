using System;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SSE.TESVPlugin;
using SSE.TESVPlugin.Reader;
using SSE.TESVRecord;
using SSE.TESVArchive;
using SSE.TESVNif;

namespace SSECLI
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var info = new FileInfo("/Users/lukekaalim/projects/SSE-Data-Lib/TestData/Skyrim - Meshes0.bsa");
                using var stream = info.OpenRead();
                var archive = await ArchiveStreamReader.Load(stream);

                var record = archive.FileRecordBlocks.ToList()[2].FileRecords.First();
                var file = await archive.ReadFile(record);
                using var nifStream = new MemoryStream(file);
                //var nif = NIFReader.Read(nifStream);
            } catch (Exception error)
            {
                Console.WriteLine("There was an unexpected error");
                Console.WriteLine(error.Message);
                Console.WriteLine(error.StackTrace);
                return;
            }
        }
    }
}
