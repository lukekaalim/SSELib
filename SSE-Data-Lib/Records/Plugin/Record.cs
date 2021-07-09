using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SSE
{
	public partial class SSEPlugin
    {
        public readonly struct Record
		{
            public static uint FixedSize = 4 + 4 + 4 + FormID.Size + VersionControlInfo.Size + 4;
            public uint Size => FixedSize + dataSize;

            public readonly string type;
            public readonly UInt32 dataSize;
            /// <summary>
            /// Flags means different things depending on the record type
            /// </summary>
            [Flags]
            public enum RecordFlags : int
			{
                Master = 0x00000001,
                Compressed = 0x00040000,
            }
            public readonly RecordFlags flags;
            public readonly LocalFormID id;
            public readonly VersionControlInfo versionControlInfo;
            public readonly UInt16 version;
            public readonly UInt16 unknown;
            public readonly List<Field> data;

            public Record(Byte[] bytes, int offset = 0)
            {
                type = Encoding.UTF8.GetString(bytes, offset + 0, 4);
                dataSize = BitConverter.ToUInt32(bytes, offset + 4);
                flags = (RecordFlags)BitConverter.ToUInt32(bytes, offset + 8);
                id = new LocalFormID(bytes, offset + 12);
                versionControlInfo = VersionControlInfo.Parse(bytes, offset + 16);
                version = BitConverter.ToUInt16(bytes, offset + 20);
                unknown = BitConverter.ToUInt16(bytes, offset + 22);
                if ((flags & RecordFlags.Compressed) == RecordFlags.Compressed)
                {
                    data = new List<Field>();
                } else
                {
                    data = Field.ParseAll(bytes, (uint)(offset + FixedSize), dataSize);
                }
			}

            public static async Task<Record> Read(Stream stream)
			{
                Byte[] fixedBytes = new byte[FixedSize];

                await stream.ReadAsync(fixedBytes);

                var dataSize = BitConverter.ToUInt32(fixedBytes, 4);

                Byte[] allBytes = new byte[FixedSize + dataSize];
                Buffer.BlockCopy(fixedBytes, 0, allBytes, 0, fixedBytes.Length);

                await stream.ReadAsync(allBytes, fixedBytes.Length, (int)dataSize);

                return new Record(allBytes);
            }
        }

        /// <summary>
        /// https://en.uesp.net/wiki/Tes5Mod:Mod_File_Format#Fields
        /// </summary>
        public readonly struct Field
        {
            public static uint FixedSize = 4 + 2;
            public uint Size => FixedSize + dataSize;

            public readonly string type; // 4 bytes
            public readonly UInt16 dataSize;
            public readonly Byte[] data;

            public Field(Byte[] bytes, int offset)
			{
                type = Encoding.UTF8.GetString(bytes, offset, 4);
                dataSize = BitConverter.ToUInt16(bytes, offset + 4);
                data = bytes[(offset + (int)FixedSize)..(offset + (int)FixedSize + dataSize)];
            }

            public static List<Field> ParseAll(Byte[] bytes, uint offset, uint length)
            {
                var fields = new List<Field>();
                int index = (int)offset;
                while (index < offset + length)
                {
                    var field = new Field(bytes, index);
                    index += (int)field.Size;
                    fields.Add(field);
                }
                return fields;
            }
        }
    }
}
