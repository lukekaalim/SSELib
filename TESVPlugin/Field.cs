using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSE.TESVPlugin
{
	public struct Field
    {
        public static long FixedSize = 4 + 2;
        public long Size => FixedSize + DataSize;

        public string Type { get; set; }
        public uint DataSize { get; set; }
        public byte[] DataBytes { get; set; }

        public static Field Parse(byte[] fieldBytes, int offset)
        {
            var type = Encoding.UTF8.GetString(fieldBytes, offset, 4);
            var dataSize = BitConverter.ToUInt16(fieldBytes, offset + 4);
            var dataBytes = new byte[dataSize];
			Buffer.BlockCopy(fieldBytes, (int)(offset + FixedSize), dataBytes, 0, dataSize);

            return new Field()
            {
                Type = type,
                DataSize = dataSize,
                DataBytes = dataBytes,
            };
        }

        public static List<Field> ParseAll(Byte[] bytes, int offset, int length)
        {
            var fields = new List<Field>();
            int index = offset;
            while (index < offset + length)
            {
                var field = Parse(bytes, index);
                index += (int)field.Size;
                fields.Add(field);
            }
            return fields;
        }
    }
}
