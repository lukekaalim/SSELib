using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using BlockStructure;
using BlockStructure.Schemas;

namespace SSE.TESVNif
{
    public static class DataReaders
    {
        /*
        public static object ReadBasicObject(BinaryReader reader, BasicSchema schema, DataReader.ReadingContext context)
        {
            switch (schema.Name)
            {
                case "HeaderString":
                case "LineString":
                    return ReadLineString(reader);
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
                    return Half.ToHalf(reader.ReadBytes(2), 0);
                case "short":
                case "BlockTypeIndex":
                    return reader.ReadInt16();
                case "char":
                    return reader.ReadChar();
                case "byte":
                    return reader.ReadByte();
                case "bool":
                    if (context.Version.NifVersion <= NIFVersion.Parse("4.0.0.2"))
                        return reader.ReadInt32();
                    return reader.ReadByte();
                case "float":
                    return reader.ReadSingle();
                default:
                    throw new NotImplementedException($"Havent handled type: \"{schema.Name}\"");
            }
        }

        public static string ReadLineString(BinaryReader reader)
        {
            var content = new List<char>();
            do
            {
                content.Add(reader.ReadChar());
            } while (content.Last() != 0x0A);
            return new string(content.ToArray());
        }

        public static string ReadSizedString(BinaryReader reader)
        {
            var length = reader.ReadUInt32();
            return new string(reader.ReadChars((int)length));
        }
        */
    }
}
