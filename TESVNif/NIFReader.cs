using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using System.Xml.Linq;
using System.Threading.Tasks;

using SSE.TESVNif.BlockStructure;
using SSE.TESVNif.Blocks;
using SSE.TESVNif.Structures;

namespace SSE.TESVNif
{
    public class NIFReader
    {
        public class NIFFile
        {
            public Header Header { get; set; }
            public List<NiObject> Objects { get; set; }
            public Footer Footer { get; set; }

            public List<NiObject> Roots => Footer.Roots
                .Select(r => Objects[r])
                .ToList();
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

                    var headerString = (string)(reader.ReadSchemaByName("HeaderString") as BasicData).Value;
                    var versionString = headerString.Substring(headerString
                        .ToList()
                        .FindLastIndex(c => !(char.IsDigit(c) || char.IsWhiteSpace(c) || c == '.')) + 1);

                    memStream.Seek(0, SeekOrigin.Begin);
                    reader.Version = VersionParser.Parse(versionString.Trim());

                    var file = new NIFFile();

                    file.Header = new Header((CompoundData)reader.ReadSchemaByName("Header"));

                    reader.GlobalIdentifiers = file.Header.Globals;

                    file.Objects = file.Header.Blocks
                        .Select(block => reader.ReadSchemaByName(block.Type))
                        .Select(data => ReadNiObject(file, (BlockData)data))
                        .ToList();

                    file.Footer = new Footer((CompoundData)reader.ReadSchemaByName("Footer"));
                    return file;
                }
            }
        }

        public static NiObject ReadNiObject(NIFFile file, BlockData data)
        {
            switch (data.Name)
            {
                case "BSFadeNode":
                    return new BSFadeNode(file, data);
                case "BSInvMarker":
                    return new BSInvMarker(file, data);
                case "BSXFlags":
                    return new BSXFlags(file, data);
                case "NiStringExtraData":
                    return new NiStringExtraData(file, data);
                case "bhkConvexVerticesShape":
                    return new bhkConvexVerticesShape(file, data);
                case "bhkRigidBody":
                    return new bhkRigidBody(file, data);
                case "bhkCollisionObject":
                    return new bhkCollisionObject(file, data);
                case "BSTriShape":
                    return new BSTriShape(file, data);
                case "BSEffectShaderProperty":
                    return new BSEffectShaderProperty(file, data);
                case "NiTriShape":
                    return new NiTriShape(file, data);
                case "NiTriShapeData":
                    return new NiTriShapeData(file, data);
                case "NiAlphaProperty":
                    return new NiAlphaProperty(file, data);
                case "BSLightingShaderProperty":
                    return new BSLightingShaderProperty(file, data);
                case "BSShaderTextureSet":
                    return new BSShaderTextureSet(file, data);
                case "bhkBoxShape":
                    return new bhkBoxShape(file, data);
                case "bhkConvexTransformShape":
                    return new bhkConvexTransformShape(file, data);
                case "bhkListShape":
                    return new bhkListShape(file, data);
                default:
                    return new NiObject(file);
            }
        }
    }
}
