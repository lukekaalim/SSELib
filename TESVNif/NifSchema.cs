using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using System.Xml.Linq;
using System.Threading.Tasks;

using BlockStructure;
using BlockStructure.Schemas;


namespace SSE.TESVNif
{
    public static class NifSchema
    {
        public static SchemaDocument LoadEmbedded()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var sourceStream = assembly.GetManifestResourceStream("SSE.TESVNif.NIF.xml"))
            {
                var xDocument = XDocument.Load(sourceStream);
                return new SchemaDocument(xDocument.Root);
            }
        }
    }
}
