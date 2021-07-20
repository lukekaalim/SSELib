using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using System.Xml.Linq;
using System.Threading.Tasks;

using SSE.TESVNif.BlockStructure;

namespace SSE.TESVNif
{
    public class NIFReader
    {
        public class NIFFile
        {
            public CompoundBlock Header { get; set; }
            public List<Block> Blocks { get; set; }
            public CompoundBlock Footer { get; set; }
        }

        BlockStructureReader Reader { get; set; }

        public static async Task<NIFFile> Read(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resources = assembly.GetManifestResourceNames();
            var info = assembly.GetManifestResourceInfo("SSE.TESVNif.NIF.xml");
            using (var stream = assembly.GetManifestResourceStream("SSE.TESVNif.NIF.xml"))
            {
                var doc = XDocument.Load(stream);
                var nifFile = new FileInfo(filename);
                using (var nifStream = nifFile.OpenRead())
                {
                    var nifBytes = new byte[nifStream.Length];
                    await nifStream.ReadAsync(nifBytes, 0, nifBytes.Length);
                    var memStream = new MemoryStream(nifBytes);
                    var reader = new BlockStructureReader(doc, memStream);

                    var headerString = (string)(reader.ReadSchemaByName("HeaderString") as BasicBlock).Value;
                    var versionString = headerString.Substring(headerString
                        .ToList()
                        .FindLastIndex(c => !(char.IsDigit(c) || char.IsWhiteSpace(c) || c == '.')) + 1);

                    memStream.Seek(0, SeekOrigin.Begin);
                    reader.Version = VersionParser.Parse(versionString.Trim());

                    var header = reader.ReadSchemaByName("Header") as CompoundBlock;

                    var blockTypes = (header.Fields["Block Types"] as ListBlock)
                        .Contents
                        .Select(r => ReadSizedString(r))
                        .ToList();
                    var blockIndicies = (header.Fields["Block Type Index"] as ListBlock)
                        .Contents
                        .Select(r => (short)(r as BasicBlock).Value)
                        .ToList();
                    var blocks = blockIndicies.Select(i => blockTypes[i]).ToList();
                    var globals = new Dictionary<string, BlockStructure.Logic.Value>()
                    {
                        { "Version", BlockStructure.Logic.Value.From(header.Fields["Version"]) },
                        { "User Version", BlockStructure.Logic.Value.From(header.Fields["User Version"]) },
                        { "BS Header", BlockStructure.Logic.Value.From(header.Fields["BS Header"]) },
                    };
                    reader.GlobalIdentifiers = globals;

                    var children = new List<Block>();
                    foreach (var block in blocks)
                        children.Add(reader.ReadSchemaByName(block));
                    var footer = reader.ReadSchemaByName("Footer") as CompoundBlock;

                    return new NIFFile()
                    {
                        Header = header,
                        Blocks = children,
                        Footer = footer
                    };
                }
            }
        }

        public static string ReadSizedString(Block result)
        {
            if (result is CompoundBlock compound)
                if (compound.Fields["Value"] is ListBlock charList)
                    return new string(
                        charList.Contents
                            .Select(c => (char)(c as BasicBlock).Value)
                            .ToArray()
                    );
            throw new Exception();
        }
    }
}
