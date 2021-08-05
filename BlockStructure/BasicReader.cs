using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

using BlockStructure.Schemas;

namespace BlockStructure
{
    /*
    public class BasicReader
    {
        public int? Version { get; set; }

        public delegate object Reader(BinaryReader r); 

        public static Dictionary<string, Reader> SupportedSchemas = new Dictionary<string, Reader>()
        {
            // If we learn to read more basic types, add them here!
            { "HeaderString", r => ParseLineString(r) },
            { "LineString", r => ParseLineString(r) },

            { "uint64", r => r.ReadUInt64() },
        };
        public virtual Data Read(BinaryReader reader, Schemas.BasicSchema schema)
        {
            return new BasicData(Read(reader, schema.Name));
        }

        public object Read(BinaryReader reader, string schemaName)
        {
            switch (schemaName)
            {
                case "HeaderString":
                case "LineString":
                    return ParseLineString(reader);
                case "uint64":
                    return reader.ReadUInt64();
                case "int64":
                    return reader.ReadInt64();
                case "int":
                case "Ptr":
                case "Ref":
                case "StringOffset":
                case "NiFixedString":
                case "FileVersion":
                    return reader.ReadInt32();
                case "uint":
                case "ulittle32":
                    return reader.ReadUInt32();
                case "ushort":
                    return reader.ReadUInt16();
                case "hfloat":
                    return reader.ReadInt16();
                case "short":
                case "BlockTypeIndex":
                    return reader.ReadInt16();
                case "char":
                    return reader.ReadChar();
                case "byte":
                    return reader.ReadByte();
                case "bool":
                    if (Version == null || Version <= VersionParser.Parse("4.0.0.2"))
                        return reader.ReadInt32();
                    return reader.ReadByte();
                case "float":
                    return reader.ReadSingle();
                default:
                    throw new NotImplementedException($"Havent handled type: \"{schemaName}\"");
            }
        }

        public static string ParseLineString(BinaryReader reader)
        {
            var content = new List<char>() { (char)reader.ReadByte() };
            while (content[content.Count - 1] != 0x0A)
            {
                content.Add((char)reader.ReadByte());
            }
            return new string(content.ToArray());
        }
    }
    */
}
