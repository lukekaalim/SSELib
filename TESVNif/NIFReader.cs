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

    public static class NifSchema
    {
        public static XDocument Source { get; set; }
        public static SchemaDocument Doc { get; set; }

        static NifSchema()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var sourceStream = assembly.GetManifestResourceStream("SSE.TESVNif.NIF.xml"))
            {
                Source = XDocument.Load(sourceStream);
                Doc = new SchemaDocument(Source.Root);
            }
        }
    }

    public class NIFStreamReader : IDisposable
    {
        BinaryReader BinaryReader;
        Reader BlockReader;

        public NIFStreamReader(Stream stream)
        {
            BinaryReader = new BinaryReader(stream);
            BlockReader = new Reader(NifSchema.Doc, ReadBasic, TryReadCompound);
        }

        BasicData ReadBasic(BasicSchema schema, Reader.ReadingContext context)
        {
            return new BasicData(DataReaders.ReadBasicObject(BinaryReader, schema, context));
        }

        bool TryReadCompound(CompoundSchema schema, Reader.ReadingContext context, out Data data)
        {
            data = ReadData();
            return data != null;
            Data ReadData()
            {
                switch (schema.Name)
                {
                    case "SizedString":
                        return new BasicData(DataReaders.ReadSizedString(BinaryReader));
                    default:
                        return null;
                }
            }
        }

        public int ReadHeaderVersion()
        {
            var headerString = DataReaders.ReadLineString(BinaryReader);
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
            var headerStart = BinaryReader.BaseStream.Position;
            var version = ReadHeaderVersion();

            BinaryReader.BaseStream.Seek(headerStart, SeekOrigin.Begin);
            var data = BlockReader.ReadData("Header", new VersionKey(version));

            return new Header(data as CompoundData);
        }
        public Data[] ReadIndexedBlocks(BlockReadStrategy.TypeIndexed strategy)
        {
            var desc = strategy.BlockDescriptions;
            var blocks = new Data[desc.Count];
            for (int i = 0; i < desc.Count; i++)
                blocks[i] = BlockReader.ReadData(desc[i].Type, strategy.Version);
            return blocks;
        }
        public Data[] ReadPrefixedBlocks(BlockReadStrategy.TypePrefixed strategy)
        {
            var blocks = new Data[strategy.BlockCount];
            for (int i = 0; i < strategy.BlockCount; i++)
            {
                var type = DataReaders.ReadSizedString(BinaryReader);
                blocks[i] = BlockReader.ReadData(type, strategy.Version);
            }
            return blocks;
        }
        public Data[] ReadBlocks(Header header)
        {
            var strat = header.GetBlockStrategy();
            switch (strat)
            {
                case BlockReadStrategy.TypeIndexed indexStrat:
                    return ReadIndexedBlocks(indexStrat);
                case BlockReadStrategy.TypePrefixed prefixStrat:
                    return ReadPrefixedBlocks(prefixStrat);
                default:
                    throw new NotImplementedException();
            }
        }

        public Footer ReadFooter(Header header)
        {
            var footerData = BlockReader.ReadData("Footer", header.VersionKey);
            return new Footer(footerData as CompoundData);
        }

        public NIFFile ReadFile()
        {
            var header = ReadHeader();
            var blocks = ReadBlocks(header);
            var footer = ReadFooter(header);
            var file = new NIFFile()
            {
                Header = header,
                Footer = footer,
            };
            var objects = blocks
                .Select(d => ReadNiObject(file, d as NiObjectData))
                .ToList();
            file.Objects = objects;

            return file;
        }

        public NiObject ReadNiObject(NIFFile file, NiObjectData data)
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

        public void Dispose()
        {
            BinaryReader.Dispose();
        }
    }
}
