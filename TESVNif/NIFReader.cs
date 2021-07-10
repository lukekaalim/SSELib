using System;
using System.IO;
using System.Resources;
using System.Reflection;
using System.Xml.Linq;
using System.Threading.Tasks;
using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif
{
    public class NIFReader
    {
        BlockStructure.BlockStructureReader Reader { get; set; }

        public static async void Read(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var info = assembly.GetManifestResourceInfo("TESVNif.NIF.xml");
            using (var stream = assembly.GetManifestResourceStream("TESVNif.NIF.xml"))
            {
                var doc = XDocument.Load(stream);
                var reader = new BlockStructureReader(doc.Root);
            }
        }
    }
}
