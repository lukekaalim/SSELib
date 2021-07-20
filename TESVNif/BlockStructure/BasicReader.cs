using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace SSE.TESVNif.BlockStructure
{
    public static class BasicReader
    {
        public delegate object Reader(BinaryReader r); 

        public static Dictionary<string, Reader> SupportedSchemas = new Dictionary<string, Reader>()
        {
            // If we learn to read more basic types, add them here!
            { "HeaderString", r => ParseLineString(r) },
            { "LineString", r => ParseLineString(r) },

            { "uint64", r => r.ReadUInt64() },
        };

        public static object Read(BinaryReader reader, Schemas.BasicSchema schema)
        {
            switch (schema.Name)
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
                case "bool":
                    return reader.ReadByte();
                case "float":
                    return reader.ReadSingle();
                default:
                    throw new NotImplementedException($"Havent handled type: \"{schema.Name}\"");
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
}
