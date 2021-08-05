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
using BlockStructure.Precomputed;

using SSE.TESVNif.Blocks;
using SSE.TESVNif.Structures;

using SSE.TESVNif.Blocks.Besthesda;
using SSE.TESVNif.Blocks.Havok;

using static SSE.TESVNif.Structures.Header;

namespace SSE.TESVNif
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

    public class NifSchema
    {
        public XDocument Document { get; set; }
        public SchemaDocument Schema { get; set; }
        public Dictionary<VersionKey, PDocumentSchema> SchemasByVersion { get; set; }

        public static NifSchema LoadEmbedded()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var docStream = assembly.GetManifestResourceStream("SSE.TESVNif.NIF.xml"))
            {
                var doc = XDocument.Load(docStream);
                var schema = new SchemaDocument(doc.Root);
                var schemas = schema.Versions
                    .SelectMany(v => v.GetVersionKeys())
                    .Select(k => schema.Precompute(k))
                    .ToDictionary(p => p.Key);

                return new NifSchema()
                {
                    Document = doc,
                    Schema = schema,
                    SchemasByVersion = schemas,
                };
            }
        }
    }

    public class NIFStreamReader
    {
        public Stream Stream { get; set; }
        public NifSchema Schema { get; set; }

        public string ReadHeaderString()
        {
            var reader = new BlockStructureReader(Schema.Document, Stream);

            return (string)(reader.ReadSchemaByName("HeaderString") as BasicData).Value;
        }
        public int ReadHeaderVersion()
        {
            var headerString = ReadHeaderString();
            var versionString = headerString.Substring(
                headerString
                    .ToList()
                    .FindLastIndex(c => !(char.IsDigit(c) || char.IsWhiteSpace(c) || c == '.')) + 1
                )
                .Trim();
            return VersionParser.Parse(versionString);
        }
        public Header ReadHeader()
        {
            var headerStart = Stream.Position;
            var version = ReadHeaderVersion();

            Stream.Seek(headerStart, SeekOrigin.Begin);
            var reader = new BlockStructureReader(Schema.Document, Stream);
            reader.Version = version;

            return new Header((CompoundData)reader.ReadSchemaByName("Header"));
        }

        public Data[] ReadIndexedBlocks(BlockReadStrategy.TypeIndexed indexStrat, PReader reader)
        {
            var desc = indexStrat.BlockDescriptions;
            var blocks = new Data[desc.Count];
            for (int i = 0; i < desc.Count; i++)
                blocks[i] = reader.ReadBlock(desc[i].Type);
            return blocks;
        }
        public Data[] ReadPrefixedBlocks(BlockReadStrategy.TypePrefixed prefixStrat, PReader reader)
        {
            var blocks = new Data[prefixStrat.BlockCount];
            for (int i = 0; i < prefixStrat.BlockCount; i++)
            {
                var typeData = reader.ReadBlock("Sized String");
                var type = CharList.ReadString(typeData);
                blocks[i] = reader.ReadBlock(type);
            }
            return blocks;
        }
        public PReader GetReader(Header header)
        {
            var key = header.GetVersionKey();
            var strat = header.GetBlockStrategy();
            var doc = Schema.SchemasByVersion[key];
            var reader = new PReader()
            {
                Document = doc,
                Reader = new BinaryReader(Stream),
                BasicReader = new BasicReader(),
            };
            return reader;
        }
        public Data[] ReadBlocks(Header header, PReader reader)
        {
            var strat = header.GetBlockStrategy();
            switch (strat)
            {
                case BlockReadStrategy.TypeIndexed indexStrat:
                    return ReadIndexedBlocks(indexStrat, reader);
                case BlockReadStrategy.TypePrefixed prefixStrat:
                    return ReadPrefixedBlocks(prefixStrat, reader);
                default:
                    throw new NotImplementedException();
            }
        }
        public Footer ReadFooter(PReader reader)
        {
            var data = reader.ReadBlock("Footer") as CompoundData;
            return new Footer(data);
        }

        public NIFFile ReadFile()
        {
            var header = ReadHeader();
            var reader = GetReader(header);
            var blocks = ReadBlocks(header, reader);
            var footer = ReadFooter(reader);
            var file = new NIFFile()
            {
                Header = header,
                Footer = footer,
            };
            var objects = blocks
                .Select(d => ReadNiObject(file, d as BlockData))
                .ToList();
            file.Objects = objects;

            return file;
        }

        public NiObject ReadNiObject(NIFFile file, BlockData data)
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
